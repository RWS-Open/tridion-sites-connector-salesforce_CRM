using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity.Configuration.ConfigurationHelpers;
using Newtonsoft.Json.Linq;
using Sdl.Dxa.Integration.Client.Models;
using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Dxa.Modules.Crm;
using Sdl.Tridion.Api.Client;
using Sdl.Tridion.Api.GraphQL.Client;
using Sdl.Tridion.Api.GraphQL.Client.Request;
using Sdl.Tridion.Api.GraphQL.Client.Schema;
using Sdl.Web.Common.Logging;
using Sdl.Web.Tridion.ApiClient;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Connector.SDK.Schema.Builder;
using Tridion.ConnectorFramework.Contracts;
using Tridion.ConnectorFramework.Contracts.Schema;
using Tridion.Remoting.Contracts;

namespace Sdl.Dxa.Integration.Client
{
    
    // TODO: Use async here!!
    
    public class IntegrationApi : IIntegrationApi
    {
        private GraphQLClient _graphQlClient;

        private static ExternalNamespace[] _namespaces;
        
        public IntegrationApi(GraphQLClient graphQlClient)
        {
            //System.Diagnostics.Debugger.Launch();
            //System.Diagnostics.Debugger.Break();
            _graphQlClient = graphQlClient;
            if (_namespaces == null)
            {
                _namespaces = GetNamespaces();
            }
        }
        
        public DynamicEntity GetEntity(IEntityIdentity identity)
        {
         //   System.Diagnostics.Debugger.Launch();
        //    System.Diagnostics.Debugger.Break();
            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.GetEntity)
                .ReplaceTag("identity", GenerateEntityIdentityString(identity))
                .ReplaceTag("fragment", GenerateEntityFragment(identity.NamespaceId, identity.Type))
                .Build();
            var response = _graphQlClient.Execute<ExternalItemResponse>(request);
            return ToEntity(response.TypedResponseData.ExternalItem);
        }

        public DynamicEntity GetEntity(string eclUri)
        {
            return GetEntity(new EclUri(eclUri));
        }

        public DynamicEntityPaginatedList ListEntities(IEntityIdentity parentIdentity, IPaginationData paginationData)
        {
            throw new NotImplementedException();
        }

        public DynamicEntityPaginatedList QueryEntities(IEntityFilter filter, IPaginationData paginationData)
        {

            if (filter == null)
            {
                throw new ApiClientException("Filter can not be null.");
            }
            if (filter.Context == null)
            {
                throw new ApiClientException("Missing filter context.");
            }

            if (filter.Context.NamespaceId == null || filter.Context.Id == null)
            {
                throw new ApiClientException("Missing mandatory fields in the filter context.");
            }

            if (filter.EntityType == null)
            {
                throw new ApiClientException(
                    "Missing entity type in the search query. Cross entity search is currently not supported.");
            }

            if (filter.SearchText == null)
            {
                throw new ApiClientException("Missing search text in the search query.");
            }

            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.QueryEntities)
                .ReplaceTag("namespace", filter.Context.NamespaceId)
                .ReplaceTag("searchText", filter.SearchText)
                .ReplaceTag("entityType", filter.EntityType)
                .ReplaceTag("context", GenerateEntityIdentityString(filter.Context))
                .ReplaceTag("fragment", GenerateEntityFragment(filter.Context.NamespaceId, filter.EntityType))
                .Build();

            // TODO: Handle pagination here as well!!
            
