namespace Shop_ProjForWeb.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Application.Services;
using Shop_ProjForWeb.Core.Domain.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductService productService,
    IProductRepository productRepository,
    ProductImageService imageService,
    IValidationService validationService) : ControllerBase
{
    private readonly IProductService _productService = productService;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ProductImageService _imageService = imageService;
    private readonly IValidationService _validationService = validationService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetAllProducts([FromQuery] PaginatedRequest request)
    {
        var products = await _productService.GetAllProductsAsync();
        
        // Apply sorting
        var sortedProducts = request.SortBy.ToLower() switch
        {
            "name" => request.SortDescending ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name),
            "price" => request.SortDescending ? products.OrderByDescending(p => p.BasePrice) : products.OrderBy(p => p.BasePrice),
            "createdat" => request.SortDescending ? products.OrderByDescending(p => p.CreatedAt) : products.OrderBy(p => p.CreatedAt),
            _ => request.SortDescending ? products.OrderByDescending(p => p.Id) : products.OrderBy(p => p.Id)
        };

        // Apply pagination
        var totalCount = sortedProducts.Count();
        var pagedProducts = sortedProducts
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = new PaginatedResponse<ProductDto>
        {
            Items = pagedProducts,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductAsync(id);
        return Ok(product);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ProductDto>>> GetActiveProducts()
    {
        try
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return Ok(products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                BasePrice = p.BasePrice,
                DiscountPercent = p.DiscountPercent,
                IsActive = p.IsActive,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            }).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving active products", details = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductDto>>> SearchProducts([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = "Search name cannot be empty" });
            }

            var products = await _productService.SearchProductsAsync(name);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while searching products", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        // Use ValidationService for consistent validation
        var validationResult = await _validationService.ValidateBusinessRulesAsync(dto, "create");
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
            return BadRequest(new { error = "Validation failed", validationErrors = errors });
        }

        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        // Use ValidationService for consistent validation
        var validationResult = await _validationService.ValidateBusinessRulesAsync(dto, "update");
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
            return BadRequest(new { error = "Validation failed", validationErrors = errors });
        }

        await _productService.UpdateProductAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while deleting the product", details = ex.Message });
        }
    }

    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadProductImage(Guid id, IFormFile file)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { error = "Product not found" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file provided" });
            }

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
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while uploading the image", details = ex.Message });
        }
    }
}
