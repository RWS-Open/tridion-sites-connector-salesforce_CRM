using System;
using System.Collections.Generic;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ContentDelivery.AmbientData;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    public class ADFTrackingHandler : ITrackingHandler
    {
        public static readonly Uri TRACKING_ID_URI = new Uri("taf:tracking:id");
        
        public string CreateTrackingKey(DynamicEntity visitor, string namespaceId, string type)
        {
       //    System.Diagnostics.Debugger.Launch();
       //     System.Diagnostics.Debugger.Break();
            return GetVisitorTrackingKey(namespaceId, type);
        }

        public string GetVisitorTrackingKey(string namespaceId, string type)
        {
            var trackingId = AmbientDataContext.CurrentClaimStore.Get<string>(TRACKING_ID_URI);
            return trackingId;
        }

        public DynamicEntity LoadVisitor(string trackingKey, string namespaceId, string type)
        {
            return null;
        }

        public void TrackVisitor(DynamicEntity visitor, PageModel pageModel, IList<string> trackedCategories)
        {
        }
    }
}