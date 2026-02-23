package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.connector.salesforce.model.ContentVersion;
import com.sdl.tridion.connectorframework.connector.sdk.DownloadBinaryCapabilityBase;
import com.sdl.tridion.connectorframework.connector.sdk.LoggerFactory;
import com.sdl.tridion.connectorframework.connector.sdk.builder.Builder;
import com.sdl.tridion.connectorframework.contracts.Binary;
import com.sdl.tridion.connectorframework.contracts.BinaryDownloadOptions;
import com.sdl.tridion.connectorframework.contracts.BinaryReference;
import com.sdl.tridion.connectorframework.contracts.ConnectorContext;
import com.sdl.tridion.connectorframework.contracts.content.capabilities.CreateThumbnailCapability;
import org.slf4j.Logger;

import java.io.InputStream;
import java.util.StringTokenizer;

/**
 * Salesforce Download File Service.
 */
public class SalesforceDownloadFileService extends DownloadBinaryCapabilityBase {

    private static final Logger LOG = LoggerFactory.getLogger(SalesforceDownloadFileService.class);

    private SalesforceClient client;
    private CreateThumbnailCapability createThumbnailCapability;

    SalesforceDownloadFileService(SalesforceClient client, CreateThumbnailCapability createThumbnailCapability) {
        this.client = client;
        this.createThumbnailCapability = createThumbnailCapability;
    }

    @Override
    protected Binary handleDownloadBinary(BinaryReference binaryReference, BinaryDownloadOptions options, ConnectorContext context, int maxWidth, int maxHeight) {


        if (binaryReference.getType().equalsIgnoreCase("file")) {
            StringTokenizer tokenizer = new StringTokenizer(binaryReference.getId(), "-.");
            String contentVersionId = tokenizer.nextToken();
            LOG.debug("Downloading binary reference with content version ID: " + contentVersionId);
            long size = Long.parseLong(tokenizer.nextToken());
            String contentType = ContentVersion.getContentType(tokenizer.nextToken());
            InputStream stream = this.client.getFile(contentVersionId);

            if (createThumbnailCapability != null && maxWidth != -1 && maxHeight != -1) {
                LOG.debug("Resizing image to :" + maxWidth + "," + maxHeight);
                // Resize image
                //
                stream = createThumbnailCapability.createThumbnailImage(maxWidth, maxHeight, stream);
            }

            return Builder.newBinary()
                    .stream(stream)
                    .contentType(contentType)
                    //.contentLength(size)
                    .build();
        }

        return null;
    }
}
