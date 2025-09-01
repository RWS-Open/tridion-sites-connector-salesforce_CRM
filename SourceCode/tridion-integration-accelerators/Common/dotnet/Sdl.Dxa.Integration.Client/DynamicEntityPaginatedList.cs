using System.Collections.Generic;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Integration.Client
{
    public class DynamicEntityPaginatedList : IEntityPaginatedList
    {
        public int TotalCount { get; internal set; }
        public int PageSize { get; internal set; }
        public int StartIndex { get; internal set; }
        public int PageIndex { get; internal set; }
        public bool HasMoreItems { get; internal set; }
        
        // TODO: Set this to DynamicEntities if empty
        public IEnumerable<IEntity> Entities { get; internal set; }
        public IEnumerable<DynamicEntity> DynamicEntities { get; internal set; }
        
    }
}