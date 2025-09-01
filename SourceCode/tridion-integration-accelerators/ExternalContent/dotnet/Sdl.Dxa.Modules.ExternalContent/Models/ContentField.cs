using System.Collections.Generic;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Modules.ExternalContent.Models
{
    
    public class ContentField 
    {
        public string Name { get; set; }
        public string Value { get; set; }
        
        public BinaryReference MultimediaReference { get; set; }
        
        // TODO: Have a MediaItem if it's a image binary reference
        
        public ContentFieldType Type { get; set; } 
        
        public IList<ContentField> ContentFields { get; set; }
    }
}