using System;
using System.Collections.Generic;
using Autofac;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.Job.WebApi.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobQueueManager.JobQueueManager>().As<IJobQueueManager>().InstancePerLifetimeScope();
            builder.Register(context =>
                {
                    var queueManagerSettings = context.Resolve<JobQueueManagerSettings>();
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlServer(
                        queueManagerSettings.ConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions>()
                .SingleInstance();
        }
    }
}