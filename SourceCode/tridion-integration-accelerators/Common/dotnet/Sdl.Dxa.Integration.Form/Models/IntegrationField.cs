using Sdl.Dxa.Modules.Crm;
using Sdl.Web.Common.Extensions;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    [SemanticEntity(EntityName = "IntegrationField", Vocab = CoreVocabulary, Prefix = "e")]
    public class IntegrationField : EntityModel
    {
        [SemanticProperty("e:fieldName")]
        public string FieldName { get; set; }

        [SemanticProperty("e:eclField")]
        public FormFieldEclItem EclField { get; set; }

        [SemanticProperty("e:fieldValue")]
        public string FieldValue { get; set; }
     
        [SemanticProperty("e:eclFieldValue")]
        public EclItemValue EclFieldValue { get; set; }
        
        [SemanticProperty("e:fieldType")]
        public string FieldType { get; set; }
        
        [SemanticProperty("e:schemaField")]
        public bool IsSchemaField { get; set; }
        
        public string ResolveFieldName()
        {
            string fieldName;
            if (FieldName != null)
            {
                fieldName = FieldName;
            }
            else if (EclField != null)
            {
                fieldName = EclField.FieldName;
            }
            else
            {
                throw new ResolveFieldException("Could not resolve field name.");
            }

            return fieldName.ToCamelCase();
        }
        
        public object ResolveFieldValue()
        {
            if (EclFieldValue != null)
            {
                return EclFieldValue.Value;
            }
            else
            {
                return VariableSubstituter.ResolveEntityValue(FieldValue);
            }
        }
        
    }
}