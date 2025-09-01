using System.Collections.Generic;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    public interface ITrackingHandler
    {
        string CreateTrackingKey(DynamicEntity visitor, string namespaceId, string type);

        string GetVisitorTrackingKey(string namespaceId, string type);
        
        // TODO: Should this be in a separate loader interface???
        DynamicEntity LoadVisitor(string trackingKey, string namespaceId, string type);

        void TrackVisitor(DynamicEntity visitor, PageModel pageModel, IList<string> trackedCategories);  
        // TODO: Separate between view & conversion tracking? Should it be different kind of the tracking events? That can be configurable?
    }
}