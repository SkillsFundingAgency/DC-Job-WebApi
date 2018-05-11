using ESFA.DC.JobQueueManager.Interfaces;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class JobQueueManagerSettings : IJobQueueManagerSettings
    {
        [JsonRequired]
        public string ConnectionString { get; set; }
    }
}
