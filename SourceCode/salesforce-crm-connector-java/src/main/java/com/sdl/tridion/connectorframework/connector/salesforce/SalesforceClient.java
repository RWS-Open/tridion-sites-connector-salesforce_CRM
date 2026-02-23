package com.sdl.tridion.connectorframework.connector.salesforce;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.force.api.*;
import com.sdl.tridion.connectorframework.contracts.exception.SystemException;
import org.apache.commons.lang3.StringUtils;

import java.io.InputStream;
import java.util.List;
import java.util.Map;

/**
 * Salesforce Lightweight Client.
 */
public class SalesforceClient {

    private SalesforceConfiguration configuration;
    private ExtendedForceApi api;

    private static String SEARCH_STRING_RESERVED_CHARS = "([?&|!{}\\[\\]()^~*:\"'+\\-])";

    public SalesforceClient(SalesforceConfiguration configuration) {
        this.configuration = configuration;
        this.initialize();
    }

    private void initialize() {
        try {
            // TODO: We most likely do not need object mapper here
            ObjectMapper objectMapper = new ObjectMapper();
            objectMapper.configure(com.fasterxml.jackson.databind.SerializationFeature.
                    WRITE_DATES_AS_TIMESTAMPS, false);
            objectMapper.configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true);
            objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
            objectMapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);
            //objectMapper.setPropertyNamingStrategy(new PropertyNamingStrategy.UpperCamelCaseStrategy());
            ApiConfig apiConfig = new ApiConfig()
                    .setUsername(configuration.getUsername())
                    .setPassword(configuration.getPassword())
                    .setLoginEndpoint(configuration.getLoginEndpoint())
                    .setObjectMapper(objectMapper);
            if (configuration.getClientId() != null && configuration.getClientSecret() != null) {
                apiConfig.setClientId(configuration.getClientId());
                apiConfig.setClientSecret(configuration.getClientSecret());
            }
            if (configuration.getRequestTimeout() != null) {
                apiConfig.setRequestTimeout(configuration.getRequestTimeout());
            }
            api = new ExtendedForceApi(apiConfig);

        }
        catch (Exception e) {
            throw new SystemException("Could not connect to Salesforce.", e);
        }
    }

    public DescribeSObject getObjectDescription(String type) {
        return api.describeSObject(type);
    }

    public Map getObject(String type, String id) {
        ResourceRepresentation result = api.getSObject(type, id);
        return result.asMap();
    }

    public String createObject(String type, Map entity) {
        return api.createSObject(type, entity);
    }

    public void updateObject(String type, String id, Map entity) {
        api.updateSObject(type, id, entity);
    }

    public void deleteObject(String type, String id) {
        api.deleteSObject(type, id);
    }

    public QueryResult<Map> listObjects(String type, List<String> fieldNames, int maxItems, int startIndex) {
        return listObjects(type, fieldNames, null, maxItems, startIndex);
    }

    public QueryResult<Map> listObjects(String type, List<String> fieldNames, String condition, int maxItems, int startIndex) {
        String soql = "SELECT " + StringUtils.join(fieldNames, ",") + " FROM " + type;
        if (condition != null) {
            soql += " WHERE " + condition;
        }
        soql += " LIMIT " + maxItems + " OFFSET " + startIndex;
        return api.query(soql);
    }

    public QueryResult<Map> searchObjects(String type, List<String> fieldNames, String searchString, int maxItems, int startIndex) {
        String escapedSearchString = searchString.replaceAll(SEARCH_STRING_RESERVED_CHARS, "\\\\$1");
        String sosl = "FIND {" + escapedSearchString + "} RETURNING " + type +
                "(" + StringUtils.join(fieldNames, ",") + " OFFSET " + startIndex + ")" +
                " LIMIT " + maxItems;
        return api.search(sosl);
    }

    public InputStream getFile(String id) {
        return api.getStream("/sobjects/ContentVersion/" + id + "/VersionData");
    }
}
