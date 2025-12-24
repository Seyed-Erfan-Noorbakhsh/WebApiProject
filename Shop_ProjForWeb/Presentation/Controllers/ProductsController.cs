namespace Shop_ProjForWeb.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ProductImageService _imageService;

    public ProductsController(IProductRepository productRepository, ProductImageService imageService)
    {
        _productRepository = productRepository;
        _imageService = imageService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(product));
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAllProducts()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products.Select(MapToDto).ToList());
    }

    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadProductImage(Guid id, IFormFile file)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        try
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                _imageService.DeleteImage(product.ImageUrl);
            }

            // Upload new image
            var imagePath = await _imageService.UploadImageAsync(file, id);
            product.ImageUrl = imagePath;

            await _productRepository.UpdateAsync(product);

            return Ok(new { message = "Image uploaded successfully", imageUrl = imagePath });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static ProductDto MapToDto(Shop_ProjForWeb.Core.Domain.Entities.Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            BasePrice = product.BasePrice,
            DiscountPercent = product.DiscountPercent,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt
        };
    }
}
