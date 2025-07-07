using DiscountManager.Core.Enums;

namespace DiscountManager.Core.Models;

public class DiscountCode
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required State State { get; set; }
}
