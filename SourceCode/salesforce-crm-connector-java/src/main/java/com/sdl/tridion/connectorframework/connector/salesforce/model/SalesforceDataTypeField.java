package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.sdl.tridion.connectorframework.connector.sdk.annotation.Schema;
import com.sdl.tridion.connectorframework.connector.sdk.annotation.SchemaField;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.content.MultimediaBase;
import com.sdl.tridion.connectorframework.contracts.EntityIdentity;
import com.sdl.tridion.connectorframework.contracts.StructureType;
import com.sdl.tridion.connectorframework.contracts.schema.SchemaFieldDefinition;

@Schema(description = "Salesforce Data Type Field")
public class SalesforceDataTypeField extends MultimediaBase {

    static public final String TYPE_NAME = SalesforceDataTypeField.class.getSimpleName();
    static public final String ID_DELIMITER = "~";

    @SchemaField(description = "Description")
    private String description;

    @SchemaField(description = "Field Type")
    private String fieldType;

    @SchemaField(description = "Read only")
    private String readonly;

    // TODO: What will happen here at publish when the item is a content item? Will the publish link be used?

    public SalesforceDataTypeField(EntityIdentity parentIdentity, EntityIdentity identity, SchemaFieldDefinition field) {
        this.setParentIdentity(parentIdentity);
        if (identity == null) {
            this.setIdentity(Builder.newEntityIdentity()
                    .namespaceId(parentIdentity.getNamespaceId())
                    .type(TYPE_NAME)
                    .id(parentIdentity.getId() + ID_DELIMITER + field.getName())
                    .localeId(parentIdentity.getLocaleId())
                    .structureType(StructureType.Leaf)
                    .build());
        } else {
            this.setIdentity(identity);
        }
        this.setTitle(field.getName());
        String iconId = field.isReadonly() ? "ro-field" : "rw-field";
        this.setIcon(Builder.newBinaryReference()
                .namespaceId(this.getIdentity().getNamespaceId())
                .id(iconId)
                .type("icon")
                .build());

        this.description = field.getDescription();
        this.fieldType = field.getType().name();
        this.readonly = "" + field.isReadonly();
        this.setFilename(field.getName());
    }

    public String getDescription() {
        return description;
    }

    public String getFieldType() {
        return fieldType;
    }

    public String getReadonly() {
        return readonly;
    }

    // TODO: Add further fields here
}
