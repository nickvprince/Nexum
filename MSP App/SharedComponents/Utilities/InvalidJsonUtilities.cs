using Newtonsoft.Json;
using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Utilities
{
    public class InvalidJsonUtilities : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DeviceJob);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
                return;
            }
            var job = (DeviceJob)value;

            writer.WriteStartObject();
            if (job.Device != null)
            {
                if (job.Device.DeviceInfo != null)
                {
                    writer.WritePropertyName("client_id");
                    writer.WriteValue(job.Device.DeviceInfo.ClientId);
                }
            }
            if (job.Settings != null && job.Name != null)
            {
                if (job.Settings.Schedule != null)
                {
                    writer.WritePropertyName(job.JobId.ToString());
                    writer.WriteStartObject();
                    writer.WritePropertyName("title");
                    writer.WriteValue(job.Name);
                    writer.WritePropertyName("settings");
                    writer.WriteStartObject();
                    writer.WritePropertyName("schedule");
                    writer.WriteValue(ScheduleUtilities.ConvertToTenantServerFormat(job.Settings.Schedule));
                    writer.WritePropertyName("startTime");
                    writer.WriteValue(job.Settings.StartTime);
                    writer.WritePropertyName("stopTime");
                    writer.WriteValue(job.Settings.EndTime);
                    writer.WritePropertyName("retryCount");
                    writer.WriteValue(job.Settings.retryCount);
                    writer.WritePropertyName("sampling");
                    writer.WriteValue(job.Settings.Sampling);
                    writer.WritePropertyName("heartbeat_interval");
                    writer.WriteValue(job.Settings.UpdateInterval);
                    writer.WritePropertyName("retention");
                    writer.WriteValue(job.Settings.Retention);
                    writer.WritePropertyName("id");
                    writer.WriteValue(job.Settings.BackupServerId);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndObject();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
