package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.force.api.DescribeSObject;
import com.sdl.tridion.connectorframework.connector.sdk.DynamicEntity;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.builder.EntitySchemaImpl;
import com.sdl.tridion.connectorframework.connector.sdk.content.MultimediaBase;
import com.sdl.tridion.connectorframework.contracts.BinaryReference;
import com.sdl.tridion.connectorframework.contracts.EntityIdentity;
import com.sdl.tridion.connectorframework.contracts.StructureType;
import com.sdl.tridion.connectorframework.contracts.schema.EntitySchema;
import com.sdl.tridion.connectorframework.contracts.schema.SchemaFieldDefinition;

import java.net.FileNameMap;
import java.net.URLConnection;
import java.util.HashMap;
import java.util.Map;

import static com.sdl.tridion.connectorframework.connector.salesforce.SalesforceUtils.getFieldType;

/**
 * Content Version.
 * Handles Salesforce file assets.
 */
public final class ContentVersion extends MultimediaBase {

    public static final String TYPE_NAME = ContentVersion.class.getSimpleName();
    public static final String PARENT_FOLDER_ID = "Files";

    private static FileNameMap fileNameMap = URLConnection.getFileNameMap();
    private static Map<String,String> nameMappings = new HashMap<>();

    static {
        nameMappings.put("CreatedAt", "CreatedDate");
        nameMappings.put("CreatedBy", "CreatedById");
        nameMappings.put("LastModifiedAt", "LastModifiedDate");
        nameMappings.put("LastModifiedBy", "LastModifiedById");
    }

    private ContentVersion() {
    }

    static public EntitySchema createSchema(DescribeSObject salesforceSchema) {

        EntitySchema assetSchema = Builder.newEntitySchema()
                .entityClass(ContentVersion.class)
                .build();

        EntitySchemaImpl.Builder schemaBuilder = Builder.newEntitySchema()
                .name(salesforceSchema.getName())
                .description(salesforceSchema.getLabel())
                .contractQualifiers(assetSchema.getContractQualifiers());

        for (SchemaFieldDefinition field : assetSchema.getFields()) {
            if (field.getContractQualifier() != null) {
                schemaBuilder.field(field);
            }
        }

        for (DescribeSObject.Field fieldDescription : salesforceSchema.getFields()) {
            if (!fieldDescription.getName().equals("Id") &&
                    !fieldDescription.getName().equals("Title") &&
                    !fieldDescription.getName().equals("CreatedDate") &&
                    !fieldDescription.getName().equals("CreatedById") &&
                    !fieldDescription.getName().equals("LastModifiedDate") &&
                    !fieldDescription.getName().equals("LastModifiedById")) {
                schemaBuilder.field(
                        Builder.newSchemaFieldDefinition()
                                .name(fieldDescription.getName())
                                .description(fieldDescription.getLabel())
                                .isReadonly(!fieldDescription.isUpdateable())
                                .type(getFieldType(fieldDescription.getType()))
                                .build()
                );

            }
        }

        return schemaBuilder.build();
    }

    static public Map<String,String> getMappings() {
        return nameMappings;
    }

    static public DynamicEntity enrich(DynamicEntity entity) {

        EntityIdentity identity = entity.getIdentity();
        entity.setParentIdentity(
                Builder.newEntityIdentity()
                    .namespaceId(identity.getNamespaceId())
                    .id(PARENT_FOLDER_ID)
                    .type(SalesforceContentFolder.TYPE_NAME)
                    .localeId(identity.getLocaleId())
                    .structureType(StructureType.Container)
                    .build());

        String extension = (String) entity.getField("FileExtension");
        String contentType = null;
        if (extension != null) {
            contentType = getContentType(extension);
            entity.setField("ContentType", contentType);
        }
        entity.setField("CanUpdate", true); // TODO: Check the schema here??
        entity.setField("Filename", entity.getField("PathOnClient"));

        //this.setIcon(); // TODO: Define icons for the standard types

        String binaryId = entity.getIdentity().getId() + "-" + entity.getField("ContentSize") + "." + extension;

        BinaryReference binaryReference = Builder.newBinaryReference()
                .namespaceId(entity.getIdentity().getNamespaceId())
                .id(binaryId)
                .type("file")
                .build();
        entity.setField("BinaryReference", binaryReference);
        if (contentType != null && contentType.startsWith("image")) {
            entity.setField("Thumbnail", binaryReference);
        }

        return new DynamicContentManagerMultimedia(entity);
    }

    static public String getContentType(String fileExtension) {
        return fileNameMap.getContentTypeFor("file." + fileExtension);
    }
}
