package com.sdl.tridion.connectorframework.connector.salesforce.model;

import com.sdl.tridion.connectorframework.connector.sdk.content.DynamicMultimedia;
import com.sdl.tridion.connectorframework.contracts.BinaryReference;
import com.sdl.tridion.connectorframework.contracts.Entity;
import com.sdl.tridion.connectorframework.contracts.EntityIdentity;
import com.sdl.tridion.connectorframework.contracts.contentmanager.ContentManagerItem;

import java.util.Map;

/**
 * Extension of DynamicMultimedia that also implements the ContentManagerItem interface.
 * Workaround until Connector Framework SDK has been updated.
 */
public class DynamicContentManagerMultimedia extends DynamicMultimedia implements ContentManagerItem {

    public DynamicContentManagerMultimedia() {
    }

    public DynamicContentManagerMultimedia(Entity entity) {
        super(entity);
    }

    public DynamicContentManagerMultimedia(EntityIdentity parentIdentity, EntityIdentity identity, Map<String, Object> fields) {
        super(parentIdentity, identity, fields);
    }

    @Override
    public BinaryReference getIcon() {
        return (BinaryReference) this.getField("Icon");
    }

    @Override
    public BinaryReference getThumbnail() {
        return (BinaryReference) this.getField("Thumbnail");
    }

    @Override
    public String getThumbnailETag() {
        return (String) this.getField("ThumbnailETag");
    }
}