            var response = _graphQlClient.Execute<ExternalItemsResponse>(request);
            return new DynamicEntityPaginatedList
            {
                DynamicEntities = response.TypedResponseData.ExternalItems.Edges.Select(e => ToEntity(e.Node)).ToList(),
                TotalCount = -1  // Total count is currently not supported through GraphQL
            };
        }

        public IEntityIdentity CreateEntity(IEntityIdentity parentIdentity, string type, DynamicEntity entity)
        {
            InvokePreInterceptors(null, entity, parentIdentity.NamespaceId, type, EntityOperationType.CREATE);
            
            // TODO: Make sure type is in right format, i.e. pascal case
            
            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.CreateEntity)
                .ReplaceTag("type", type)
                .ReplaceTag("parentIdentity", GenerateEntityIdentityString(parentIdentity))
                .ReplaceTag("input",  GenerateEntityInput(entity))
                .ReplaceTag("fragment", string.Empty)
                .Build();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Create Entity GraphQL Request:");
                Log.Debug(request.Query);
            }

            var response = _graphQlClient.Execute<ExternalItemResponse>(request);
            var createdIdentity = ToEntity(response.TypedResponseData.ExternalItem).Identity;
            Log.Info($"Successfully created entity of type '{type}' and identity: {createdIdentity?.Id}");
            
            InvokePostInterceptors(createdIdentity, entity, parentIdentity.NamespaceId, type, EntityOperationType.CREATE);
            
            return createdIdentity;
        }

        public DynamicEntity CreateEntityWithResponse(IEntityIdentity parentIdentity, string type, DynamicEntity entity)
        {
            InvokePreInterceptors(null, entity,  parentIdentity.NamespaceId, type, EntityOperationType.CREATE);
            
            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.CreateEntity)
                .ReplaceTag("type", type)
                .ReplaceTag("parentIdentity", GenerateEntityIdentityString(parentIdentity))
                .ReplaceTag("input",  GenerateEntityInput(entity))
                .ReplaceTag("fragment", GenerateEntityFragment(parentIdentity.NamespaceId, type))
                .Build();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Create Entity GraphQL Request:");
                Log.Debug(request.Query);
            }
            
            var response = _graphQlClient.Execute<ExternalItemResponse>(request);
            var createdEntity = ToEntity(response.TypedResponseData.ExternalItem);
            Log.Info($"Successfully created entity of type '{type}' and identity: {createdEntity?.Identity?.Id}");
            
            InvokePostInterceptors(createdEntity.Identity, createdEntity, parentIdentity.NamespaceId, type, EntityOperationType.CREATE);

            return createdEntity;
        }

        public void UpdateEntity(IEntityIdentity identity, DynamicEntity entity)
        {
            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.UpdateEntity)
                .ReplaceTag("type", identity.Type)
                .ReplaceTag("identity", GenerateEntityIdentityString(identity))
                .ReplaceTag("input",  GenerateEntityInput(entity))
                .ReplaceTag("fragment", String.Empty)
                .Build();
             _graphQlClient.Execute<ExternalItemResponse>(request);
        }

        public DynamicEntity UpdateEntityWithResponse(IEntityIdentity identity, DynamicEntity entity)
        {
           var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.UpdateEntity)
                .ReplaceTag("type", identity.Type)
                .ReplaceTag("identity", GenerateEntityIdentityString(identity))
                .ReplaceTag("input",  GenerateEntityInput(entity))
                .ReplaceTag("fragment", GenerateEntityFragment(identity.NamespaceId, identity.Type))
                .Build();
            var response = _graphQlClient.Execute<ExternalItemResponse>(request); 
            return ToEntity(response.TypedResponseData.ExternalItem); 
        }

        public void DeleteEntity(IEntityIdentity identity)
        {
            var request = new QueryBuilder()
                .WithQuery(IntegrationApiRequests.DeleteEntity)
                .ReplaceTag("type", identity.Type)
                .ReplaceTag("identity", GenerateEntityIdentityString(identity))
                .Build();
            _graphQlClient.Execute<ExternalIdentityResponse>(request);
        }

        public IEntitySchema GetEntitySchema(string typeName)
        {
         //   System.Diagnostics.Debugger.Break();
        //    System.Diagnostics.Debugger.Launch();
            var graphQlSchema = GetSchemaType(typeName);
            var mutationInputSchema = GetSchemaType("Input" + graphQlSchema.Name);
            var schemaBuilder = Builder.NewEntitySchema(graphQlSchema.Name, graphQlSchema.Description, null, false, null);
            BuildSchema(schemaBuilder, graphQlSchema, mutationInputSchema);
            return schemaBuilder.Build();
        }

        private void BuildSchema<T>(ISchemaBuilder<T> schemaBuilder, GraphQLSchemaType graphQlSchema, GraphQLSchemaType mutationInputSchema) where T : ISchema
        {
            foreach (var schemaField in graphQlSchema.Fields)
            {
                bool isReadonly = mutationInputSchema == null || mutationInputSchema != null &&
                    !mutationInputSchema.InputFields.Any(f => f.Name.Equals(schemaField.Name));
                bool isList = schemaField.Type.Kind.Equals("LIST", StringComparison.OrdinalIgnoreCase);
                if (schemaField.Type.Kind.Equals("OBJECT", StringComparison.OrdinalIgnoreCase))
                {
                    var nestedType = GetSchemaType(schemaField.Type.Name);
                    var nestedMutationType = GetSchemaType("Input" + nestedType.Name);
                    var nestedSchemaBuilder = Builder.NewNestedSchema(nestedType.Name, nestedType.Description);
                    BuildSchema(nestedSchemaBuilder, nestedType, nestedMutationType);
                    var nestedSchema = nestedSchemaBuilder.Build();
                    schemaBuilder.AddNestedFieldDefinition(schemaField.Name, nestedSchema);
                }
                else
                {
                    schemaBuilder.AddFieldDefinition(schemaField.Name, GetFieldType(schemaField, isList),
                        schemaField.Description, false, isReadonly, isList);
                }
            } 
        }

        private FieldType GetFieldType(GraphQLSchemaField schemaField, bool isList)
        {
            var graphQlFieldType = isList ? schemaField.Type.OfType : schemaField.Type;
            
            if (graphQlFieldType.Kind.Equals("SCALAR", StringComparison.OrdinalIgnoreCase))
            {
                if (graphQlFieldType.Name.Equals("String", StringComparison.OrdinalIgnoreCase))
                {
                    return FieldType.String;
                }
                if (graphQlFieldType.Name.Equals("Int", StringComparison.OrdinalIgnoreCase))
                {
                    return FieldType.Integer;
                }
                if (graphQlFieldType.Name.Equals("Float", StringComparison.OrdinalIgnoreCase))
                {
                    return FieldType.Float;
                }
                if (graphQlFieldType.Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return FieldType.Boolean;
                }
            }
            else if (graphQlFieldType.Kind.Equals("Object", StringComparison.OrdinalIgnoreCase))
            {
                return FieldType.NestedSchema;
            }
            
            // If no match -> assume the type is a string
            return FieldType.String;
        }
        

        private ExternalNamespace[] GetNamespaces() =>
            _graphQlClient.Execute<ExternalNamespacesResponse>(new GraphQLRequest
                {Query = IntegrationApiRequests.NamespaceQuery}).TypedResponseData.ExternalNamespaces;
        
        private string GenerateEntityIdentityString(IEntityIdentity identity) {
            var sb = new StringBuilder();
            bool firstField = true;
            if (identity != null)
            {
                sb.Append("{");
                if (identity.NamespaceId != null)
                {
                    sb.Append("namespace: \"");
                    sb.Append(identity.NamespaceId);
                    sb.Append("\"");
                    firstField = false;
                }

                if (identity.Id != null)
                {
                    if (!firstField)
                    {
                        sb.Append(",");
                    }
                    sb.Append("id: \"");
                    sb.Append(identity.Id);
                    sb.Append("\"");
                    firstField = false;
                }

                if (identity.Type != null)
                {
                    if (!firstField)
                    {
                        sb.Append(",");
                    }
                    sb.Append("type: \"");
                    sb.Append(identity.Type);
                    sb.Append("\"");
                    firstField = false;
                }

                if (identity.LocaleId != null)
                {
                    if (!firstField)
                    {
                        sb.Append(",");
                    }
                    sb.Append(", localeId: \"");
                    sb.Append(identity.LocaleId);
                    sb.Append("\"");
                }

                sb.Append("}");
            }

            return sb.ToString();
        }
        
        private string GenerateEntityFragment(string namespaceId, string type) {
        //    System.Diagnostics.Debugger.Launch();
       //     System.Diagnostics.Debugger.Break();
            foreach (var ns in _namespaces) 
            {
                if (ns.Namespace.Equals(namespaceId)) 
                {
                    foreach (var nsType in ns.EntityTypes) 
                    {
                        if (nsType.Equals(type, StringComparison.OrdinalIgnoreCase)) 
                        {
                            var graphQLType = GetSchemaType(type);
                            if (graphQLType != null) 
                            {
                                var sb = new StringBuilder();
                                sb.Append("... on " + graphQLType.Name + " {\n");
                                outputFragment(graphQLType, sb);
                                sb.Append("}\n");
                                return sb.ToString();
                            }
                        }
                    }
                }
            }
            throw new ApiClientException($"Invalid namespace or type used. Namespace: {namespaceId}, Type: {type}");
        }

        private void outputFragment(GraphQLSchemaType graphQLType, StringBuilder sb) {
            foreach (var field in graphQLType.Fields) 
            {
                String typeName = field.Type.Name;
                if (typeName == null && field.Type != null) 
                {
                    typeName = field.Type.OfType.Name;
                }
                if (typeName != null &&
                    (typeName.Equals("ExternalIdentity", StringComparison.OrdinalIgnoreCase) || typeName.Equals("ExternalItemReference", StringComparison.OrdinalIgnoreCase))) {
                    continue;
                }
                sb.Append(field.Name + "\n");
                if (field.Type.Kind.Equals("OBJECT", StringComparison.OrdinalIgnoreCase) ||
                    (field.Type.Kind.Equals("LIST", StringComparison.OrdinalIgnoreCase) &&
                     !field.Type.OfType.Kind.Equals("SCALAR", StringComparison.OrdinalIgnoreCase))) 
                {
                    sb.Append("{\n");
                    GraphQLSchemaType nestedType = GetSchemaType(typeName);
                    outputFragment(nestedType, sb);
                    sb.Append("}\n");
                }
            }
        }

        private GraphQLSchemaType GetSchemaType(string typeName)
        {
            return _graphQlClient.Schema.Types
                .FirstOrDefault(type => type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }
        
        private IEntityIdentity ToEntityIdentity(Dictionary<string,object> identity)
        {
            // TODO: Use NewtonSoft here instead to deserialize the map to identity object
            StructureType structureType = StructureType.Leaf;
            String structureTypeStr = identity.GetOrNull("structureType")?.ToString();
            if (structureTypeStr != null)
            {
                StructureType.TryParse(structureTypeStr, out structureType);
            }
            return new EntityIdentity(identity.GetOrNull("id")?.ToString(), structureType)
            {
                NamespaceId = identity.GetOrNull("namespace")?.ToString(),
                Type = identity.GetOrNull("type")?.ToString(),
                LocaleId = identity.GetOrNull("localeId")?.ToString()
            };
        }
        
        private DynamicEntity ToEntity(Dictionary<string,object> dictionary) {
            DynamicEntity entity = new DynamicEntity();
            var id = dictionary.GetOrNull("identity");
            if (id != null) 
            {
                entity.Identity = ToEntityIdentity(ToDictionary(id));
            }
            var parentId = dictionary.GetOrNull("parentIdentity");
            if (parentId != null) 
            {
                entity.ParentIdentity = ToEntityIdentity(ToDictionary(parentId));
            }
            dictionary.Remove("identity");
            dictionary.Remove("parentIdentity");
            PopulateValueObject(entity, dictionary);
            return entity;
        }

        private void PopulateValueObject(IDynamicValueObject valueObject, Dictionary<string,object> dictionary) {
            foreach (var fieldName in dictionary.Keys) 
            {
                var fieldValue = dictionary.GetOrNull(fieldName);
                fieldValue = ProcessFieldValue(fieldValue);
                valueObject.SetField(fieldName, fieldValue);
            }
        }

        private Object ProcessFieldValue(Object fieldValue) {
            if (fieldValue is JObject nestedDictionary) 
            {
                NestedObject nestedObject = new NestedObject();
                PopulateValueObject(nestedObject, ToDictionary(nestedDictionary));
                fieldValue = nestedObject;
            }
            else if (fieldValue is JArray array)
            {
                if (array.Count > 0 && array.First.Type == JTokenType.Object)
                {
                    var dictionaryList = array.ToObject<List<Dictionary<string, object>>>();
                    var nestedObjectList = new List<NestedObject>();
                    foreach (var dictionary in dictionaryList)
                    {
                        NestedObject nestedObject = new NestedObject();
                        PopulateValueObject(nestedObject, dictionary);
                        nestedObjectList.Add(nestedObject);
                    }
                    fieldValue = nestedObjectList;
                }
                else
                {
                    fieldValue = array.ToObject<List<object>>();
                }
            }
            else if (fieldValue is string strValue)
            {
                if (strValue.Contains(BinaryReferenceHelper.CDBinaryReferencePrefix))
                {
                    fieldValue = BinaryReferenceHelper.DXABinaryReferencePrefix + BinaryReferenceHelper.EncodeBinaryUrl(strValue);
                }
            }
            return fieldValue;
        }

        private Dictionary<string, object> ToDictionary(object obj)
        {
            if (obj is JObject jobject)
            {
                return jobject.ToObject<Dictionary<string, object>>();
            }

            Log.Warn("Could not map JSON object to a dictionary. Make sure correct version of NewtonSoft is used.");
            return new Dictionary<string, object>();
        }
        
        private string GenerateEntityInput(DynamicEntity entity) {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            GenerateInput(entity, sb);
            sb.Append("}\n");
            return sb.ToString();
        }

        private void GenerateInput(IDynamicValueObject valueObject, StringBuilder sb) {
            // TODO: Use input type to make sure only writable fields are used...

            foreach (var fieldName in valueObject.GetFieldNames()) 
            {
                sb.Append(fieldName);
                sb.Append(": ");
                var fieldValue = valueObject.GetField(fieldName);
                if (fieldValue is IDynamicValueObject nestedObject) 
                {
                    sb.Append("{\n");
                    GenerateInput(nestedObject, sb);
                    sb.Append("}\n");
                }
                else if (fieldValue is IList<IDynamicValueObject> list) // TODO: Is this ever used?
                {
                    sb.Append("[\n");
                    foreach (var listItem in list)
                    {
                        sb.Append("{\n");
                        GenerateInput(listItem, sb);
                        sb.Append("}\n");
                    }
                    sb.Append("]\n");
                }
                else if (fieldValue is IList<NestedObject> nestedList)
                {
                    sb.Append("[\n");
                    bool firstValue = true;
                    foreach (var listItem in nestedList)
                    {
                        if (!firstValue)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            firstValue = false;
                        }
                        sb.Append("{\n");
                        GenerateInput(listItem, sb);
                        sb.Append("}\n");
                    }
                    sb.Append("]\n");
                }
                else if (fieldValue is IList<string> stringList)
                {
                    sb.Append("[");
                    bool firstValue = true;
                    foreach (var str in stringList)
                    {
                        if (!firstValue)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            firstValue = false;
                        }
                        sb.Append("\"" + str + "\"");
                    }
                    sb.Append("]\n");
                }
                else if (fieldValue is IList numberList)
                {
                    sb.Append($"[{string.Join(",", numberList)}]");
                }
                else 
                {
                    // TODO: Add support for DateTime here
                    if (fieldValue is string) {
                        sb.Append("\"" + fieldValue + "\"");
                    } else {
                        sb.Append(fieldValue);
                    }
                    sb.Append("\n");
                }
            }
        }

        private void InvokePreInterceptors(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
            // TODO: How to configure what types that are applicable???
            if (entityType == null)
            {
                entityType = identity?.Type;
            }
            foreach (var interceptor in DIRegisty.GetList<IIntegrationInterceptor>())
            {
                interceptor.PreProcess(identity, entity, namespaceId, entityType, operationType);
            }
        }
        
        private void InvokePostInterceptors(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
            if (entityType == null)
            {
                entityType = identity?.Type;
            }
            foreach (var interceptor in DIRegisty.GetList<IIntegrationInterceptor>())
            {
                interceptor.PostProcess(identity, entity, namespaceId, entityType, operationType);
            }
        }
    }
}