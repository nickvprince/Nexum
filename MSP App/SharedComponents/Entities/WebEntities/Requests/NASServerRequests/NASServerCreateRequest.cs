
namespace SharedComponents.Entities.WebEntities.Requests.NASServerRequests
{
    public class NASServerCreateRequest
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
        public int TenantId { get; set; }
    }
}
