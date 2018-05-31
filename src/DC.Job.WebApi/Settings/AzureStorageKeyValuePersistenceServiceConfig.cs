using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Settings
{
    public class AzureStorageKeyValuePersistenceServiceConfig : IAzureStorageKeyValuePersistenceServiceConfig
    {
        [JsonRequired]
        public string ConnectionString { get; set; }

        [JsonRequired]
        public string ContainerName { get; set; }
    }
}
