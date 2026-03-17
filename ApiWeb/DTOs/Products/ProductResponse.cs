namespace ApiWeb.DTOs.Products;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    int QuantityStock,
    string BarCode,
    string Mark,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);