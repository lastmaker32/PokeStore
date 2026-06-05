namespace PokeStore.Api.Application.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CategoryListResponseDTO
{
    public List<CategoryDTO> Categories { get; set; } = new();
}
