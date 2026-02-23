package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.connector.salesforce.model.ContentVersion;
import com.sdl.tridion.connectorframework.connector.salesforce.model.SalesforceContentFolder;
import com.sdl.tridion.connectorframework.connector.salesforce.model.SalesforceDataType;
import com.sdl.tridion.connectorframework.connector.salesforce.model.SalesforceDataTypeField;
import com.sdl.tridion.connectorframework.connector.sdk.DynamicEntity;
import com.sdl.tridion.connectorframework.connector.sdk.LoggerFactory;
import com.sdl.tridion.connectorframework.connector.sdk.RootEntity;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.contracts.*;
import com.sdl.tridion.connectorframework.contracts.capabilities.GetEntityCapability;
import com.sdl.tridion.connectorframework.contracts.capabilities.ListEntitiesCapability;
import com.sdl.tridion.connectorframework.contracts.connector.EntityCapabilityMetadata;
import com.sdl.tridion.connectorframework.contracts.contentmanager.capabilities.TemplateCapability;
import com.sdl.tridion.connectorframework.contracts.schema.EntitySchema;
import com.sdl.tridion.connectorframework.contracts.schema.SchemaFieldDefinition;
import org.slf4j.Logger;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.StringTokenizer;

import static com.sdl.tridion.connectorframework.connector.sdk.utils.PaginationUtils.recalculatePaginationData;

/**
 * Root List Service. Is used to navigate the tree from CME.
 * Is primarily used to find documents and form fields.
 */
public class SalesforceRootListService implements ListEntitiesCapability, GetEntityCapability, TemplateCapability {

    private static final Logger LOG = LoggerFactory.getLogger(SalesforceRootListService.class);

    static final String DATA_TYPES_FOLDER = "Data Types";

    private SalesforceCrudService crudService;
    private String namespaceId;
    private int defaultPageSize;
    private SalesforceConfiguration configuration;

    // TODO: Make sure that only make items writable if the user has access to modify metadata

    public SalesforceRootListService(SalesforceCrudService crudService, SalesforceConfiguration configuration, String namespaceId, int defaultPageSize) {
        this.crudService = crudService;
        this.namespaceId = namespaceId;
        this.defaultPageSize = defaultPageSize;
        this.configuration = configuration;
    }

    @Override
    public Entity getEntity(EntityIdentity identity, ConnectorContext connectorContext) {

        LOG.debug("Getting entity: " + identity.getId());
        if (identity.getType().equalsIgnoreCase(SalesforceContentFolder.TYPE_NAME)) {
            if (identity.getId().equalsIgnoreCase(DATA_TYPES_FOLDER)) {
                return new SalesforceContentFolder(DATA_TYPES_FOLDER, RootEntity.createRootEntityIdentity(identity.getNamespaceId(), identity.getLocaleId()));
            }
            if (identity.getId().equalsIgnoreCase(ContentVersion.PARENT_FOLDER_ID)) {
                return new SalesforceContentFolder(ContentVersion.PARENT_FOLDER_ID, RootEntity.createRootEntityIdentity(identity.getNamespaceId(), identity.getLocaleId()));
            }
        }
        if (identity.getType().equalsIgnoreCase(SalesforceDataType.TYPE_NAME)) {
            EntityIdentity parentIdentity = Builder.newEntityIdentity()
                    .namespaceId(identity.getNamespaceId())
                    .id(DATA_TYPES_FOLDER)
                    .type(SalesforceContentFolder.TYPE_NAME)
                    .localeId(identity.getLocaleId())
                    .structureType(StructureType.Container)
                    .build();
            return new SalesforceDataType(parentIdentity, identity);
        }
        if (identity.getType().equalsIgnoreCase(SalesforceDataTypeField.TYPE_NAME) && identity.getId().contains(SalesforceDataTypeField.ID_DELIMITER)) {
            StringTokenizer tokenizer = new StringTokenizer(identity.getId(), SalesforceDataTypeField.ID_DELIMITER);
            String dataTypeName = tokenizer.nextToken();
            String fieldName = tokenizer.nextToken();
            EntitySchema schema = this.crudService.getEntitySchema(dataTypeName);
            if (schema != null) {
                SchemaFieldDefinition field = schema.getFields().stream().filter(f -> f.getName().equalsIgnoreCase(fieldName)).findFirst().get();
                EntityIdentity parentIdentity = Builder.newEntityIdentity()
                        .namespaceId(identity.getNamespaceId())
                        .id(dataTypeName)
                        .type(SalesforceDataType.TYPE_NAME)
                        .localeId(identity.getLocaleId())
                        .structureType(StructureType.Container)
                        .build();
                return new SalesforceDataTypeField(parentIdentity, identity, field);
            }
        }
        return null;
    }

