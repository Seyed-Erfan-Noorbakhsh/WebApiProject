namespace Shop_ProjForWeb.Core.Domain.Interfaces;

public interface IDiscountCalculator
{
    decimal CalculateFinalPrice(decimal basePrice, decimal productDiscountPercent, bool isVip);
    DiscountBreakdown GetDiscountBreakdown(decimal basePrice, decimal productDiscountPercent, bool isVip);
}

public class DiscountBreakdown
{
    public decimal BasePrice { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal VipDiscountAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal ProductDiscountPercent { get; set; }
    public decimal VipDiscountPercent { get; set; }
    public decimal EffectiveDiscountPercent { get; set; }
}