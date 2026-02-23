package com.sdl.tridion.connectorframework.connector.salesforce;

import com.sdl.tridion.connectorframework.contracts.logging.ConnectorLogLevel;
import com.sdl.tridion.connectorframework.contracts.logging.ConnectorLogger;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class TestConnectorLogger implements ConnectorLogger {

    private static final Logger LOG = LoggerFactory.getLogger(TestConnectorLogger.class);

    @Override
    public ConnectorLogLevel getLevel() {
        return ConnectorLogLevel.Debug;
    }

    @Override
    public void logTrace(String s) {
        LOG.trace(s);
    }

    @Override
    public void logDebug(String s) {
        LOG.debug(s);
    }

    @Override
    public void logInfo(String s) {
        LOG.info(s);
    }

    @Override
    public void logWarning(String s) {
        LOG.warn(s);
    }

    @Override
    public void logError(String s) {
        LOG.error(s);
    }

    @Override
    public void logCritical(String s) {
        LOG.error(s);
    }
}
