package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.connector.sdk.DynamicConnectorConfiguration;
import com.sdl.tridion.connectorframework.connector.sdk.DynamicEntity;
import com.sdl.tridion.connectorframework.connector.sdk.RootEntity;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.contracts.*;
import com.sdl.tridion.connectorframework.contracts.capabilities.CreateEntityCapability;
import com.sdl.tridion.connectorframework.contracts.capabilities.GetEntityCapability;
import com.sdl.tridion.connectorframework.contracts.capabilities.ListEntitiesCapability;
import com.sdl.tridion.connectorframework.contracts.connector.Connector;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;

public class TestSalesforceConnector {

    private static final Logger LOG = LoggerFactory.getLogger(TestSalesforceConnector.class);

    private Connector connector;

    @Before
    public void initialize() {
        connector = new SalesforceConnector();
        DynamicConnectorConfiguration config = new DynamicConnectorConfiguration();
        config.setField("username", "");
        config.setField("password", "");
        config.setField("exposeDataTypes", true);

        List<String> entityTypes = new ArrayList<>();
        entityTypes.add("Account");
        entityTypes.add("Contact");
        entityTypes.add("Lead");
        entityTypes.add("ContentVersion");
        //entityTypes.add("VisitorTracking");
        config.setField("entityTypes", entityTypes);

        connector.initialize("salesforce", config,new TestConnectorLogger(), new ArrayList<>());
        connector.start();

    }

    @Test
    public void testCreateCustomObject() {

        CreateEntityCapability createEntityCapability = (CreateEntityCapability) connector.getCapability("CreateEntity", "VisitorTracking", null);

        EntityIdentity parentId = Builder.newEntityIdentity()
                .namespaceId("salesforce")
                .id("0034J00000LGkdUQAT")
                .type("Contact")
                .build();

        DynamicEntity entity = new DynamicEntity();
        entity.setField("sessionId", "1234567");
        entity.setField("siteUrl", "http://test.com/page1");
        List<String> categories = new ArrayList<>();
        categories.add("Product");
        categories.add("Campaign");
        entity.setField("categories", categories);

        LOG.info("Creating new 'VisitorTracking' entity in Salesforce CRM...");
        EntityIdentity createdId = createEntityCapability.createEntity(parentId, "VisitorTracking", entity, null);

        LOG.info("Entity created with ID: " + createdId.getId());
    }

    @Test
    public void testNavigateStructure() {
        ListEntitiesCapability listCapability = (ListEntitiesCapability) connector.getCapability("ListEntities", null);

        PaginationData paginationData = Builder.newPaginatedData().build();
        EntityPaginatedList result = listCapability.listEntities(RootEntity.createRootEntityIdentity("salesforce", null), paginationData, StructureTypeSelector.All, null);
        listFolder(result, listCapability, 0);
    }

    private void listFolder(EntityPaginatedList result, ListEntitiesCapability listCapability, int level) {
        for (Entity entity : result.getEntities()) {
            LOG.info(spaces(level) + entity.getIdentity().getId() + " (" + entity.getIdentity().getType() + ")");
            if (entity.getIdentity().getStructureType() == StructureType.Container) {
                PaginationData paginationData = Builder.newPaginatedData().build();
                EntityPaginatedList nestedResult = listCapability.listEntities(entity.getIdentity(), paginationData, StructureTypeSelector.All, null);
                listFolder(nestedResult, listCapability, level+1);
            } else {

            }
        }
    }

    /*
    private void printParent(EntityIdentity entityIdentity) {
        GetEntityCapability getCapability = (GetEntityCapability) connector.getCapability("GetEntity", entityIdentity.getType(), null);
        if (getCapability != null) {
            Entity nestedEntity = getCapability.getEntity(entity.getIdentity(), null);
            LOG.info(spaces(level) + " Parent ID: " + nestedEntity.getParentIdentity().getId() + " (" + nestedEntity.getParentIdentity().getType() + ")");
        }
    }

     */

    private String spaces(int no) {
        String spaces = "";
        for (int i=0; i < no*2; i++) {
            spaces += " ";
        }
        return spaces;
    }
}
