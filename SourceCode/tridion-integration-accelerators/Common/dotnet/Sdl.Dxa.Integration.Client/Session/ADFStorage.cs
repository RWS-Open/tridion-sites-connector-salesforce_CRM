using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Sdl.Web.Common.Logging;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ContentDelivery.AmbientData;
using Tridion.ContentDelivery.AmbientData.Runtime;
using Tridion.Remoting.Contracts;

namespace Sdl.Dxa.Integration.Client
{
    /// <summary>
    /// ADF Storage.
    /// </summary>
    public class ADFStorage : IEntitySessionStorage
    {
        const string BASE_CLAIM_URI = "taf:claim:integration";
        
        private IDictionary<string, IList<string>> _adfClaims = new Dictionary<string, IList<string>>(); 
        
        public ADFStorage()
        {
          //  System.Diagnostics.Debugger.Break();
          //  System.Diagnostics.Debugger.Launch();

            var adfClaims = WebConfigurationManager.AppSettings["session-adf-claims"];
            if (adfClaims != null)
            {
                foreach (var adfClaim in adfClaims.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = adfClaim.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var entityName = parts[0];
                        var fieldName = parts[1];

                        if (!_adfClaims.TryGetValue(entityName, out IList<string> fieldNames))
                        {
                            fieldNames = new List<string>();
                            _adfClaims.Add(entityName, fieldNames);
                        }

                        fieldNames.Add(fieldName);
                    }
                    else
                    {
                        Log.Warn(
                            $"Invalid format of ADF claim '{adfClaim}'. Should be of format: [Entity Name].[Field Name]. Ignoring the claim definition.");
                    }
                }
            }
        }
        
        public string SessionId => null;

        // TODO: Need namespace ID here as well???
        
        public void SaveEntity(string type, DynamicEntity entity, string id = null)
        {
            // TODO: We need to have a refresh interval configurable
            
            // Store selected entity fields as ADF claims so it can be consumed by Experience Optimization etc
            //
            if (type != null && entity != null && _adfClaims.TryGetValue(type, out IList<string> fieldsToExpose))
            {
                // TODO: Refactor to use a XPath like syntax instead
                
                var claimStore = AmbientDataContext.CurrentClaimStore;
                foreach (var fieldName in fieldsToExpose)
                {
                    string fieldNameToProcess = fieldName;
                    // If giving an ADF claim a specific name instead of using default field name
                    //
                    string adfSpecificName = null;
                    if (fieldName.Contains(">"))
                    {
                        var tokens = fieldName.Split(new char[]{'>'}, StringSplitOptions.RemoveEmptyEntries);
                        fieldNameToProcess = tokens[0];
                        adfSpecificName = tokens[1].ToLower();

                    }
                    var adfFieldName = fieldNameToProcess.ToLower();
                    object fieldValue = null;
                    if (fieldNameToProcess.Contains("#"))
                    {
                        var tokens = fieldNameToProcess.Split(new char[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
                        
                        // For now only support 1 nested level
                        //
                        if (tokens.Length > 2)
                        {
                            Log.Warn("ADF Storage only supports 1 level nesting of entity fields. Skipping field: " + fieldNameToProcess);
                            continue;
                        }

                        var nestedFieldName = tokens[0];
                        var leafFieldName = tokens[1];
                        bool firstItem = false;
                        if (leafFieldName.EndsWith("[0]"))
                        {
                            firstItem = true;
                            leafFieldName = leafFieldName.Replace("[0]", "");
                        }
                        var nestedField = entity.GetField(nestedFieldName);
                        if (nestedField is IList<NestedObject> nestedList)
                        {
                            var valueList = new List<object>(); 
                            foreach (var nestedObject in nestedList)
                            {
                                valueList.Add(GetFieldValue(leafFieldName, nestedObject, out adfFieldName));
                            }

                            if (firstItem)
                            {
                               fieldValue = valueList.FirstOrDefault();
                            }
                            else
                            {
                                fieldValue = valueList;
                            }
                        }
                        else if (nestedField is NestedObject nestedObject)
                        {
                            fieldValue = GetFieldValue(leafFieldName, nestedObject, out adfFieldName);
                        }
                    }
                    else
                    {
                        if (fieldNameToProcess.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            fieldValue = entity.Identity?.Id;
                        }
                        else
                        {
                            fieldValue = GetFieldValue(fieldNameToProcess, entity, out adfFieldName);
                        }
                    }

                    if (fieldValue != null)
                    {
                        // TODO: Why is this not available in the same request?
                        var claimUri = new Uri(BASE_CLAIM_URI + ":" + type.ToLower() + ":" + (adfSpecificName ?? adfFieldName));
                        Log.Info($"Exposing ADF claim: {claimUri} = {fieldValue}");
                        claimStore.Put(claimUri, fieldValue, ClaimType.Normal, ClaimValueScope.Session);
                    }
                }
            }
            
        }

        public DynamicEntity LoadEntity(string type, string id = null)
        {
            return null;
        }


        private static object GetFieldValue(string fieldName, IDynamicValueObject valueObject, out string adfFieldName)
        {
            if (fieldName.Contains("["))
            {
                // If name-value field
                //
                return GetNameValueFieldValue(fieldName, valueObject, out adfFieldName);
            }
            else
            {
                adfFieldName = fieldName.ToLower();
                return valueObject.GetField(fieldName);
            }
        }
        
        private static object GetNameValueFieldValue(string fieldName, IDynamicValueObject valueObject, out string adfFieldName)
        {
            object fieldValue = null;
            var parts = fieldName.Split(new char[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            var nameValueListFieldName = parts[0];
            if (parts.Length == 1)
            {
                Log.Warn("Invalid claim field: " + fieldName);
               
            }
            else
            {
                var nestedFieldName = parts[1];
                var nameValueList = valueObject.GetField(nameValueListFieldName) as IList<NestedObject>;
                if (nameValueList != null)
                {
                    foreach (var nameValue in nameValueList)
                    {
                        var name = nameValue.GetField("Name") as string;
                        if (name != null && name.Equals(nestedFieldName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            adfFieldName = (nameValueListFieldName + "." + nestedFieldName).ToLower();
                            return nameValue.GetField("Value");
                        }
                    }
                }
            }

            adfFieldName = fieldName;
            return null;
        }
    }
}