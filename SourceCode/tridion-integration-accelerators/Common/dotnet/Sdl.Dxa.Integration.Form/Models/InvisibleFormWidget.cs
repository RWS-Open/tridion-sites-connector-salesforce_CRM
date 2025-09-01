using System.Collections.Generic;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    [SemanticEntity(EntityName = "InvisibleFormWidget", Vocab = CoreVocabulary, Prefix = "e")]
    public class InvisibleFormWidget : EntityModel
    {
        [SemanticProperty("e:title")]
        public string Title { get; set; }
        
        [SemanticProperty("e:namespaceId")]
        public string NamespaceId { get; set; }
        
        [SemanticProperty("e:objectType")]
        public string ObjectType { get; set; }
        
        [SemanticProperty("e:objectType")]
        public string OperationType { get; set; }
        
        [SemanticProperty("e:fields")]
        public List<IntegrationField> Fields { get; set; }
        
    }
}