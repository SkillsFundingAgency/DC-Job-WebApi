using System;
using System.Collections.Generic;
using Autofac;
using ESFA.DC.CollectionsManagement.Services;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobNotifications;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.Job.WebApi.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobManager>().As<IJobManager>().InstancePerLifetimeScope();
            builder.RegisterType<FileUploadJobManager>().As<IFileUploadJobManager>().InstancePerLifetimeScope();
            builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();
            builder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<EmailNotifier>().As<IEmailNotifier>().InstancePerLifetimeScope();
            builder.RegisterType<EmailTemplateManager>().As<IEmailTemplateManager>().InstancePerLifetimeScope();
            builder.RegisterType<ReturnCalendarService>().As<IReturnCalendarService>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationService>().As<IOrganisationService>().InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<IAzureStorageKeyValuePersistenceServiceConfig>(JobType.IlrSubmission);
                return new AzureStorageKeyValuePersistenceService(config);
            }).Keyed<IKeyValuePersistenceService>(JobType.IlrSubmission).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<IAzureStorageKeyValuePersistenceServiceConfig>(JobType.EsfSubmission);
                return new AzureStorageKeyValuePersistenceService(config);
            }).Keyed<IKeyValuePersistenceService>(JobType.EsfSubmission).InstancePerLifetimeScope();

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