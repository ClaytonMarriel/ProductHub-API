using ApiWeb.Data;
using ApiWeb.DTOs.Products;
using ApiWeb.Exceptions;
using ApiWeb.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Services.Products;

public sealed class ProductService : IProductService
{
    private readonly AppDbContent _context;

    public ProductService(AppDbContent context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryParams queryParams, CancellationToken ct)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.Trim().ToLower();

            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search) ||
                p.BarCode.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Mark))
        {
            var mark = queryParams.Mark.Trim().ToLower();
            query = query.Where(p => p.Mark.ToLower() == mark);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(ct);

        var responseItems = items.Select(p => p.ToResponse()).ToList();

        return new PagedResponse<ProductResponse>(
            responseItems,
            queryParams.PageNumber,
            queryParams.PageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)queryParams.PageSize)
        );
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        return product?.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var barcodeExists = await _context.Products
            .AnyAsync(p => p.BarCode == request.BarCode && !p.IsDeleted, ct);

        if (barcodeExists)
            throw new BusinessRuleException("Já existe um produto com esse código de barras.");

        var entity = request.ToEntity();

        await _context.Products.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (entity is null)
            return null;

        var duplicatedMarkBarcode = await _context.Products.AnyAsync(
            p => p.Id != id &&
                 p.BarCode == entity.BarCode &&
                 !p.IsDeleted,
            ct);

        if (duplicatedMarkBarcode)
            throw new BusinessRuleException("Já existe outro produto com esse código de barras.");

        entity.ApplyUpdate(request);

        await _context.SaveChangesAsync(ct);
        return entity.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (entity is null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}