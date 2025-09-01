using System;
using System.Web.Configuration;
using Sdl.Tridion.Api.GraphQL.Client;
using Sdl.Tridion.Api.Http.Client.Auth;
using Sdl.Web.Common.Logging;
using Sdl.Web.Delivery.DiscoveryService;
using Sdl.Web.Tridion.ApiClient;

namespace Sdl.Dxa.Integration.Client
{
    public sealed class IntegrationApiClientProvider
    {
        
        private static readonly Lazy<IntegrationApiClientProvider> lazy =
            new Lazy<IntegrationApiClientProvider>(() => new IntegrationApiClientProvider());

        public static IntegrationApiClientProvider Instance => lazy.Value;
        
        private readonly Uri _endpoint;
        private readonly IAuthentication _oauth;

        private IntegrationApiClientProvider()
        {
            try
            {
                string uri = WebConfigurationManager.AppSettings["pca-service-uri"];
                if (string.IsNullOrEmpty(uri))
                {
                    var discoveryService = DiscoveryServiceProvider.Instance.ServiceClient;
                    Uri contentServiceUri = discoveryService.ContentServiceUri;
                    if (contentServiceUri == null)
                    {
                        Log.Error("Unable to retrieve content-service endpoint from discovery-service.");
                    }
                    else
                    {
                        Log.Info($"Content-service endpoint located at {contentServiceUri}");
                        _endpoint = new Uri(contentServiceUri.AbsoluteUri.Replace("content.svc",
                            "cd/api"));
                    }
                }
                else
                {
                    _endpoint = new Uri(uri);
                }
                if (_endpoint == null)
                {
                    throw new ApiClientException("Unable to retrieve endpoint for Public Content Api");
                }
                

                _oauth = new OAuth(DiscoveryServiceProvider.DefaultTokenProvider);
               
            }
            catch (Exception ex)
            {
                const string error = "Failed to initialize PCA client. Check the UDP services are running.";
                Log.Error(ex, error);
                throw;
            }
        }

        public IIntegrationApi Client
        {
            get
            {
                var graphQl = new GraphQLClient(_endpoint, new Logger(), _oauth);
                return new IntegrationApi(graphQl);
            }
        }
    }
}