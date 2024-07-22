
namespace SharedComponents.Entities.TenantServerHttpEntities.Requests
{
    public class UpdateNASServerRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
