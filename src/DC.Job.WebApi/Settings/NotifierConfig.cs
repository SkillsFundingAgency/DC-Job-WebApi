using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.JobNotifications;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class NotifierConfig : INotifierConfig
    {
        [JsonRequired]
        public string ApiKey { get; set; }

        [JsonRequired]
        public string ReplyToEmailAddress { get; set; }
    }
}
