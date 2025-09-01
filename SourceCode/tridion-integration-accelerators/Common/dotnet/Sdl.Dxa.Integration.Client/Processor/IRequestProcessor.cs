using System.Web;

namespace Sdl.Dxa.Integration.Client.Processor
{
    // TODO: Do we need this ??? For more advanced personalization scenarios ??
    public interface IRequestProcessor
    {
        void PreProcess(HttpRequest request); // TODO: Do we need to have the page model here as well???
        void PostProcess(HttpRequest request, HttpResponse response);
    }
}