package com.sdl.tridion.connectorframework.connector.salesforce;

import com.force.api.ApiException;
import com.force.api.DescribeSObject;
import com.force.api.QueryResult;
import com.sdl.tridion.connectorframework.connector.salesforce.model.ContentVersion;
import com.sdl.tridion.connectorframework.connector.sdk.DynamicEntity;
import com.sdl.tridion.connectorframework.connector.sdk.LoggerFactory;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.builder.EntitySchemaImpl;
import com.sdl.tridion.connectorframework.connector.sdk.utils.FieldUtils;
import com.sdl.tridion.connectorframework.contracts.*;
import com.sdl.tridion.connectorframework.contracts.capabilities.*;
import com.sdl.tridion.connectorframework.contracts.connector.EntityCapabilityMetadata;
import com.sdl.tridion.connectorframework.contracts.exception.*;
import com.sdl.tridion.connectorframework.contracts.schema.EntitySchema;
import com.sdl.tridion.remoting.contracts.DynamicValueObject;
import org.slf4j.Logger;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import static com.sdl.tridion.connectorframework.connector.salesforce.SalesforceUtils.*;
import static com.sdl.tridion.connectorframework.connector.sdk.utils.PaginationUtils.recalculatePaginationData;

/**
 * Salesforce CRUD Service.
 */
public class SalesforceCrudService implements GetEntityCapability, ListEntitiesCapability, QueryEntitiesCapability, CreateEntityCapability, UpdateEntityCapability, DeleteEntityCapability {

    private static final Logger LOG = LoggerFactory.getLogger(SalesforceCrudService.class);

    private SalesforceClient client;
    private List<EntityCapabilityMetadata> capabilities = new ArrayList<>();
    private Map<String, DescribeSObject> salesforceSchemas = new HashMap<>();
    private Map<String, Map<String,String>> nameMappings = new HashMap<>();
    private SalesforceConfiguration configuration;
    private String namespaceId;
    private int defaultPageSize;

    static final int NOT_FOUND = 404;
    static final String CUSTOM_TYPE_SUFFIX = "__c";

    // TODO: Have support for virtual fields like 'Name' which is expanded into 'firstName' & 'lastName' ??
    // TODO: Have a configurable sort order for results
    // TODO: Improve the list using context. Seems to not work that good for accounts as owner for example

    /**
     * Facade that only expose the get & update capabilities.
     */
    class GetUpdateService implements GetEntityCapability, ListEntitiesCapability, QueryEntitiesCapability, UpdateEntityCapability {

        @Override
        public Entity getEntity(EntityIdentity identity, ConnectorContext context) {
            return SalesforceCrudService.this.getEntity(identity, context);
        }

        @Override
        public EntityPaginatedList listEntities(EntityIdentity parentEntity, PaginationData paginationData, StructureTypeSelector structureTypeSelector, ConnectorContext context) {
            return SalesforceCrudService.this.listEntities(parentEntity, paginationData, structureTypeSelector, context);
        }

        @Override
        public EntityPaginatedList queryEntities(EntityFilter filter, PaginationData paginationData, ConnectorContext context) {
            return SalesforceCrudService.this.queryEntities(filter, paginationData, context);
        }

        @Override
        public Entity updateEntity(EntityIdentity identity, DynamicValueObject changedFields, ConnectorContext context) {
            return SalesforceCrudService.this.updateEntity(identity, changedFields, context);
        }
    }

    public SalesforceCrudService(SalesforceClient client, SalesforceConfiguration configuration, String namespaceId, int defaultPageSize) {
        this.client = client;
        this.namespaceId = namespaceId;
        this.defaultPageSize = defaultPageSize;
        this.configuration = configuration;
        this.readCapabilities();
    }

