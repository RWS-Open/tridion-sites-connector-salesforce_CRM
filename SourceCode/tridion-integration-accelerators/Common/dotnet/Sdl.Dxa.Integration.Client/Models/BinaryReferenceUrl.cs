using System;
using System.Web;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Integration.Client.Models
{
    /// <summary>
    /// Binary Reference URL.
    /// </summary>
    public class BinaryReferenceUrl
    {
        public const string URL_PROTOCOL = "tif-binref://";
        
        public BinaryReferenceUrl(string url)
        {
            if (!url.StartsWith(URL_PROTOCOL))
            {
                throw new ArgumentException("Not a binary reference URL: " + url);
            }

            var tokens = url.Replace(URL_PROTOCOL, "").Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
            {
                throw new ArgumentException("Invalid binary reference URL: " + url);
            }

            BinaryReference = new BinaryReference
            {
                NamespaceId = tokens[0],
                Id = HttpUtility.UrlDecode(tokens[1]),
                Type = tokens[2]
            };
        }
        
        public IBinaryReference BinaryReference { get; }
    }
}