using System;
using System.Collections.Generic;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Contracts.Schema;

namespace Sdl.Dxa.Integration.Form.Models
{
    public class AggregatedFieldValidation : EntityModel
    {
        public AggregatedFieldValidation(ISchemaFieldDefinition schemaFieldDefinition, 
            List<KeywordModel> fieldValidation,
            List<FieldValidationMetadata> fieldValidationMetadata)
        {
            if (fieldValidation != null)
            {
                foreach (var validationKey in fieldValidation)
                {
                    if (validationKey.Key.Equals("IsRequired", StringComparison.OrdinalIgnoreCase))
                    {
                        IsRequired = true;
                    }
                    else
                    {
                        Type = validationKey.Key.ToLower();
                    }
                }
            }

            if (fieldValidationMetadata != null)
            {
                foreach (var validationMetadata in fieldValidationMetadata)
                {
                    if (validationMetadata.Name.Equals("ErrorMessage", StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorMessage = validationMetadata.Value;
                    }
                    else if (validationMetadata.Name.Equals("Pattern"))
                    {
                        Pattern = validationMetadata.Value;
                    }
                    else if (validationMetadata.Name.Equals("Format"))
                    {
                        Format = validationMetadata.Value;
                    }
                    else if (validationMetadata.Name.Equals("MinLength"))
                    {
                        MinLength = Int32.Parse(validationMetadata.Value);
                    }
                    else if (validationMetadata.Name.Equals("MaxLength"))
                    {
                        MaxLength = Int32.Parse(validationMetadata.Value);
                    }
                    else if (validationMetadata.Name.Equals("AjaxValidator"))
                    {
                        AjaxValidator = validationMetadata.Value;
                    }
                }
            }
            
            if (Type == null)
            {
                if ( Format != null && Format.Equals("date-time") )
                {
                    Type = "date";
                }
                else if (schemaFieldDefinition != null && (schemaFieldDefinition.Type == FieldType.Float || schemaFieldDefinition.Type == FieldType.Integer))
                {
                    Type = "number";
                }
                else
                {
                    Type = "text";
                }
            }
        }

        public string Type { get; set; }

        public bool IsRequired { get; set; } = false;
        
        public int? MinLength { get; set; } // TODO: Rename??
        
        public int? MaxLength { get; set; } // TODO: Rename
        
        public string Pattern { get; set; }
        
        public string Format { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string AjaxValidator { get; set; }

    }
}