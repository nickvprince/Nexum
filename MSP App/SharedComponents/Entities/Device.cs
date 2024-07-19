using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities
{
    public class Device
    {
        public int Id { get; set; }
        [Required]
        public DeviceInfo? DeviceInfo { get; set; }
        public DeviceStatus? Status { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public ICollection<DeviceAlert>? Alerts { get; set; }
        public ICollection<DeviceLog>? Logs { get; set; }
        public ICollection<DeviceJob>? Jobs { get; set; }
        public ICollection<DeviceBackup>? Backups { get; set; }
        [Required]
        public int TenantId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Tenant? Tenant { get; set; }
    }

    public enum DeviceStatus
    {
        Offline,
        Online,
        ServiceOffline,
        BackupInProgress,
        RestoreInProgress
    }
}
