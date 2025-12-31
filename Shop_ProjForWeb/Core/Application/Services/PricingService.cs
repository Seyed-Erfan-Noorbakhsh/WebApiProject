namespace Shop_ProjForWeb.Core.Application.Services;

public class PricingService
{
    private const decimal VipDiscountPercent = 10m;

    public decimal CalculateFinalPrice(decimal basePrice, int productDiscountPercent, bool isVip)
    {
        decimal priceAfterProductDiscount = basePrice * (1 - productDiscountPercent / 100m);

        if (isVip)
        {
            priceAfterProductDiscount = priceAfterProductDiscount * (1 - VipDiscountPercent / 100m);
        }

        return priceAfterProductDiscount;
    }
}
