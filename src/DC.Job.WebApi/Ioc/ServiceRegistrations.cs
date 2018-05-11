using Autofac;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobQueueManager.Interfaces;

namespace ESFA.DC.Job.WebApi.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobQueueManager.JobQueueManager>().As<IJobQueueManager>().InstancePerLifetimeScope();
            //builder.RegisterType<JobQueueManagerSettings>().As<IJobQueueManagerSettings>().SingleInstance();

            //builder.RegisterType<SqlServerKeyValuePersistenceService>().As<IKeyValuePersistenceService>().InstancePerLifetimeScope();
            //builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();

            //builder.Register(context =>
            //{
            //    var logger = ESFA.DC.Logging.LoggerManager.CreateDefaultLogger();
            //    return logger;
            //})
            //    .As<ILogger>()
            //    .InstancePerLifetimeScope();
        }
    }
}