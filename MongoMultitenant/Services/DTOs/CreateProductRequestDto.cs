namespace MongoMultitenant.Services.DTOs
{
    public record CreateProductRequestDto(string Name, string Description, int Price, int Stock);
}
