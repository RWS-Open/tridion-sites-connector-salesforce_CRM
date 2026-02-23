package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.contracts.connector.ConnectorConfiguration;

import java.util.List;

/**
 * Salesforce Configuration.
 */
public class SalesforceConfiguration implements ConnectorConfiguration {

    private String clientId;
    private String clientSecret;
    private String username;
    private String password;
    private Integer requestTimeout = 30000; // Default 30 sec request timeout
    private List<String> entityTypes;
    private Integer pageSize;
    private boolean exposeDataTypes = false; // TODO: Make sure the default value works. It seems there an issue with unboxing to a native boolean here
    private String namespacePrefix;
    private String loginEndpoint;

    public String getClientId() {
        return clientId;
    }

    public void setClientId(String clientId) {
        this.clientId = clientId;
    }

    public String getClientSecret() {
        return clientSecret;
    }

    public void setClientSecret(String clientSecret) {
        this.clientSecret = clientSecret;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public Integer getRequestTimeout() {
        return requestTimeout;
    }

    public void setRequestTimeout(Integer requestTimeout) {
        this.requestTimeout = requestTimeout;
    }

    public List<String> getEntityTypes() {
        return entityTypes;
    }

    public void setEntityTypes(List<String> entityTypes) {
        this.entityTypes = entityTypes;
    }

    public Integer getPageSize() {
        return pageSize;
    }

    public void setPageSize(Integer pageSize) {
        this.pageSize = pageSize;
    }

    public boolean isExposeDataTypes() {
        return exposeDataTypes;
    }

    public void setExposeDataTypes(boolean exposeDataTypes) {
        this.exposeDataTypes = exposeDataTypes;
    }

    public String getNamespacePrefix() {
        return namespacePrefix;
    }

    public void setNamespacePrefix(String namespacePrefix) {
        this.namespacePrefix = namespacePrefix;
    }

    public String getLoginEndpoint() {
        return loginEndpoint;
    }

    public void setLoginEndpoint(String loginEndpoint) {
        this.loginEndpoint = loginEndpoint;
    }

}
