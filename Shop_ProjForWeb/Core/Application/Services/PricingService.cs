namespace Shop_ProjForWeb.Core.Application.Services;

using Shop_ProjForWeb.Core.Domain.Interfaces;

public class PricingService(IDiscountCalculator discountCalculator)
{
    private readonly IDiscountCalculator _discountCalculator = discountCalculator;

    public decimal CalculateFinalPrice(decimal basePrice, int productDiscountPercent, bool isVip)
    {
        return _discountCalculator.CalculateFinalPrice(basePrice, productDiscountPercent, isVip);
    }

    /// <summary>
    /// Calculates final price and returns both discount percentages for audit trail.
    /// </summary>
    public (decimal FinalPrice, int ProductDiscount, int VipDiscount) CalculateFinalPriceWithDiscounts(
        decimal basePrice, int productDiscountPercent, bool isVip)
    {
        var breakdown = _discountCalculator.GetDiscountBreakdown(basePrice, productDiscountPercent, isVip);
        
        return (
            breakdown.FinalPrice, 
            (int)breakdown.ProductDiscountPercent, 
            (int)breakdown.VipDiscountPercent
        );
    }

    /// <summary>
    /// Gets detailed discount breakdown for transparency and debugging.
    /// </summary>
    public DiscountBreakdown GetDiscountBreakdown(decimal basePrice, int productDiscountPercent, bool isVip)
    {
        return _discountCalculator.GetDiscountBreakdown(basePrice, productDiscountPercent, isVip);
    }
}
