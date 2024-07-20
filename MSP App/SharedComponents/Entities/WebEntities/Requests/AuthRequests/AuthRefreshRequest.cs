
namespace SharedComponents.Entities.WebEntities.Requests.AuthRequests
{
    public class AuthRefreshRequest
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
