using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    [SemanticEntity(EntityName = "FieldValidationMetadata", Vocab = CoreVocabulary, Prefix = "e")]
    public class FieldValidationMetadata : EntityModel
    {
        [SemanticProperty("e:name")]
        public string Name { get; set; }
        
        [SemanticProperty("e:value")]
        public string Value { get; set; }
    }
}