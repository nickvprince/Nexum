
namespace SharedComponents.Entities.WebEntities.Requests.NASServerRequests
{
    public class NASServerUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NASUsername { get; set; }
        public string? NASPassword { get; set; }
    }
}
