using System;
using System.Linq;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Form.Models;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;
using Sdl.Web.Mvc.Controllers;

namespace Sdl.Dxa.Integration.Form.Controllers
{
    /// <summary>
    /// Form Render Controller
    /// </summary>
    public class FormRenderController : EntityController
    {
        // TODO: How to build an invisible widget that automatically post things to SFMC etc? Inherit from this class?
        
        protected override ViewModel EnrichModel(ViewModel model)
        {
          //  System.Diagnostics.Debugger.Launch();
            Log.Info("Info FormRenderController EnrichModel");
            // TODO: Refactor into one controller for the form so ModelState can be used???

            var form = base.EnrichModel(model) as IntegrationForm;

            if ( form.FormType.Equals("Update") && form.Fields != null )
            {
                // TODO: Have this configurable in the metadata, either use a configured visitor
                // However this CRM specific or???
                 
                // TODO: Should update always be connected to the current visitor?
                // TODO: Use the session storage for this
                // TODO: Use IVisitorLoader???
                //PopulateFormFieldsWithAdfClaims(form);
            }

            form.ErrorMessage = Request.QueryString["errorMessage"]; // TODO: Use form state here instead???
            form.SuccessMessage = Request.QueryString["successMessage"];
            
            // If message -> restore form state
            // TODO: Use ASP.NET best practices here to handle error messages etc?
            if ( form.ErrorMessage != null || form.SuccessMessage != null )
            {
                Log.Info($"Restoring form fields for form '{form.FormId}' from entity session....");
                var currentFormEntity = EntitySession.Instance.GetEntity("Form", form.FormId); // TODO: What happens here if we use cookie session state?
                if (currentFormEntity != null)
                {
                    Log.Info("Found entity - Restoring...");
                    foreach (var formFieldName in currentFormEntity.GetFieldNames())
                    {
                        var field = GetField(form, formFieldName);
                        if (field != null )
                        {
                            field.Value = currentFormEntity.GetField(formFieldName)?.ToString();
                        }
                    }
                }
            }
            
            // Mark fields read-only based on current entity schema & default type validations if not defined
            //
            if (FormFieldPolicies.GetPolicy(form.ObjectType) == FormFieldPolicy.EntityFields && form.Fields != null && form.Fields.Count > 0)
            {
                // TODO: Build a form type repository. Store the entity schema to avoid rereading the schema the whole time
                
                var entitySchema = IntegrationApiClientProvider.Instance.Client.GetEntitySchema(form.ObjectType);
                foreach (var formField in form.Fields)
                {
                    if (formField.ExternalField != null)
                    {
                        var fieldName = formField.ExternalField.FieldName;
                        if (entitySchema != null)
                        {
                            var schemaField = entitySchema.Fields.FirstOrDefault(f =>
                                f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                            if (schemaField != null)
                            {
                                formField.SchemaFieldDefinition = schemaField;
                            } 
                        }
                        if (!formField.ExternalField.EntityType.Equals(form.ObjectType, StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Warn($"Invalid form field definition for field '{fieldName}' - Invalid entity type used: {formField.ExternalField.EntityType}");
                            formField.InvalidFieldDefinition = true;
                        }
                    }
                }
            }

            return model;
        }

        /* TODO: This should be something that is fetched from the session
        /// <summary>
        /// Populate form fields with ADF claims
        /// </summary>
        /// <param name="form"></param>
        protected virtual void PopulateFormFieldsWithAdfClaims(CRMForm form)
        {
            Log.Info("Populating the form field with ADF claims...");
            var claims = ADFManager.GetClaims(form.ObjectType);
            foreach (var claim in claims)
            {
                var claimUriStr = claim.Key.ToString();
                if (claimUriStr.EndsWith(":id") )
                {
                    // Set entity ID
                    //
                    var entityId = claim.Value as string;
                    form.EntityId = entityId;
                }             
                var fieldName = ADFManager.ToFieldName(claim.Key);
                CRMField field = GetField(form, fieldName);
                if (field != null)
                {
                    var fieldValue = claim.Value.ToString();
                    field.Value = fieldValue;
                }
            }

        }
        */
        
        /// <summary>
        /// Get Form Field
        /// </summary>
        /// <param name="form"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual FormField GetField(IntegrationForm form, String name)
        {
            foreach (var field in form.Fields)
            {
                if (field.ExternalField != null && field.ExternalField.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase) )
                {
                    return field;
                }
            }
            return null;
        }

    }
}