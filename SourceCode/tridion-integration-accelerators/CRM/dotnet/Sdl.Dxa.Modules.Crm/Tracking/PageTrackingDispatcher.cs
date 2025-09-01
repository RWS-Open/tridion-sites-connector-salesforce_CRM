using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;
using Sdl.Web.DataModel;
using Sdl.Web.Tridion.Mapping;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    // TODO: Should this be part of the personalization module??
    public class PageTrackingDispatcher : IPageModelBuilder
    {
        private IList<TrackingNamespaceEntityType> _entityTypes = TrackingNamespaceEntityType.ReadConfiguredTypes();

        private readonly string[] _excludePaths = null;
        private readonly bool _trackOnlyCategorizedPages = false;

        public PageTrackingDispatcher()
        {
      //      System.Diagnostics.Debugger.Launch();
     //       System.Diagnostics.Debugger.Break();
            // TODO: Add support for ANT style path style here
            var excludePathsStr = WebConfigurationManager.AppSettings["crm-tracking-exclude-paths"];
            _excludePaths = excludePathsStr.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            
            var trackOnlyCategorizedPagesStr = WebConfigurationManager.AppSettings["crm-track-only-categorized-pages"];
            if (trackOnlyCategorizedPagesStr != null)
            {
                _trackOnlyCategorizedPages = Boolean.Parse(trackOnlyCategorizedPagesStr);
            }
        }
        
        public void BuildPageModel(ref PageModel pageModel, PageModelData pageModelData, bool includePageRegions,
            Localization localization)
        {
            //System.Diagnostics.Debugger.Launch();
           // System.Diagnostics.Debugger.Break();
            if (_entityTypes == null || _entityTypes.Count == 0)
            {
                return;
            }
            
            if (_excludePaths != null)
            {
                foreach (var path in _excludePaths)
                {
                    if (pageModel.Url.StartsWith(path))
                    {
                        return;
                    }
                }
            }

            var trackedCategories = new List<string>();
            foreach (var region in pageModel.Regions)
            {
                ExtractTrackedCategories(region, trackedCategories);
            }

            if (_trackOnlyCategorizedPages && trackedCategories.Count == 0)
            {
                return;
            }

            foreach (var entityType in _entityTypes)
            {
                string trackingKey = null;
                DynamicEntity visitor = null;
                foreach (var trackingHandler in DIRegisty.GetList<ITrackingHandler>())
                {
                    trackingKey = trackingHandler.GetVisitorTrackingKey(entityType.NamespaceId, entityType.TypeName);
                    if (trackingKey != null)
                    {
                        break;
                    }
                }

                // TODO: Have a tracking enabled option here
                
                if (trackingKey != null)
                {
                    foreach (var trackingHandler in DIRegisty.GetList<ITrackingHandler>())
                    {
                        visitor = trackingHandler.LoadVisitor(trackingKey, entityType.NamespaceId, entityType.TypeName);
                        if (visitor != null)
                        {
                            break;
                        }
                    }

                    if (visitor != null)
                    {
                        foreach (var trackingHandler in DIRegisty.GetList<ITrackingHandler>())
                        {
                            trackingHandler.TrackVisitor(visitor, pageModel, trackedCategories);
                        }
                    }
                }
            }
        }
        
        private void ExtractTrackedCategories(RegionModel region, List<string> trackedCategories)
        {
            foreach (var entity in region.Entities)
            {
                if (entity is ITrackedEntity trackedEntity)
                {
                    if (trackedEntity.TrackedCategories != null)
                    {
                        trackedCategories.AddRange(trackedEntity.TrackedCategories);
                    }
                }
            }

            foreach (var subRegion in region.Regions)
            {
                ExtractTrackedCategories(subRegion, trackedCategories);
            }
        }
    }
    
}