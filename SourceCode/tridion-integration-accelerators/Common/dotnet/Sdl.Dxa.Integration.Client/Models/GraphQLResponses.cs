using System.Collections.Generic;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client.Models
{
    
    /// <summary>
    /// External Namespaces Response
    /// </summary>
    public class ExternalNamespacesResponse
    {
        public ExternalNamespace[] ExternalNamespaces { get; set; }
    }

    public class ExternalItemResponse
    {
        public Dictionary<string,object> ExternalItem { get; set; }
    }

    public class ExternalItemsResponse
    {
        public ExternalItemsEdges ExternalItems { get; set; }
    }

    public class ExternalItemsEdges
    {
        public List<ExternalItemNode> Edges { get; set; }
    }

    public class ExternalItemNode
    {
        public Dictionary<string,object> Node { get; set; }
    }

    public class ExternalIdentityResponse
    {
        public EntityIdentity ExternalIdentity { get; set; }
    }
}