package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.sdl.tridion.connectorframework.connector.sdk.annotation.Schema;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.content.ContentFolderBase;
import com.sdl.tridion.connectorframework.contracts.EntityIdentity;
import com.sdl.tridion.connectorframework.contracts.StructureType;

/**
 * Salesforce Content Folder.
 */
@Schema(description = "Salesforce Content Folder")
public class SalesforceContentFolder extends ContentFolderBase {

    static public final String TYPE_NAME = SalesforceContentFolder.class.getSimpleName();

    public SalesforceContentFolder(String title, EntityIdentity parentId) {
        this.setParentIdentity(parentId);
        this.setIdentity(Builder.newEntityIdentity()
            .namespaceId(parentId.getNamespaceId())
            .id(title)
            .localeId(parentId.getLocaleId())
            .type(TYPE_NAME)
            .structureType(StructureType.Container)
            .build());
        this.setTitle(title);
    }

    public SalesforceContentFolder(EntityIdentity id) {
        this.setIdentity(id);
        this.setTitle(id.getId());
    }
}
