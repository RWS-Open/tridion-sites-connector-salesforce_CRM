using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    [SemanticEntity(EntityName = "StaticFormField", Vocab = CoreVocabulary, Prefix = "e")]
    public class StaticFormField : EntityModel
    {
        [SemanticProperty("e:externalField")]
        public FormFieldEclItem ExternalField { get; set; }

        [SemanticProperty("e:fieldValue")]
        public string FieldValue { get; set; }
    }
}