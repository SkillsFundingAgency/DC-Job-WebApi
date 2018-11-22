using ESFA.DC.JobNotifications;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class NotifierConfig : INotifierConfig
    {
        [JsonRequired]
        public string ApiKey { get; set; }
    }
}
