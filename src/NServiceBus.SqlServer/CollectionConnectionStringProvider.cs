﻿namespace NServiceBus.Transports.SQLServer
{
    using System.Collections.Generic;
    using System.Linq;

    class CollectionConnectionStringProvider : IConnectionStringProvider
    {
        readonly LocalConnectionParams localConnectionParams;
        readonly IEnumerable<EndpointConnectionInfo> connectionStrings;

        public CollectionConnectionStringProvider(IEnumerable<EndpointConnectionInfo> connectionStrings, LocalConnectionParams localConnectionParams)
        {
            this.localConnectionParams = localConnectionParams;
            this.connectionStrings = connectionStrings.ToList();
        }

        public ConnectionParams GetForDestination(Address destination)
        {
            var found = connectionStrings.FirstOrDefault(x => x.Endpoint == destination.Queue);
            return found != null
                ? found.CreateConnectionParams(localConnectionParams)
                : null;
        }
    }
}