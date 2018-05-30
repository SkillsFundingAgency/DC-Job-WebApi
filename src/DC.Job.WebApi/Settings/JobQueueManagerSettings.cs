using ESFA.DC.JobQueueManager.Interfaces;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class JobQueueManagerSettings
    {
        [JsonRequired]
        public string ConnectionString { get; set; }
    }
}
