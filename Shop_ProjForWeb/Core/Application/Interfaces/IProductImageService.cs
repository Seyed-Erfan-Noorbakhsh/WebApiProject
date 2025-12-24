namespace Shop_ProjForWeb.Core.Application.Interfaces;

public interface IProductImageService
{
    Task<string> UploadImageAsync(IFormFile file, Guid productId);
    void DeleteImage(string? imagePath);
}
