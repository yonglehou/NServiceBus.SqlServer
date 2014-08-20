namespace NServiceBus.Features
{
    using System;
    using System.Linq;
    using Pipeline;
    using Settings;
    using Support;
    using Transports;
    using Transports.SQLServer;
    using System.Configuration;

    /// <summary>
    /// Configures NServiceBus to use SqlServer as the default transport
    /// </summary>
    class SqlServerTransport : ConfigureTransport
    {
        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return @"Data Source=.\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True"; }
        }

        protected override void Configure(FeatureConfigurationContext context, string connectionString)
        {
            //Until we refactor the whole address system
            CustomizeAddress(context.Settings);

            var defaultConnectionString = context.Settings.Get<string>("NServiceBus.Transport.ConnectionString");

            //Load all connectionstrings 
            var collection =
                ConfigurationManager
                    .ConnectionStrings
                    .Cast<ConnectionStringSettings>()
                    .Where(x => x.Name.StartsWith("NServiceBus/Transport/"))
                    .ToDictionary(x => x.Name.Replace("NServiceBus/Transport/", String.Empty), y => y.ConnectionString);

            if (String.IsNullOrEmpty(defaultConnectionString))
            {
                throw new ArgumentException("Sql Transport connection string cannot be empty or null.");
            }
            var container = context.Container;
            container.ConfigureComponent<SqlServerQueueCreator>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.ConnectionString, defaultConnectionString);

            container.ConfigureComponent<SqlServerMessageSender>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.DefaultConnectionString, defaultConnectionString)
                .ConfigureProperty(p => p.ConnectionStringCollection, collection);

            container.ConfigureComponent<SqlServerPollingDequeueStrategy>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.ConnectionString, defaultConnectionString);

            context.Container.ConfigureComponent(b => new SqlServerStorageContext(b.Build<PipelineExecutor>(), defaultConnectionString), DependencyLifecycle.InstancePerUnitOfWork);
        }

        void CustomizeAddress(ReadOnlySettings settings)
        {
            Address.IgnoreMachineName();

            if (!settings.GetOrDefault<bool>("ScaleOut.UseSingleBrokerQueue"))
            {
                LocalAddress(settings.EndpointName() + "." + RuntimeEnvironment.MachineName);
            }
        }
    }
}