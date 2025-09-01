using System.Collections.Generic;
using System.IO;
using HttpMultipartParser;
using Newtonsoft.Json;
using Sdl.Web.Common.Logging;

namespace Sdl.Dxa.Modules.ExternalContent.Models
{
    public class SerializedExternalContent
    {
        private MultipartFormDataParser _multipartParser;
        
        public SerializedExternalContent(Stream multipartStream)
        {
            _multipartParser = MultipartFormDataParser.Parse(multipartStream);
            Log.Info("Multimedia files: " +  _multipartParser.Files.Count);
        }

        public IList<ContentField> GetContentFields()
        {
            var contentFieldsJson = _multipartParser.GetParameterValue("ContentFields");
            if (contentFieldsJson != null)
            {
                return JsonConvert.DeserializeObject<List<ContentField>>(contentFieldsJson);
            }

            return null;
        }

        public string GetHtmlFragment()
        {
            var htmlFragment = _multipartParser.GetParameterValue("HtmlFragment");
            return htmlFragment;
        }
        
        public FilePart GetMultimedia(string path)
        {
            Log.Info("Getting multimedia: " + path);
            foreach (var file in _multipartParser.Files)
            {
                Log.Info("Filename: " + file.FileName);
                if (file.FileName.Equals(path))
                {
                    return file;
                }
            }

            return null;
        }
    }
}