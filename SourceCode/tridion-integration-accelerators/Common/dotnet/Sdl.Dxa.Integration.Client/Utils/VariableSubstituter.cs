using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sdl.Dxa.Integration.Client;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.Remoting.Contracts;

namespace Sdl.Dxa.Modules.Crm
{
    public class VariableSubstituter
    {
        private static readonly Regex VariableRegex = new Regex("%([^%]+)%", RegexOptions.Compiled);

        public static string ProcessContent(string content, Func<string,string> SubstituteFunction) 
        {
            return VariableRegex.Replace(content, m => SubstituteFunction(m.Value.Replace("%", "")));
        }

        public static object ResolveEntityValue(string value, DynamicEntity entity = null)
        {
            if (value.StartsWith("%[") && value.EndsWith("]"))
            {
                var expression = value.Substring(2, value.Length - 3);
                var tokens = expression.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                {
                    return default;
                }
                var entityType = tokens[0];
                if (entityType.Equals("this") && entity == null)
                {
                    // Keep this values unhandled here for further processing at later stage
                    // when an entity has been built up.
                    //
                    return value;
                }
                else if (entity == null)
                {
                    entity = EntitySession.Instance.GetEntity(entityType);
                }

                if (entity == null)
                {
                    throw new ResolveFieldException($"Entity {entityType} not found");
                }
                if (tokens.Length > 1 && entity != null)
                {
                    if (tokens[1].Equals("Id"))
                    {
                        // Retrieve identity identity directly from entity
                        //
                        return entity.Identity?.Id;
                    }
                    object fieldValue = null;
                    IDynamicValueObject currentValueObject = entity;
                    for (var i = 1; i < tokens.Length; i++)
                    {
                        fieldValue = currentValueObject.GetField(tokens[i]);
                        if (fieldValue is IDynamicValueObject nestedValueObject)
                        {
                            currentValueObject = nestedValueObject;
                        }
                        else
                        {
                            if (fieldValue == null)
                            {
                                // Try if value is available as a name-value 
                                //
                                if (entity.GetField("values") is IList<NestedObject> nameValueList)
                                {
                                    foreach (var nameValue in nameValueList)
                                    {
                                        var name = nameValue.GetField("Name") as string;
                                        if (name != null && name.Equals(tokens[i], StringComparison.OrdinalIgnoreCase))
                                        {
                                            return nameValue.GetField("Value");
                                        }
                                    }
                                }
                            }
                            
                            throw new ResolveFieldException($"Field expression '{expression}' could not be resolved to an entity field.");
                        }
                    }

                    return fieldValue;
                }
            }

            return value;
        }

    }
    
    public class ResolveFieldException : Exception
    {
        public ResolveFieldException(string message) : base(message)
        {
        }
        
    }

}