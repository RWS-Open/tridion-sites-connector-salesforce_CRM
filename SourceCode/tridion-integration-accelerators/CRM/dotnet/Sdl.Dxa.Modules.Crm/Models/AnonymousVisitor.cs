using System.Collections.Generic;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.Crm.Models
{
    public class AnonymousVisitor : DynamicEntity
    {
        public IList<DynamicEntity> TrackingRecords { get; } = new List<DynamicEntity>();
        
    }
}