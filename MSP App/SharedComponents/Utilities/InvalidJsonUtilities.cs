using Newtonsoft.Json;
using SharedComponents.RequestEntities.Http;

namespace SharedComponents.Utilities
{
    public class InvalidJsonUtilities : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CreateJobRequest);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
                return;
            }
            var createJobRequest = (CreateJobRequest)value;

            writer.WriteStartObject();

            writer.WritePropertyName("client_id");
            writer.WriteValue(createJobRequest.Client_Id);

            //writer.WritePropertyName("jobs");
            //writer.WriteStartObject();
            if (createJobRequest.Jobs != null)
            {
                foreach (var job in createJobRequest.Jobs)
                {
                    if (job.Settings != null)
                    {
                        writer.WritePropertyName("jobs");
                        writer.WritePropertyName("client_id");
                        writer.WritePropertyName(job.Title);
                        serializer.Serialize(writer, job);
                    }
                }
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
