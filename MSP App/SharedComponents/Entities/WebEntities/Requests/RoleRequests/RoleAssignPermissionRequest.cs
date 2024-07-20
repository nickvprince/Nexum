
namespace SharedComponents.Entities.WebEntities.Requests.RoleRequests
{
    public class RoleAssignPermissionRequest
    {
        public string? RoleId { get; set; }
        public int PermissionId { get; set; }
        public int TenantId { get; set; }
    }
}