    public EntityIdentity createEntity(EntityIdentity parentId, String type, Entity entity, ConnectorContext context) {

        EntitySchema schema = getEntitySchema(type);
        //DescribeSObject salesforceSchema = salesforceSchemas.get(type);
        Map<String,String> nameMappings = this.nameMappings.get(schema.getName());

        // TODO: Validate here??

        DynamicValueObject valueObject = entity instanceof DynamicValueObject ? (DynamicValueObject) entity : new DynamicEntity(entity);
        Map<String, Object> entityMap = toMap(valueObject, schema, nameMappings, true);
        if (parentId != null && !parentId.getId().equals("root") ) {//&& salesforceSchema != null && salesforceSchema.getAllFields().stream().findFirst(f -> )) {

            // Use the following convention for parent relationships: [entity name]Id
            //
            String parentFieldIdName = toSalesforceFieldName(parentId.getType() + "Id", nameMappings);
            entityMap.put(parentFieldIdName, parentId.getId());

            // TODO: Can the parent field be figured out instead of using the above convention???
        }

        try {
            String id = this.client.createObject(toSalesforceFieldName(type, nameMappings), entityMap);
            return Builder.newEntityIdentity()
                    .namespaceId(namespaceId)
                    .id(id)
                    .type(type)
                    .build();
        }
        catch (ApiException e) {
            throw createException(e);
        }
    }

    public Entity updateEntity(EntityIdentity identity, DynamicValueObject changedFields, ConnectorContext context) {

        EntitySchema schema = getEntitySchema(identity.getType());
        Map<String,String> nameMappings = this.nameMappings.get(schema.getName());
        try {
            this.client.updateObject(toSalesforceFieldName(identity.getType(), nameMappings), identity.getId(),
                    toMap(changedFields, schema, nameMappings, true));
            return getEntity(identity, context);
        }
        catch (ApiException e) {
            throw createException(e, identity);
        }
    }

    public void deleteEntity(EntityIdentity identity, ConnectorContext context) {
        try {
            Map<String,String> nameMappings = this.nameMappings.get(identity.getType());
            this.client.deleteObject(toSalesforceFieldName(identity.getType(), nameMappings), identity.getId());
        }
        catch (ApiException e) {
           throw createException(e, identity);
        }
    }

    public Entity getEntity(EntityIdentity identity, ConnectorContext connectorContext) {
        EntitySchema schema = getEntitySchema(identity.getType());
        try {
            Map<String,String> nameMappings = this.nameMappings.get(schema.getName());
            Map map = client.getObject(toSalesforceFieldName(identity.getType(), nameMappings), identity.getId());
            DynamicEntity entity = toEntity(map, schema, nameMappings);
            entity.setIdentity(Builder.newEntityIdentity()
                .namespaceId(this.namespaceId)
                .id(identity.getId())
                .type(identity.getType())
                .localeId(identity.getLocaleId())
                .structureType(StructureType.Leaf)
                .build());
            if (identity.getType().equalsIgnoreCase(ContentVersion.TYPE_NAME)) {
                entity = ContentVersion.enrich(entity);
            }
            return entity;
        }
        catch (ApiException e) {
            throw createException(e, identity);
        }
    }

    @Override
    public EntityPaginatedList listEntities(EntityIdentity parentEntity, PaginationData paginationData, StructureTypeSelector structureTypeSelector, ConnectorContext context) {
        paginationData = recalculatePaginationData(paginationData, this.defaultPageSize);

        // TODO: This needs to extended in the Connector Framework contracts so entity type can be passed in the query.
        // TODO: How do we handle pagination when having multiple entities?

        EntitySchema parentSchema = getEntitySchema(parentEntity.getType());
        if (parentSchema == null) {
            throw new ValidationException("Invalid type used in parent identity when performing a list operation");
        }
        DescribeSObject parentSalesforceSchema = salesforceSchemas.get(parentEntity.getType());
        for (DescribeSObject.ChildEntity childEntity : parentSalesforceSchema.getChildRelationships()) {

            String typeName = childEntity.getChildSObject();

            if (typeName.equalsIgnoreCase(parentEntity.getType()) || typeName.equalsIgnoreCase(parentEntity.getType() + CUSTOM_TYPE_SUFFIX)) {
                // For now skip recursive structures
                //
                continue;
            }
            EntitySchema schema = null;
            for (EntityCapabilityMetadata capabilityMetadata : this.capabilities) {
                if (capabilityMetadata.getType().equalsIgnoreCase(typeName)) {
                    schema = capabilityMetadata.getEntitySchema();
                }
            }
            if (schema != null) {

                // For now just pick the first relationship found that are available in the configured entity types
                //
                Map<String,String> nameMappings = this.nameMappings.get(schema.getName());
                DescribeSObject salesforceSchema = salesforceSchemas.get(typeName);
                List<String> fieldNames = salesforceSchema.getAllFields().stream()
                        .map(f -> f.getName()).collect(Collectors.toList());
                try {
                    QueryResult<Map> result = client.listObjects(
                            typeName,
                            fieldNames,
                            childEntity.getField() + "='" + parentEntity.getId() + "'",
                            paginationData.getPageSize(),
                            paginationData.getStartIndex());

                    List<Entity> entities = new ArrayList<>();
                    for (Map resultItem : result.getRecords()) {
                        DynamicEntity entity = toEntity(resultItem, schema, nameMappings);
                        entity.setIdentity(Builder.newEntityIdentity()
                                .namespaceId(namespaceId)
                                .id(resultItem.get("Id").toString())
                                .type(typeName)
                                .localeId(parentEntity.getLocaleId())
                                .build());
                        entities.add(entity);
                    }

                    return Builder.newEntityPaginatedList()
                            .entities(entities)
                            .pageSize(paginationData.getPageSize())
                            .pageIndex(paginationData.getStartIndex())
                            .totalCount(result.getTotalSize())
                            .build();
                }
                catch (ApiException e) {
                    throw createException(e);
                }


            }

            // TODO: Clean up this!!
            // TODO: Recalc the limits

            // Max items: 10
            // Contacts: 10
            // No lead query

            // Need to queries to all before correct index can be calc

            /*
            Algorithm:
            1. Send query1 with offset
            2. If empty or not filled up to max -> send query2 with offset=paginationData.startIndex+query1.resultSize
            3

             */
        }

        return null;
    }

