using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Form.Models;
using Sdl.Dxa.Modules.Crm;
using Sdl.Tridion.Api.GraphQL.Client.Exceptions;
using Sdl.Web.Common;
using Sdl.Web.Common.Extensions;
using Sdl.Web.Common.Logging;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Form.Controllers
{
    /// <summary>
    /// Integration Form Controller
    /// </summary>
    public class IntegrationFormController : Controller
    {
        // Hook this into the TIF orchestration engine in some way??
        // Use forms for activity requests as well? Like integrating with an MRM for example... So an portal for activity requests etc could potentially be DXA based???

        // TODO: Merge static & additional fields into one single field
        // TODO: Add support for form selector to select different forms depending on selection?? 
        
        // Refactor the create/update parts into a separate class so it can be used as an invisible widget as well

        private IDictionary<string, string> _objectKeyFieldNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IntegrationFormController()
        {
      //      System.Diagnostics.Debugger.Launch();
      //      System.Diagnostics.Debugger.Break();
            var objectKeyFieldNamesConfig = WebConfigurationManager.AppSettings["form-objectkey-fieldnames"];
            var parts = objectKeyFieldNamesConfig.Split(new char[] {'=', ',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i=i+2)
            {
                var entityName = parts[i];
                if (i + 1 < parts.Length)
                {
                    var fieldName = parts[i + 1];
                    _objectKeyFieldNames.Add(entityName, fieldName);
                }
            }
        }
        
        /// <summary>
        /// Create CRM Object
        /// </summary>
        /// <param name="namespaceId"></param>
        /// <param name="objectType"></param>
        /// <param name="formData"></param>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/integration/form/create/{namespaceId}/{objectType}/{objectKey?}")]
        public virtual async Task<ActionResult> CreateObject(string namespaceId, string objectType, FormCollection formData, string objectKey = null)
        {
          
            var formId = GetFormId(formData); 
            Log.Info($"Create object of type: {objectType}, form ID: {formId}, object key: {objectKey}");
            Log.Debug($"Create object of type: {objectType}, form ID: {formId}, object key: {objectKey}");
            // Remove CSRF token
            //
            formData.Remove("__RequestVerificationToken"); // TODO: Do some validation on the request verification token!!

            var integrationClient = IntegrationApiClientProvider.Instance.Client;

            DynamicEntity entity;
            if (FormFieldPolicies.GetPolicy(objectType) == FormFieldPolicy.EntityFields)
            {
                entity = BuildEntityFromFormData(formData, "___");
            }
            else // if FormFieldPolicy.NameValueList
            {
                entity = BuildEntityWithNameValueFieldsFromFormData(formData, "___");
            }

            try
            {
                // TODO: Add support for parent IDs (taken from a hidden form field)
                // This needs to be configurable -> Maybe we need a form type repository anyway?
                /*
                if (parentId != null)
                {
                    integrationClient.CreateEntityWithResponse(
                        new EntityIdentity(parentId)
                        {
                            // Type on the parent ID??  
                            Type = "", // TODO: Read from config.
                            NamespaceId = namespaceId
                        },
                        objectType, entity);
                }
                */

                if (objectKey != null)
                {
                    if (!_objectKeyFieldNames.ContainsKey(objectType))
                    {
                        throw new DxaException($"No field name defined for object key={objectKey}");
                    }

                    var fieldName = _objectKeyFieldNames[objectType];
                    entity.SetField(fieldName, objectKey);
                }
                
                // Process additional fields
                //
                ProcessAdditionalFields(entity, formData);
                
                integrationClient.CreateEntityWithResponse(RootEntity.CreateRootEntityIdentity(namespaceId, null),
                        objectType, entity);
                
                string successUrl = formData.Get("___successUrl"); // TODO: Add constants for this
                if (!string.IsNullOrWhiteSpace(successUrl))
                {
                    return Redirect(successUrl); //  + "?" + objectType.toLowerCase() + "Id=" + id);
                }
                else
                {
                    return ShowSuccessMessageOnFormPage(formId, entity,
                        objectType + " successfully created."); // TODO: Add localization support here!!!
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not create form object: " + e);
                string errorUrl = formData.Get("___errorUrl");
                if (errorUrl == null)
                {
                    // If no error URL, show the error on the form page
                    //
                    return ShowErrorOnFormPage(formId, entity, e);
                }
                return Redirect(errorUrl);
            }
            
        }
        
        /// <summary>
        /// Update CRM Object
        /// </summary>
        /// <param name="namespaceId"></param>
        /// <param name="objectType"></param>
        /// <param name="id"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/integration/form/update/{namespaceId}/{objectType}/{id}")]
        public virtual async Task<ActionResult> UpdateObject(string namespaceId, string objectType, string id, FormCollection formData)
        {
            var formId = GetFormId(formData);

            // Remove CSRF token
            //
            formData.Remove("__RequestVerificationToken"); // TODO: Do some validation on the request verification token!!
            
            var integrationClient = IntegrationApiClientProvider.Instance.Client;
            var entity = BuildEntityFromFormData(formData,  "___");
            var entityIdentity = new EntityIdentity(id)
            {
                NamespaceId = namespaceId,
                Type = objectType
            };
            
            try
            {
                integrationClient.UpdateEntityWithResponse(entityIdentity, entity);
                Log.Debug("Updated '" + objectType + "' entity with ID: " + id);

                string successUrl = formData.Get("___successUrl"); // TODO: Add constants for this
                if (successUrl != null)
                {
                    return Redirect(successUrl); //  + "?" + objectType.toLowerCase() + "Id=" + id);
                }
                else
                {
                    return ShowSuccessMessageOnFormPage(formId, entity, objectType + " successfully updated."); // TODO: Add localization support here!!!
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not update external object: " + e);
                string errorUrl = formData.Get("___errorUrl");
                if (errorUrl == null)
                {
                    // If no error URL, show the error on the form page
                    //
                    return ShowErrorOnFormPage(formId, entity, e);
                }
                return Redirect(errorUrl);
            }
        }

        private ActionResult ShowSuccessMessageOnFormPage(string formId, DynamicEntity formEntity, string message)
        {
            EntitySession.Instance.SaveEntity("Form", formEntity, formId);
            var formUrl = Request.UrlReferrer.ToString();
            formUrl = AddMessageToUrl(formUrl, "successMessage", message); // TODO: Store this onto the session as well? Or have form session ID?
            return Redirect(formUrl);
        }

        /// <summary>
        /// Show error on form page
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="formData"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private ActionResult ShowErrorOnFormPage(string formId, DynamicEntity formEntity, Exception e)
        {
            EntitySession.Instance.SaveEntity("Form", formEntity, formId);
            var formUrl = Request.UrlReferrer.ToString();
            var errorMessage = e.Message;
            if (e is GraphQLClientException graphQlClientException)
            {
                var errors = graphQlClientException?.GraphQLResponse?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    var firstError = errors[0].Message;
                    if (firstError.Contains(":"))
                    {
                        errorMessage = firstError.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                    else
                    {
                        errorMessage = firstError;
                    }
                }
            }
            formUrl = AddMessageToUrl(formUrl, "errorMessage", errorMessage);
            return Redirect(formUrl);
        }

        /// <summary>
        /// Add message to URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="messageParamName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string AddMessageToUrl(string url, string messageParamName, string message)
        {
            bool firstParam = true;
            if (url.Contains("Message="))
            {
                // Clear old messages
                //
                var tokens = url.Split(new char[] {'?', '&'}, StringSplitOptions.RemoveEmptyEntries);
                url = null;
                foreach ( var token in tokens )
                {
                    if ( url == null )
                    {
                        url = token;
                        continue;
                    }
                    if (!token.Contains("Message"))
                    {
                        if (firstParam)
                        {
                            url += "?";
                            firstParam = false;
                        }
                        else
                        {
                            url += "&";
                        }
                        url += token;
                    }
                }          
            } 
            url += (firstParam ? "?" : "&") + messageParamName + "=" + HttpUtility.UrlEncode(message);
            return url;
        }

        /// <summary>
        /// Get Form ID
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        private string GetFormId(FormCollection formData)
        {
            return formData.Get("___formId");
        }
        
        /// <summary>
        /// Build Entity from form data
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="objectType"></param>
        /// <param name="excludePrefix"></param>
        /// <returns></returns>
        private static DynamicEntity BuildEntityFromFormData(FormCollection formData, string excludePrefix = null)
        {
            // TODO: Add support for nested fields and readonly fields

            var entity = new DynamicEntity();
            foreach (var param in formData.Keys)
            {
                var paramStr = param.ToString();
                if (excludePrefix == null || !paramStr.StartsWith(excludePrefix))
                {
                    entity.SetField((param.ToString().ToCamelCase()), formData.Get(paramStr));
                }
            }
            return entity;
        }
        
        private static DynamicEntity BuildEntityWithNameValueFieldsFromFormData(FormCollection formData, string excludePrefix = null)
        {

            var entity = new DynamicEntity();
            var list = new List<NestedObject>();
            entity.SetField("values", list); // TODO: Have this configurable!!
            foreach (var param in formData.Keys)
            {
                var paramStr = param.ToString();
                if (excludePrefix == null || !paramStr.StartsWith(excludePrefix))
                {
                    var value = new NestedObject();
                    value.SetField("name", param.ToString());
                    value.SetField("value", formData.Get(paramStr));
                    list.Add(value);
                }
            }
            return entity;
        }

        private static void ProcessAdditionalFields(DynamicEntity entity, FormCollection formData)
        {
            foreach (var param in formData.Keys)
            {
                var paramStr = param.ToString();
                if (paramStr.StartsWith("____"))
                {
                    var value = formData.Get(paramStr);
                    paramStr = paramStr.Substring(4);
                    entity.SetField(paramStr, VariableSubstituter.ResolveEntityValue(value, entity));
                }
                
            }
        }
        
    }
}