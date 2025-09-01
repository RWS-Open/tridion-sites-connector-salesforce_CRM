using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Sdl.Web.Common.Logging;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    public class TrackingNamespaceEntityType
    {
        public static IList<TrackingNamespaceEntityType> ReadConfiguredTypes()
        {
         //   System.Diagnostics.Debugger.Break();
        //    System.Diagnostics.Debugger.Launch();
            var list = new List<TrackingNamespaceEntityType>();
            var trackingConfig = WebConfigurationManager.AppSettings["crm-tracking-entity-types"];
            if (trackingConfig != null)
            {
                var nsEntityTypes = trackingConfig.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var nsEntityType in nsEntityTypes)
                {
                    var parts = nsEntityType.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        Log.Info(
                            $"Invalid tracking entity type: {nsEntityType}. Should be in format: [namespace]:[type name]");
                    }

                    list.Add(new TrackingNamespaceEntityType {NamespaceId = parts[0], TypeName = parts[1]});
                }
            }
            else
            {
                Log.Warn("No tracking entities configured. Visitor tracking is disabled.");
            }
            return list;
        }
        
        public string NamespaceId { get; set; }
        public string TypeName { get; set; }
    }
}