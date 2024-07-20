
namespace SharedComponents.Entities.WebEntities.Responses.AuthResponses
{
    public class AuthLoginResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? Expires { get; set; }
    }
}
