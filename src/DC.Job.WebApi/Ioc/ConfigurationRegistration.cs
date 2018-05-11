﻿using Autofac;
using ESFA.DC.Job.WebApi.Extensions;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ESFA.DC.Job.WebApi.Ioc
{
    public static class ConfigurationRegistration
    {
        public static void SetupConfigurations(this ContainerBuilder builder, IConfiguration configuration)
        {
            builder.Register(c => configuration.GetConfigSection<JobQueueManagerSettings>())
                .As<IJobQueueManagerSettings>().SingleInstance();
        }
    }
}