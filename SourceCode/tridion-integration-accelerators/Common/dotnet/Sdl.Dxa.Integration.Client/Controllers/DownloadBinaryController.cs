using System;
using System.IO;
using System.Web.Mvc;
using Sdl.Dxa.Modules.Crm;
using Sdl.Tridion.Api.Http.Client.Request;
using Sdl.Web.Common.Logging;
using Sdl.Web.Mvc.Formats;
using Sdl.Web.Tridion.ApiClient;

namespace Sdl.Dxa.Integration.Client.Controllers
{
    public class DownloadBinaryController : Controller 
    {
        // TODO: Rename to /api/integration/binary
        [Route("~/externalitem/binary/{encodedBinaryUrl}")]
        [FormatData]
        public virtual ActionResult Binary(string encodedBinaryUrl)
        {
            try
            {
                var publicContentApi = ApiClientFactory.Instance.CreateClient();
                var binaryDownloadUrl = BinaryReferenceHelper.DecodeBinaryUrl(encodedBinaryUrl);
                var response = publicContentApi.HttpClient.Execute<byte[]>(
                    new HttpClientRequest
                    {
                        Path = binaryDownloadUrl,
                        ContentType = null
                    }
                );
                
                // TODO: Solve so a stream can be returned here directly and also be cached in the similar as all other DXA images
                return new FileStreamResult(new MemoryStream(response.ResponseData), response.ContentType);

            }
            catch (Exception ex)
            {
                // TODO: Handle the exception differently here?
                Log.Error(ex);
                throw ex;
            }
        }
        
    }
}