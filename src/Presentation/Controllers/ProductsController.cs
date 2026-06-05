namespace PokeStore.Api.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Application.Services;

/// <summary>
/// Controller for product catalog endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>List of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ProductListResponseDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetProductsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    /// <summary>
    /// Search products by query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Search results</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ProductListResponseDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Search query cannot be empty" });

        var result = await _productService.SearchProductsAsync(query, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Products in category</returns>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(ProductListResponseDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetProductsByCategoryAsync(categoryId, pageNumber, pageSize);
        return Ok(result);
    }
}
