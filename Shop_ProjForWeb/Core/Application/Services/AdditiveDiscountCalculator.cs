using Shop_ProjForWeb.Core.Domain.Interfaces;

namespace Shop_ProjForWeb.Core.Application.Services;

public class AdditiveDiscountCalculator : IDiscountCalculator
{
    private const decimal VipDiscountPercent = 10m;

    public decimal CalculateFinalPrice(decimal basePrice, decimal productDiscountPercent, bool isVip)
    {
        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative");
        
        if (productDiscountPercent < 0 || productDiscountPercent > 100)
            throw new ArgumentException("Product discount percent must be between 0 and 100");

        // Calculate discount amounts based on base price (additive approach)
        var productDiscountAmount = basePrice * (productDiscountPercent / 100m);
        var vipDiscountAmount = isVip ? basePrice * (VipDiscountPercent / 100m) : 0m;
        
        // Total discount is sum of both discounts
        var totalDiscountAmount = productDiscountAmount + vipDiscountAmount;
        
        // Ensure total discount never exceeds 100% of base price
        totalDiscountAmount = Math.Min(totalDiscountAmount, basePrice);
        
        var finalPrice = basePrice - totalDiscountAmount;
        
        // Round to 2 decimal places
        return Math.Round(finalPrice, 2);
    }

    public DiscountBreakdown GetDiscountBreakdown(decimal basePrice, decimal productDiscountPercent, bool isVip)
    {
        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative");
        
        if (productDiscountPercent < 0 || productDiscountPercent > 100)
            throw new ArgumentException("Product discount percent must be between 0 and 100");

        var productDiscountAmount = basePrice * (productDiscountPercent / 100m);
        var vipDiscountAmount = isVip ? basePrice * (VipDiscountPercent / 100m) : 0m;
        var totalDiscountAmount = Math.Min(productDiscountAmount + vipDiscountAmount, basePrice);
        var finalPrice = Math.Round(basePrice - totalDiscountAmount, 2);
        var effectiveDiscountPercent = basePrice > 0 ? (totalDiscountAmount / basePrice) * 100m : 0m;

        return new DiscountBreakdown
        {
            BasePrice = basePrice,
            ProductDiscountAmount = Math.Round(productDiscountAmount, 2),
            VipDiscountAmount = Math.Round(vipDiscountAmount, 2),
            TotalDiscountAmount = Math.Round(totalDiscountAmount, 2),
            FinalPrice = finalPrice,
            ProductDiscountPercent = productDiscountPercent,
            VipDiscountPercent = isVip ? VipDiscountPercent : 0m,
            EffectiveDiscountPercent = Math.Round(effectiveDiscountPercent, 2)
        };
    }
}