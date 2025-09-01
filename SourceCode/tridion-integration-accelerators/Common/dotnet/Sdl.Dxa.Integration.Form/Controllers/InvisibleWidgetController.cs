using System;
using System.Collections.Generic;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Form.Models;
using Sdl.Dxa.Modules.Crm;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;
using Sdl.Web.Mvc.Controllers;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Form.Controllers
{
    public class InvisibleWidgetController : EntityController
    {
        protected override ViewModel EnrichModel(ViewModel model)
        {
            
            var widget = base.EnrichModel(model) as InvisibleFormWidget;
            
            try
            {
                var entity = new DynamicEntity();
                var nameValueList = new List<NestedObject>();
                foreach (var field in widget.Fields)
                {
                    if (field.IsSchemaField)
                    {
                        entity.SetField(field.ResolveFieldName(), field.ResolveFieldValue());
                    }
                    else
                    {
                        // Untyped value to be inserted into a name-value list
                        //
                        var value = new NestedObject();
                        value.SetField("name", field.ResolveFieldName());
                        value.SetField("value", field.ResolveFieldValue());
                        nameValueList.Add(value);
                    }
                }

                if (nameValueList.Count > 0)
                {
                    entity.SetField("values", nameValueList);
                }
                
                // TODO: Trigger only once per session. Can we have a option for that? Like TriggerOncePerRequest, TriggerOncePerSession
                // TODO: Trigger either an create or an update here
                var integrationClient = IntegrationApiClientProvider.Instance.Client;

                // TODO: Have a threshold or pre-condition if this should happen? Like all fields could be resolved?
                // TODO: Do this in the background to not impact page load time
                integrationClient.CreateEntityWithResponse(RootEntity.CreateRootEntityIdentity(widget.NamespaceId, null),
                    widget.ObjectType, entity);

            }
            catch (ResolveFieldException e)
            {
                Log.Warn(e.Message);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Could not trigger external action towards entity of type: {widget.ObjectType}");
            }

            return widget;
        }
        
        
    }
}