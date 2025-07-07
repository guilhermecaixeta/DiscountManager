using DiscountManager.Core;
using DiscountManager.Core.Services;
using DiscountManager.DTO;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DiscountManager.Api.Services;

public class DiscountService(
    DiscountDbContext dbContext,
    IMessageProducer<IEnumerable<string>> messageProducer,
    ICodeGeneratorService codeGeneratorService,
    ILogger<DiscountService> logger) : Discount.DiscountBase
{
    public override async Task<GetCodesReply> GetCodes(GetCodesRequest request, ServerCallContext context)
    {
        var codes = await dbContext.DiscountCodes
            .Where(dc => dc.State == Core.Enums.State.Active)
            .Take(50)
            .Select(dc => dc.Code)
            .OrderBy(dc => dc)
            .AsNoTracking()
            .ToListAsync();

        await using var ms = new MemoryStream();

        await JsonSerializer.SerializeAsync(ms, codes, typeof(List<string>));

        return new GetCodesReply { Result = ByteString.CopyFrom(ms.ToArray()) };
    }

    public override async Task<CreatedCodesReply> CreateCodes(CreateCodesRequest request, ServerCallContext context)
    {
        try
        {
            var codeLength = BitConverter.ToInt16(request.Length.ToByteArray());

            logger.LogInformation($"Generating {request.Count} codes with length {codeLength}");

            var codes = codeGeneratorService.GenerateCodes((int)request.Count, codeLength).ToList().Distinct();

            await messageProducer.PublishAsync(codes, context.CancellationToken);

            return new CreatedCodesReply { Result = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);

            return new CreatedCodesReply { Result = false };
        }
    }

    public override async Task<UseCodeReply> UseCode(UseCodeRequest request, ServerCallContext context)
    {
        var discountCode = await dbContext
            .DiscountCodes
            .SingleOrDefaultAsync(dc => dc.Code == request.Code, context.CancellationToken);

        if (discountCode != null)
        {
            discountCode.State = Core.Enums.State.Used;

            dbContext.DiscountCodes.Update(discountCode);

            await dbContext.SaveChangesAsync();
        }

        var discountCodeDTO = (discountCode == null) switch
        {
            true => new DiscountCodeDTO { Code = string.Empty, State = "NOT_FOUND" },
            _ => new DiscountCodeDTO { Code = discountCode.Code, State = GetState(discountCode.State) }
        };

        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, discountCodeDTO, typeof(DiscountCodeDTO));
        return new UseCodeReply { Result = ByteString.CopyFrom(ms.ToArray()) };
    }

    private string GetState(Core.Enums.State state) =>
        state switch { Core.Enums.State.Active => "ACTIVE", Core.Enums.State.Used => "CODE_USED", _ => "CODE_UNKNOWN" };
}
