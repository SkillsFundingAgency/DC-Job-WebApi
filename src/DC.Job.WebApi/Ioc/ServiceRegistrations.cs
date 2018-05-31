using System;
using System.Collections.Generic;
using Autofac;
using ESFA.DC.ILR.ValidationErrors;
using ESFA.DC.ILR.ValidationErrors.Interface;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.Job.WebApi.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobQueueManager.JobQueueManager>().As<IJobQueueManager>().InstancePerLifetimeScope();
            builder.RegisterType<ValidationErrorsService>().As<IValidationErrorsService>().InstancePerLifetimeScope();
            builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();
            builder.RegisterType<AzureStorageKeyValuePersistenceService>().As<IKeyValuePersistenceService>().InstancePerLifetimeScope();
            builder.RegisterType<KeyGenerator.KeyGenerator>().As<IKeyGenerator>().InstancePerLifetimeScope();

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