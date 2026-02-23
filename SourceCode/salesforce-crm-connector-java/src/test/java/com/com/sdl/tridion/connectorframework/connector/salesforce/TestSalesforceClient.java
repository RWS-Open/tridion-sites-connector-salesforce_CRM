package com.sdl.tridion.connectorframework.connector.salesforce;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.force.api.*;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.contracts.*;
import com.sdl.tridion.connectorframework.contracts.capabilities.ListEntitiesCapability;
import org.apache.commons.io.IOUtils;
import org.junit.Test;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.net.URLEncoder;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.*;

public class TestSalesforceClient {

    static class Contact {

        private String id;
        private String name;
        private String firstName;
        private String lastName;
        private String phone;

        public String getId() {
            return id;
        }

        public void setId(String id) {
            this.id = id;
        }

        public String getName() {
            return name;
        }

        public void setName(String name) {
            this.name = name;
        }

        public String getFirstName() {
            return firstName;
        }

        public void setFirstName(String firstName) {
            this.firstName = firstName;
        }

        public String getLastName() {
            return lastName;
        }

        public void setLastName(String lastName) {
            this.lastName = lastName;
        }

        public String getPhone() {
            return phone;
        }

        public void setPhone(String phone) {
            this.phone = phone;
        }
    }

    //@Test
    public void testGetObjectSpecification() throws Exception {

        ForceApi api = new ForceApi(new ApiConfig();
        DescribeSObject objectDescr = api.describeSObject("ContentVersion"); //"Contact");
        for (DescribeSObject.Field field : objectDescr.getAllFields()) {
            System.out.println("Field: " + field.getName());
            System.out.println("Type: " + field.getType() + ", SOAP type: " + field.getSoapType());
            System.out.println("Updateable: " + field.isUpdateable());
        }
    }

    //@Test
    public void testGetObjectSpecificationForTrackingData() throws Exception {

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                //.setForceURL("")
        //.setUsername("")
        //.setPassword(""));
        DescribeSObject objectDescr = api.describeSObject("VisitorTracking__c"); //"Contact");
        for (DescribeSObject.Field field : objectDescr.getAllFields()) {
            System.out.println("Field: " + field.getName());
            System.out.println("Type: " + field.getType() + ", SOAP type: " + field.getSoapType());
            System.out.println("Updateable: " + field.isUpdateable());
        }
    }

    //@Test
    public void testGetObject() throws Exception {

        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        ResourceRepresentation result = api.getSObject("contact", "");

        //Contact contact = result.as(Contact.class);
        //System.out.println("Contact: " + contact.getFirstName() + " " + contact.getLastName() + ", Phone: " + contact.getPhone());
        Map contact = result.asMap();
        for (Object key : contact.keySet()) {
            System.out.println(key + " = " + contact.get(key));
            if (contact.get(key) != null) {
                System.out.println("Type: " + contact.get(key).getClass());
                if (contact.get(key).getClass() == String.class) {
                    String value = (String) contact.get(key);
                    if (value.contains("+0000")) {
                        System.out.println("Date: " + ZonedDateTime.parse(value, DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSZ")));
                    }
                }

            }
        }
        System.out.println("Contact: " + contact);
    }

    //@Test
    public void testGetFile() throws Exception {
        ExtendedForceApi api = new ExtendedForceApi(new ApiConfig()
                .setUsername("")
                .setPassword(""));

        /*

        System.out.println("File metadata fields:");
        DescribeSObject objectDescr = api.describeSObject("ContentVersion");
        for (DescribeSObject.Field field : objectDescr.getAllFields()) {
            System.out.println("Field: " + field.getName());
            System.out.println("Type: " + field.getType() + ", SOAP type: " + field.getSoapType());
            System.out.println("Updateable: " + field.isUpdateable());
        }
         */

        QueryResult<Map> result = api.query("SELECT Id, Title, Description, VersionData, FileType, FileExtension, ContentSize FROM ContentVersion WHERE IsLatest=true LIMIT 100");
        System.out.println("Total size: " + result.getTotalSize());
        for (Map file : result.getRecords()) {
            System.out.println("File: " + file);
            InputStream stream = api.getStream("/sobjects/ContentVersion/" + file.get("Id").toString() + "/VersionData");
            ByteArrayOutputStream outStream = new ByteArrayOutputStream();
            IOUtils.copy(stream, outStream);
            System.out.println("Read " + outStream.size() + " bytes");
        }
    }

    //@Test
    public void testSearch() throws Exception {
        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

        ExtendedForceApi api = new ExtendedForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        String SEARCH_STRING_RESERVED_CHARS = "([?&|!{}\\[\\]()^~*:\"'+\\-])";
        String searchString = "tridion_c4aa74c0-64cf-49cc-868d-04fbba436987";
        String escapedSearchString = searchString.replaceAll(SEARCH_STRING_RESERVED_CHARS, "\\\\$1");

        QueryResult<Contact> result = api.search("FIND {" + escapedSearchString + "} IN ALL FIELDS RETURNING Contact(Id, Name  OFFSET 0) LIMIT 20", Contact.class);
        System.out.println("Total size: " + result.getTotalSize());
        for (Contact contact : result.getRecords()) {
            System.out.println("Contact: " + contact.getName());
        }

    }

    //@Test
    public void testListObjects() throws Exception {
        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        QueryResult<Contact> result = api.query("SELECT Id, FirstName, LastName FROM Contact LIMIT 10 OFFSET 10", Contact.class);
        System.out.println("Total size: " + result.getTotalSize());
        for (Contact contact : result.getRecords()) {
            System.out.println("Contact: " + contact.getFirstName() + " " + contact.getLastName());
        }
    }

    //@Test
    public void testListFiles() throws Exception {
        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        QueryResult<Map> result = api.query("SELECT Id, Title, Description, VersionNumber FROM ContentVersion WHERE IsLatest = true");
        System.out.println("Total size: " + result.getTotalSize());
        for (Map file : result.getRecords()) {
            System.out.println("File: " + file.get("Id") + " " + file.get("Title") + " v" + file.get("VersionNumber"));
        }
    }

    //@Test
    public void testListObjectsUnderAccount() throws Exception {
        SalesforceConfiguration config = new SalesforceConfiguration();
        List<String> entityTypes = new ArrayList<>();
        entityTypes.add("Account");
        entityTypes.add("Contact");
        config.setEntityTypes(entityTypes);
        config.setUsername("");
        config.setPassword("");
        SalesforceConnector connector = new SalesforceConnector();
        connector.initialize("salesforce", config, null, Collections.emptyList());
        connector.start();

        ListEntitiesCapability listEntitiesCapability = (ListEntitiesCapability) connector.getCapability("ListEntities", "Account", null);
        EntityIdentity context = Builder.newEntityIdentity()
                .type("")
                .id("")
                .namespaceId("")
                .build();

        PaginationData paginationData = Builder.newPaginatedData()
                .startIndex(0)
                .pageSize(10)
                .build();
        EntityPaginatedList result = listEntitiesCapability.listEntities(context, paginationData, StructureTypeSelector.All, null);
        for (Entity entity : result.getEntities()) {
            System.out.println("Entity: " + entity.getIdentity().getId() + " (" + entity.getIdentity().getType() + ")");
        }

    }

    //@Test
    public void testUpdateObject() throws Exception {
        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        objectMapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        Contact contact = new Contact();
        contact.setPhone("");
        api.updateSObject("contact", "", contact);
    }

    //@Test
    public void testCreateObject() throws Exception {

        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                WRITE_DATES_AS_TIMESTAMPS, false);
        objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        objectMapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);

        ForceApi api = new ForceApi(new ApiConfig()
                .setUsername("")
                .setPassword("")
                .setObjectMapper(objectMapper));

        Map<String,Object> obj =  new HashMap<>();
        obj.put("ContactId__c", "");
        obj.put("SessionId__c", "");
        obj.put("SiteUrl__c", "");
        obj.put("Categories__c", "");

        String id = api.createSObject("VisitorTracking__c", obj);
        System.out.println("Created object with ID: " + id);
    }

    //@Test
    public void testGetSchema() throws Exception {
        SalesforceConfiguration config = new SalesforceConfiguration();
        config.setEntityTypes(Collections.singletonList("Contact"));
        config.setUsername("");
        config.setPassword("");
        SalesforceConnector connector = new SalesforceConnector();
        connector.initialize("salesforce", config, null, Collections.emptyList());
        connector.start();
    }


}