    @Override
    public EntityPaginatedList listEntities(EntityIdentity parentEntity, PaginationData paginationData, StructureTypeSelector structureTypeSelector, ConnectorContext context) {

        LOG.debug("Listing folder: " + parentEntity.getId());
        paginationData = recalculatePaginationData(paginationData, this.defaultPageSize);

        int totalCount = 0;
        List<Entity> resultList = new ArrayList<>();

        // List root folders
        //
        if (parentEntity.getId().equals(RootEntity.DEFAULT_ROOT_ID) && parentEntity.getType().equals(RootEntity.DEFAULT_ROOT_TYPE)) {
            if (configuration.isExposeDataTypes()) {
                resultList.add(new SalesforceContentFolder(DATA_TYPES_FOLDER, parentEntity));
            }
            resultList.add(new SalesforceContentFolder(ContentVersion.PARENT_FOLDER_ID, parentEntity));

        } else if (parentEntity.getId().equals(DATA_TYPES_FOLDER)) {
            this.listDataTypes(parentEntity, resultList);
        } else if (parentEntity.getType().equalsIgnoreCase(SalesforceDataType.TYPE_NAME) && structureTypeSelector != StructureTypeSelector.ContainersOnly) {
            this.listDataTypeFields(parentEntity, resultList);
        } else if (parentEntity.getId().equals(ContentVersion.PARENT_FOLDER_ID) && structureTypeSelector != StructureTypeSelector.ContainersOnly) {


            // List all files
            //
            EntityPaginatedList result = crudService.queryEntities(
                    Builder.newEntityFilter()
                        .context(parentEntity)
                        .entityType(ContentVersion.TYPE_NAME)
                        .build(),
                    paginationData, context);

            // Set parent in the items in the result set
            //
            result.getEntities()
                    .forEach(e -> ((DynamicEntity) e).setParentIdentity(parentEntity));

            return result;
        }

        totalCount = resultList.size();
        return Builder.newEntityPaginatedList()
                .entities(resultList)
                .totalCount(totalCount)
                .startIndex(paginationData.getStartIndex())
                .pageSize(paginationData.getPageSize())
                .build();
    }

    @Override
    public String getTemplateFragment(EntityIdentity identity, Map<String, String> attributes, ConnectorContext context) {
        return null;
    }

    @Override
    public String getDirectLinkToPublished(EntityIdentity identity, Map<String, String> attributes, ConnectorContext context) {
        if (identity.getType().equalsIgnoreCase(SalesforceDataTypeField.TYPE_NAME)) {
            // Just to return something to not make DXA templates to publish binary.
            //
            return identity.getId().replace(SalesforceDataTypeField.ID_DELIMITER, "/");
        }
        return null;
    }

    public List<EntityCapabilityMetadata> getCapabilities() {

        List<EntityCapabilityMetadata> capabilities = new ArrayList<>();

        capabilities.add(Builder.newEntityCapabilityMetadata()
                .namespaceId(namespaceId)
                .entitySchema(Builder.newEntitySchema()
                        .entityClass(SalesforceContentFolder.class)
                        .build())
                .type(SalesforceContentFolder.TYPE_NAME)
                .capability(this)
                .build());

        if (configuration.isExposeDataTypes()) {
            capabilities.add(Builder.newEntityCapabilityMetadata()
                    .namespaceId(namespaceId)
                    .entitySchema(Builder.newEntitySchema()
                            .entityClass(SalesforceDataType.class)
                            .build())
                    .type(SalesforceDataType.TYPE_NAME)
                    .capability(this)
                    .build());
            capabilities.add(Builder.newEntityCapabilityMetadata()
                    .namespaceId(namespaceId)
                    .entitySchema(Builder.newEntitySchema()
                            .entityClass(SalesforceDataTypeField.class)
                            .build())
                    .type(SalesforceDataTypeField.TYPE_NAME)
                    .capability(this)
                    .build());
        }

        return capabilities;

    }

    private void listDataTypes(EntityIdentity parentEntity, List<Entity> resultList) {
        for (String typeName : this.configuration.getEntityTypes()) {
            if (!typeName.equalsIgnoreCase(ContentVersion.TYPE_NAME))
            resultList.add(new SalesforceDataType(parentEntity, typeName));
        }
    }

    private void listDataTypeFields(EntityIdentity parentIdentity, List<Entity> resultList) {

        // TODO: How to handle nested fields here?

        String typeName = parentIdentity.getId();
        EntitySchema schema = this.crudService.getEntitySchema(typeName);
        for (SchemaFieldDefinition field : schema.getFields()) {
            if (field.getContractQualifier() == null ||
                    (!field.getContractQualifier().equalsIgnoreCase("Content") &&
                     !field.getContractQualifier().equalsIgnoreCase("Multimedia"))) {
                resultList.add(new SalesforceDataTypeField(parentIdentity, null, field));
            }
        }
    }
}
