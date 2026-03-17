using ApiWeb.DTOs.Products;
using ApiWeb.Models;

namespace ApiWeb.Mappings
{
    public static class ProductMappingExtensions
    {
        public static ProductModel ToEntity(this CreateProductRequest request)
        {
            return new ProductModel
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                QuantityStock = request.QuantityStock,
                BarCode = request.BarCode.Trim(),
                Mark = request.Mark.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ProductResponse ToResponse(this ProductModel entity)
        {
            return new ProductResponse(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.QuantityStock,
                entity.BarCode,
                entity.Mark,
                entity.CreatedAt,
                entity.UpdatedAt
            );
        }

        public static void ApplyUpdate(this ProductModel entity, UpdateProductRequest request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = request.Description.Trim();
            entity.QuantityStock = request.QuantityStock;
            entity.Mark = request.Mark.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}