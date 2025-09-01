using Sdl.Web.Common;

namespace Sdl.Dxa.Modules.Crm
{
    public class BinaryReferenceHelper
    {
        const string ExternalItemDownloadPath = "/external/binary/";

        public const string DXABinaryReferencePrefix = "/externalitem/binary/"; // TODO: Have this configurable
        public const string CDBinaryReferencePrefix = "/cd/api/external/binary";

        private const string SLASH_REPLACEMENT = "!s!";
        private const string COLON_REPLACEMENT = "!c!";
        private const string PERCENTAGE_REPLACEMENT = "!p!";
        
        public static string EncodeBinaryUrl(string url)
        {
            var index = url.IndexOf(ExternalItemDownloadPath);
            if (index == -1)
            {
                throw new DxaException($"Could not encode binary url '{url}'");
            }

            var processedUrl = url.Substring(index + ExternalItemDownloadPath.Length);
            processedUrl = processedUrl
                .Replace(":", COLON_REPLACEMENT)
                .Replace("/", SLASH_REPLACEMENT)
                .Replace("%", PERCENTAGE_REPLACEMENT);
            return processedUrl;
        }
        
        public static string DecodeBinaryUrl(string url)
        {
            return ExternalItemDownloadPath + 
                   url
                    .Replace(COLON_REPLACEMENT, ":")
                    .Replace(SLASH_REPLACEMENT, "/")
                    .Replace(PERCENTAGE_REPLACEMENT, "%");
        }
    }
}