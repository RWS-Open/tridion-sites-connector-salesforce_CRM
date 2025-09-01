using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Sdl.Dxa.Integration.Form.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    /// <summary>
    /// Form Field Policies
    /// 
    /// Controls how the form fields should be populated in the GraphQL calls to the connector. Either it uses
    /// the defined entity fields (default) or pass it as a name-value list (for more dynamic scenarios).
    ///
    /// Example config: <add key="form-field-policy" value="JourneyTrigger=NameValueList, Contact=EntityFields" />
    /// </summary>
    public class FormFieldPolicies
    {
        // TODO: Create a form metadata configuration class instead
        // TODO: Could static be a potential problem here? Will changes in Web.config be taken in effect directly?
        private static IDictionary<string, FormFieldPolicy> _formFieldPolicies = null;

        public static FormFieldPolicy GetPolicy(string entityName)
        {
            if (_formFieldPolicies == null)
            {
                readPoliciesFromConfiguration();
            }

            if (_formFieldPolicies.ContainsKey(entityName))
            {
                return _formFieldPolicies[entityName];
            }

            // If no policy defined, use 'EntityFields' as default
            //
            return FormFieldPolicy.EntityFields;
        }

        private static void readPoliciesFromConfiguration()
        {
            _formFieldPolicies = new Dictionary<string, FormFieldPolicy>(StringComparer.OrdinalIgnoreCase);
            var policies = WebConfigurationManager.AppSettings["form-field-policies"];
            var policyParts = policies.Split(new char[] {'=', ',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            for (int i=0; i < policyParts.Length; i=i+2)
            {
                var entityName = policyParts[i];
                if (i + 1 < policyParts.Length)
                {
                    FormFieldPolicy policy;
                    if (Enum.TryParse<FormFieldPolicy>(policyParts[i+1], out policy))
                    {
                        _formFieldPolicies.Add(entityName, policy);
                    }
                }
            }
        }
    }
}