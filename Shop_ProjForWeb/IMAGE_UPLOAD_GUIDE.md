# Product Image Upload & Retrieval Guide

## Overview

This is a minimal, test-only implementation of image upload and retrieval for Product entities. It follows Clean Architecture principles and integrates seamlessly with the existing codebase.

## Architecture

### Layers

1. **Domain Layer** (`Core/Domain/Entities`)
   - `Product.cs` - Entity with nullable `ImageUrl` property

2. **Application Layer** (`Core/Application`)
   - `Services/ProductImageService.cs` - Core image handling logic
   - `Configuration/FileUploadOptions.cs` - Configuration options
   - `DTOs/ProductDto.cs` - Data transfer object for API responses
   - `Interfaces/IProductRepository.cs` - Extended with `GetAllAsync()` and `UpdateAsync()`

3. **Infrastructure Layer** (`Infrastructure/Repositories`)
   - `ProductRepository.cs` - Updated with new methods

4. **Presentation Layer** (`Presentation/Controllers`)
   - `ProductsController.cs` - API endpoints for image operations

## Configuration

### appsettings.json

```json
{
  "FileUpload": {
    "MaxFileSizeBytes": 5242880,
    "UploadFolder": "UploadedFiles"
  }
}
```

- `MaxFileSizeBytes`: Maximum file size (default: 5MB)
- `UploadFolder`: Relative path where images are stored

## API Endpoints

### 1. Get Product by ID

```
GET /api/products/{id}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Product Name",
  "basePrice": 99.99,
  "discountPercent": 10,
  "isActive": true,
  "imageUrl": "UploadedFiles/550e8400-e29b-41d4-a716-446655440000_guid.jpg",
  "createdAt": "2025-12-24T10:00:00Z"
}
```

### 2. Get All Products

```
GET /api/products
```

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Product 1",
    "basePrice": 99.99,
    "discountPercent": 10,
    "isActive": true,
    "imageUrl": "UploadedFiles/550e8400-e29b-41d4-a716-446655440000_guid.jpg",
    "createdAt": "2025-12-24T10:00:00Z"
  }
]
```

### 3. Upload Product Image

```
POST /api/products/{id}/image
Content-Type: multipart/form-data

file: <binary image data>
```

**Response:**
```json
{
  "message": "Image uploaded successfully",
  "imageUrl": "UploadedFiles/550e8400-e29b-41d4-a716-446655440000_guid.jpg"
}
```

**Error Response (file too large):**
```json
{
  "error": "File size exceeds maximum allowed size of 5242880 bytes"
}
```

## File Storage

- **Location**: `UploadedFiles/` directory (relative to application root)
- **Naming**: `{productId}_{uniqueGuid}{extension}`
- **Access**: Files are served via static file middleware at `http://localhost:5000/UploadedFiles/...`

## Usage Example

### Using cURL

```bash
# Get product
curl http://localhost:5000/api/products/550e8400-e29b-41d4-a716-446655440000

# Upload image
curl -X POST http://localhost:5000/api/products/550e8400-e29b-41d4-a716-446655440000/image \
  -F "file=@/path/to/image.jpg"

# Access image in browser
http://localhost:5000/UploadedFiles/550e8400-e29b-41d4-a716-446655440000_guid.jpg
```

### Using Swagger UI

1. Navigate to `http://localhost:5000/swagger`
2. Expand the `Products` section
3. Use the "Try it out" button to test endpoints

## Implementation Details

### ProductImageService

Handles all image operations:

- **UploadImageAsync**: Validates file, saves to disk, returns relative path
- **DeleteImage**: Removes old image when replaced

### File Validation

- Checks file is not empty
- Validates file size against `MaxFileSizeBytes`
- Generates unique filename to prevent collisions

### Database Integration

- Image path stored in `Product.ImageUrl` (nullable string)
- No image processing or resizing
- Relative paths allow easy migration/deployment

## Constraints & Limitations

- ✅ No authentication/authorization
- ✅ No image processing or validation beyond size check
- ✅ No image format validation (accepts any file type)
- ✅ Simple file naming strategy
- ✅ Files stored locally (not cloud storage)
- ✅ No image deletion on product deletion

## Future Enhancements

If this were to be production-ready:

1. Add image format validation (JPEG, PNG, WebP only)
2. Add image resizing/thumbnail generation
3. Implement cloud storage (Azure Blob, S3)
4. Add authentication/authorization
5. Add image deletion on product deletion
6. Add image metadata (size, dimensions)
7. Add virus scanning
8. Add CDN integration
9. Add image compression
10. Add rate limiting

## Testing

### Manual Testing

1. Start the application: `dotnet run --project Shop_ProjForWeb`
2. Navigate to Swagger: `http://localhost:5000/swagger`
3. Test endpoints using Swagger UI

### Test Scenarios

1. Upload image to product without existing image
2. Upload image to product with existing image (should replace)
3. Upload file exceeding size limit (should fail)
4. Upload to non-existent product (should return 404)
5. Retrieve product with image URL
6. Access image via browser using returned URL

## File Structure

```
Shop_ProjForWeb/
├── Core/
│   └── Application/
│       ├── Configuration/
│       │   └── FileUploadOptions.cs
│       ├── DTOs/
│       │   └── ProductDto.cs
│       ├── Interfaces/
│       │   └── IProductRepository.cs (updated)
│       └── Services/
│           └── ProductImageService.cs
├── Infrastructure/
│   └── Repositories/
│       └── ProductRepository.cs (updated)
├── Presentation/
│   └── Controllers/
│       └── ProductsController.cs
├── UploadedFiles/          (created at runtime)
├── wwwroot/                (for static files)
├── Program.cs              (updated)
└── appsettings.json        (updated)
```

## Notes

- Images are stored in `UploadedFiles/` directory
- Static file serving is enabled via `app.UseStaticFiles()`
- Configuration is read from `appsettings.json`
- All paths use forward slashes for cross-platform compatibility
- Old images are automatically deleted when replaced
