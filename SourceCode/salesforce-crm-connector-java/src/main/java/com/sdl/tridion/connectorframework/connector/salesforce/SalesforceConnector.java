package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.connector.salesforce.model.SalesforceRootFolder;
import com.sdl.tridion.connectorframework.connector.sdk.ConnectorBase;
import com.sdl.tridion.connectorframework.connector.sdk.LoggerFactory;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.contracts.ContainerEntity;
import com.sdl.tridion.connectorframework.contracts.connector.ConnectorConfiguration;
import com.sdl.tridion.connectorframework.contracts.content.capabilities.CreateThumbnailCapability;
import com.sdl.tridion.connectorframework.contracts.exception.ConfigurationException;
import org.slf4j.Logger;

import java.util.stream.Collectors;

/**
 * Salesforce Connector.
 */
public class SalesforceConnector extends ConnectorBase {

    private static final Logger LOG = LoggerFactory.getLogger(SalesforceConnector.class);

    private SalesforceClient client;
    private SalesforceCrudService crudService;
    private SalesforceDownloadFileService downloadFileService;
    private SalesforceRootListService rootListService;

    static final int DEFAULT_PAGE_SIZE = 100;

    public String getName() {
        return "Salesforce"; // What name to use here???
    }

    // TODO: List items based on a parent (account for example)
    // TODO: Fix sorting option

    public void start() {
        LOG.info("Starting the Salesforce connector...");
        SalesforceConfiguration configuration = this.getConfiguration();
        validateConfiguration(configuration);

        int pageSize = configuration.getPageSize() != null ? configuration.getPageSize() : DEFAULT_PAGE_SIZE;

        // Salesforce client
        //
        client = new SalesforceClient(configuration);

        // Connector services
        //
        crudService = new SalesforceCrudService(client, configuration, this.getNamespaceId(), pageSize);
        downloadFileService = new SalesforceDownloadFileService(client,  this.getConnectorManagerCapability(CreateThumbnailCapability.class));
        rootListService = new SalesforceRootListService(crudService, configuration, this.getNamespaceId(), pageSize);

        // TODO: Register the types in lower case

        // Entity capabilities
        //
        this.addCapabilityMetadata(crudService.getCapabilities().stream().collect(Collectors.toList()));
        this.addCapabilityMetadata(rootListService.getCapabilities().stream().collect(Collectors.toList()));

        // Connector-wide capabilities
        //
        this.addCapabilityMetadata(
            Builder.newConnectorCapabilityMetadata()
                .namespaceId(this.getNamespaceId())
                .capability(downloadFileService)
                .capability(rootListService)
                .namespaceId(this.getNamespaceId())
                .build());
    }

    public void stop() {
        this.clearCapabilityMetadata();
    }

    protected Class<? extends ConnectorConfiguration> getConfigurationEntityType() {
        return SalesforceConfiguration.class;
    }

    public ContainerEntity getRoot(String localeId) {
        return new SalesforceRootFolder(this.getNamespaceId(), localeId);
    }

    private void validateConfiguration(SalesforceConfiguration configuration) throws ConfigurationException {
        if (configuration.getUsername() == null) {
            throw new ConfigurationException("'Username' is missing in the configuration");
        }
        if (configuration.getPassword() == null) {
            throw new ConfigurationException("'Password' is missing in the configuration");
        }
        if (configuration.getClientSecret() != null && configuration.getClientId() == null) {
            throw new ConfigurationException("'ClientId' is missing in the configuration. Is required when using OAuth login flow.");
        }
        if (configuration.getClientId() != null && configuration.getClientSecret() == null) {
            throw new ConfigurationException("'ClientSecret' is missing in the configuration. Is required when using OAuth login flow.");
        }
    }
}
