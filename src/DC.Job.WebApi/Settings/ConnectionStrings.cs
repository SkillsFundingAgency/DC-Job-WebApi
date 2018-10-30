using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class ConnectionStrings
    {
        [JsonRequired]
        public string AppLogs { get; set; }
    }
}