    public EntityPaginatedList queryEntities(EntityFilter filter, PaginationData paginationData, ConnectorContext context) {

        paginationData = recalculatePaginationData(paginationData, this.defaultPageSize);
        String localeId = filter.getContext() != null ? filter.getContext().getLocaleId() : null;

        // TODO: Use context in the search as well to narrow a search to a specific account etc

        EntitySchema schema = getEntitySchema(filter.getEntityType());
        Map<String,String> nameMappings = this.nameMappings.get(schema.getName());
        DescribeSObject salesforceSchema = salesforceSchemas.get(filter.getEntityType());
        List<String> fieldNames = salesforceSchema.getAllFields().stream()
                .map(f -> f.getName()).collect(Collectors.toList());

        try {
            QueryResult<Map> result;
            if (filter.getSearchText() != null) {
                result = client.searchObjects(toSalesforceFieldName(filter.getEntityType(), nameMappings),
                        fieldNames, filter.getSearchText(), paginationData.getPageSize(), paginationData.getStartIndex());
            } else {
                String condition = null;
                if (filter.getEntityType().equalsIgnoreCase(ContentVersion.TYPE_NAME)) {
                    condition = "IsLatest=true";
                }
                result = client.listObjects(toSalesforceFieldName(filter.getEntityType(), nameMappings), fieldNames, condition, paginationData.getPageSize(), paginationData.getStartIndex());
            }

            List<Entity> entities = new ArrayList<>();
            for (Map resultItem : result.getRecords()) {
                DynamicEntity entity = toEntity(resultItem, schema, nameMappings);
                entity.setIdentity(Builder.newEntityIdentity()
                        .namespaceId(namespaceId)
                        .id(resultItem.get("Id").toString())
                        .type(filter.getEntityType())
                        .localeId(localeId)
                        .build());
                if (filter.getEntityType().equalsIgnoreCase(ContentVersion.TYPE_NAME)) {
                    entity = ContentVersion.enrich(entity);
                }
                entities.add(entity);
            }

            return Builder.newEntityPaginatedList()
                    .entities(entities)
                    .pageSize(paginationData.getPageSize())
                    .pageIndex(paginationData.getStartIndex())
                    .totalCount(result.getTotalSize())
                    .build();
        }
        catch (ApiException e) {
            throw createException(e);
        }
    }

    public List<EntityCapabilityMetadata> getCapabilities() {
        return capabilities;
    }

    public EntitySchema getEntitySchema(String entityType) {
        if (entityType != null) {
            for (EntityCapabilityMetadata capabilityMetadata : this.capabilities) {
                if (capabilityMetadata.getType().equalsIgnoreCase(entityType)) {
                    return capabilityMetadata.getEntitySchema();
                }
            }
        }
        throw new ValidationException("Unrecognized entity type: " + entityType);
    }

