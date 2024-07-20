
namespace SharedComponents.Entities.TenantServerHttpEntities.Requests
{
    public class UpdateNASServerRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
    }
}
