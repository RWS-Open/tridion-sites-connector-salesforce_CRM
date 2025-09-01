using System.Collections.Generic;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Contracts.Schema;

namespace Sdl.Dxa.Integration.Form.Models
{
    [SemanticEntity(EntityName = "FormField", Vocab = CoreVocabulary, Prefix = "e")]
    public class FormField : EntityModel
    {
        private AggregatedFieldValidation _aggregatedFieldValidation = null;
        
        [SemanticProperty("e:label")]
        public string Label { get; set; }

        [SemanticProperty("e:externalField")]
        public FormFieldEclItem ExternalField { get; set; } 
        
        [SemanticProperty("e:fieldValidation")]
        public List<KeywordModel> FieldValidation { get; set; }
        
        [SemanticProperty("e:fieldValidationMetadata")]
        public List<FieldValidationMetadata> FieldValidationMetadata { get; set; }
        
        public AggregatedFieldValidation AggregatedFieldValidation
        {
            get
            {
                if (_aggregatedFieldValidation == null)
                {
                     _aggregatedFieldValidation = new AggregatedFieldValidation(SchemaFieldDefinition, FieldValidation, FieldValidationMetadata);
                }
                return _aggregatedFieldValidation;
            }
        }
        
        [SemanticProperty(IgnoreMapping = true)]
        public string Value { get; set; } // TODO: Is this possible to do if the entity models are cached????

        [SemanticProperty(IgnoreMapping = true)]
        public bool IsReadOnly => SchemaFieldDefinition != null && SchemaFieldDefinition.Readonly;

        public bool InvalidFieldDefinition { get; set; } = false;
        
        public ISchemaFieldDefinition SchemaFieldDefinition { get; set; }
    }
}