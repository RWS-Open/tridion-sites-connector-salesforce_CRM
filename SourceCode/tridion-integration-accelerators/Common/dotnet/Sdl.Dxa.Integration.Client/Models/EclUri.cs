using System;
using System.Linq;
using System.Text;
using Sdl.Web.Tridion.ApiClient;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Integration.Client.Models
{
    /// <summary>
    /// ECL URI
    /// </summary>
    public class EclUri : IEntityIdentity
    {
        private const int ECL_URI_NUMBER_OF_TOKENS = 5;
        
        public EclUri(string eclUriStr)  {
            
            if (eclUriStr.StartsWith("ecl:")) {
                var tokens = eclUriStr.Replace("ecl:", "").Split(new char[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == ECL_URI_NUMBER_OF_TOKENS)
                {
                    LocaleId = tokens[0];
                    if (!LocaleId.All(char.IsDigit)) {
                        throw new ApiClientException("Invalid publication ID in ECL-URI. Must be numeric.");
                    }
                    NamespaceId = tokens[1];
                    Id = DecodeString(tokens[2]);
                    Type = tokens[3];
                    var itemType = tokens[4];
                    // TODO: Add support for new item types introduced in Sites 9.6
                    
                    if (itemType.Equals("file", StringComparison.OrdinalIgnoreCase)) {
                        StructureType = StructureType.Leaf;
                    } else if (itemType.Equals("folder", StringComparison.OrdinalIgnoreCase)) {
                        StructureType = StructureType.Container;
                    } else if (itemType.Equals("mp", StringComparison.OrdinalIgnoreCase)) {
                        StructureType = StructureType.Root;
                    } else  {
                        throw new ApiClientException("Invalid item type in ECL-URI: " + itemType);
                    }
                }
            }
        }
        
        public string Id { get; }
        public string LocaleId { get; }
        public string NamespaceId { get; }
        public string Type { get; }
        public StructureType StructureType { get; }
        
        private string DecodeString(String str) {

            var result = new StringBuilder();
            var unicodeValue = new StringBuilder(8);

            bool inEscapedString = false;

            foreach (char c in str) {
                if (!inEscapedString) {
                    if (c == '!') {
                        inEscapedString = true;
                    } else {
                        result.Append(c);
                    }
                } else {
                    if (c == ';') {
                        inEscapedString = false;
                        try
                        {
                            int decoded = Convert.ToInt32(unicodeValue.ToString(), 16);
                            result.Append((char) decoded);
                            unicodeValue = new StringBuilder(8);
                        }
                        catch (ArgumentException)
                        {
                            throw new ApiClientException($"The ECL URI part '{str}' contains a invalid encoded character: {unicodeValue}");
                        } 

                    } else {
                        unicodeValue.Append(c);
                    }
                }
            }
            return result.ToString();
        }
    }
}