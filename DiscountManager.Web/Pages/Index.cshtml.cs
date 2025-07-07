using DiscountManagerWeb.ViewModels;
using DiscountManager.Core;
using Google.Protobuf;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using static DiscountManager.Core.Discount;

namespace DiscountManagerWeb.Pages;

public class IndexModel(ILogger<IndexModel> logger, GrpcClientFactory grpcClientFactory) : PageModel
{
    private readonly ILogger<IndexModel> logger = logger;
    private readonly DiscountClient discountClient = grpcClientFactory.CreateClient<DiscountClient>("Discount");

    [BindProperty]
    public GenerateDiscountCodeViewModel GenerateDiscountCode { get; set; } = default!;

    [BindProperty]
    public DiscountCodeViewModel DiscountCode { get; set; } = default!;

    [BindProperty]
    public string Code { get; set; } = default!;

    [BindProperty]
    public IEnumerable<string> Codes { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var response = await discountClient.GetCodesAsync(new GetCodesRequest());

        var ms = new MemoryStream(response.Result.ToByteArray());

        var codes = await JsonSerializer.DeserializeAsync<List<string>>(ms);

        if (codes is null || !codes.Any())
        {
            return Page();
        }

        Codes = codes.ToArray();

        return Page();
    }

    public async Task<IActionResult> OnPostGenerateCodeAsync()
    {
        logger.LogInformation("Receiving new request...");

        if (!ModelState.Where(kv => kv.Key.Contains(nameof(GenerateDiscountCode))).Any())
        {
            return Page();
        }

        var bytes = BitConverter.GetBytes(GenerateDiscountCode.Length);

        var request = new CreateCodesRequest { Count = GenerateDiscountCode.Count, Length = ByteString.CopyFrom(bytes) };

        var response = await discountClient.CreateCodesAsync(request);

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostCheckCodeAsync()
    {
        logger.LogInformation("Receiving new request...");

        if (string.IsNullOrEmpty(Code))
        {
            return await OnGet();
        }

        var request = new UseCodeRequest { Code = Code };

        var response = await discountClient.UseCodeAsync(request);

        await using var ms = new MemoryStream(response.Result.ToByteArray());
        ms.Position = 0;

        DiscountCode = await JsonSerializer.DeserializeAsync<DiscountCodeViewModel>(ms);

        return await OnGet();
    }
}