    private void readCapabilities() {

        if (configuration.getEntityTypes() == null || configuration.getEntityTypes().isEmpty())  {
            throw new ConfigurationException("Missing 'entityTypes' in the configuration.");
        }
        for (String entityType : this.configuration.getEntityTypes()) {
            LOG.info("Setting up Salesforce entity '" + entityType + "'...");
            // TODO: Define a SalesforceSchema class to abstract the native object description
            DescribeSObject salesforceSchema;
            boolean isCustomEntityType = false;

            try {
                salesforceSchema = client.getObjectDescription(entityType);
            }
            catch (ApiException e) {
                if (e.getCode() == NOT_FOUND) {
                    // Get object description as a custom type
                    //
                    try {
                        salesforceSchema = client.getObjectDescription(entityType + CUSTOM_TYPE_SUFFIX);
                        isCustomEntityType = true;
                    }
                    catch (ApiException e2) {
                        LOG.error("Could not get entity type: " + entityType, e2);
                        continue;
                    }
                }
                else {
                    LOG.error("Could not get entity type: " + entityType, e);
                    continue;
                }

            }
            salesforceSchemas.put(entityType, salesforceSchema);

            EntitySchema schema;
            ConnectorCapability entityCapabilities;

            if (entityType.equalsIgnoreCase(ContentVersion.TYPE_NAME)) {
                schema = ContentVersion.createSchema(salesforceSchema);
                entityCapabilities = new GetUpdateService();
                nameMappings.put(entityType, ContentVersion.getMappings());
            } else {

                Map<String,String> entityNameMappings = new HashMap<>();
                EntitySchemaImpl.Builder schemaBuilder = Builder.newEntitySchema()
                        .name(entityType)
                        .description(salesforceSchema.getLabel());
                if (isCustomEntityType) {
                    entityNameMappings.put(entityType, salesforceSchema.getName());
                }

                for (DescribeSObject.Field fieldDescription : salesforceSchema.getFields()) {
                    String fieldName = fieldDescription.getName();
                    if (!fieldName.equals("Id") && !isCompositeField(fieldDescription)) {

                        if (fieldName.endsWith(CUSTOM_TYPE_SUFFIX)) {
                            String schemaFieldName = fieldName.replace(CUSTOM_TYPE_SUFFIX, "");
                            if (configuration.getNamespacePrefix() != null && schemaFieldName.startsWith(configuration.getNamespacePrefix())) {
                                schemaFieldName = schemaFieldName.replace(configuration.getNamespacePrefix(), "");
                            }
                            schemaFieldName = FieldUtils.toPascalCase(schemaFieldName);
                            entityNameMappings.put(schemaFieldName, fieldName);
                            fieldName = schemaFieldName;
                        }
                        schemaBuilder.field(
                                Builder.newSchemaFieldDefinition()
                                        .name(fieldName)
                                        .description(fieldDescription.getLabel())
                                        .isReadonly(!fieldDescription.isUpdateable())
                                        .type(getFieldType(fieldDescription.getType()))
                                        .isList(isListType(fieldDescription.getType()))
                                        .build()
                        );

                    }
                }

                schema = schemaBuilder.build();
                entityCapabilities = this;
                if (!entityNameMappings.isEmpty()) {
                    nameMappings.put(entityType, entityNameMappings);
                }
            }

            // TODO: Skip all groupable fields and create nested type for those

            capabilities.add(
                Builder.newEntityCapabilityMetadata()
                    .namespaceId(namespaceId)
                    .type(entityType)
                    .entitySchema(schema)
                    .capability(entityCapabilities)
                    .build());

            // TODO: If picklist add the allowed values as constraints

        }

    }

    private boolean isCompositeField(DescribeSObject.Field fieldDescription) {
        return fieldDescription.getSoapType().startsWith("urn:");
    }

    private ConnectorException createException(ApiException apiException) {
        return createException(apiException, null);
    }

    private ConnectorException createException(ApiException apiException, EntityIdentity identity) {
        if (apiException.getCode() == NOT_FOUND && identity != null) {
            return new EntityNotFoundException("Could not find entity of type '" + identity.getType() + "' and ID '" + identity.getId() + "'", apiException);
        }
        // TODO: Add more error mappings here
        return new SystemException("Salesforce error received.", apiException);
    }
}
