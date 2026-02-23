package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.connector.sdk.content.RootContentFolder;
import com.sdl.tridion.connectorframework.contracts.BinaryReference;

/**
 * Salesforce Root Folder.
 */
public class SalesforceRootFolder extends RootContentFolder {

    public SalesforceRootFolder(String namespaceId, String localeId) {
        super(namespaceId, localeId);
    }

    @Override
    public BinaryReference getIcon() {
       return Builder.newBinaryReference().id("salesforce").type("icon").build();
    }
}
