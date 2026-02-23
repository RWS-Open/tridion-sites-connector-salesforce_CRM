package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.sdl.tridion.connectorframework.connector.sdk.annotation.Schema;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.content.ContentFolderBase;
import com.sdl.tridion.connectorframework.contracts.EntityIdentity;
import com.sdl.tridion.connectorframework.contracts.StructureType;

/**
 * Salesforce Data Type.
 */
@Schema(description = "Salesforce Data Type")
public class SalesforceDataType extends ContentFolderBase {

    static public final String TYPE_NAME = SalesforceDataType.class.getSimpleName();

    public SalesforceDataType(EntityIdentity parentIdentity, String typeName) {
        this.setParentIdentity(parentIdentity);
        this.setIdentity(Builder.newEntityIdentity()
            .namespaceId(parentIdentity.getNamespaceId())
            .type(TYPE_NAME)
            .id(typeName)
            .localeId(parentIdentity.getLocaleId())
            .structureType(StructureType.Container)
            .build());
        this.setTitle(typeName);
    }

    public SalesforceDataType(EntityIdentity parentIdentity, EntityIdentity identity) {
        this.setParentIdentity(parentIdentity);
        this.setIdentity(identity);
        this.setTitle(identity.getId());
    }

}
