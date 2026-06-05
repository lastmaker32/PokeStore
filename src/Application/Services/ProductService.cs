namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for product operations
/// </summary>
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDTO?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : MapToDTO(product);
    }

    public async Task<ProductListResponseDTO> GetProductsAsync(int pageNumber = 1, int pageSize = 20)
    {
        var products = await _repository.GetAllAsync(pageNumber, pageSize);
        var totalCount = await GetTotalProductCountAsync();

        return new ProductListResponseDTO
        {
            Products = products.Select(MapToDTO).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductListResponseDTO> GetProductsByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 20)
    {
        var products = await _repository.GetByCategoryIdAsync(categoryId, pageNumber, pageSize);
        var totalCount = await GetProductCountByCategoryAsync(categoryId);

        return new ProductListResponseDTO
        {
            Products = products.Select(MapToDTO).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductListResponseDTO> SearchProductsAsync(string query, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new ProductListResponseDTO();

        var products = await _repository.SearchAsync(query, pageNumber, pageSize);
        return new ProductListResponseDTO
        {
            Products = products.Select(MapToDTO).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private ProductDTO MapToDTO(Product product)
    {
        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            AvailableQuantity = product.AvailableQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            IsActive = product.IsActive
        };
    }

    private Task<int> GetTotalProductCountAsync()
    {
        // This would normally query the database to get total count
        // For now, returning 0 as placeholder
        return Task.FromResult(0);
    }

    private Task<int> GetProductCountByCategoryAsync(int categoryId)
    {
        // This would normally query the database to get category product count
        return Task.FromResult(0);
    }
}
