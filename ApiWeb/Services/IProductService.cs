using ApiWeb.DTOs.Products;

namespace ApiWeb.Services.Products;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryParams queryParams, CancellationToken ct);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}