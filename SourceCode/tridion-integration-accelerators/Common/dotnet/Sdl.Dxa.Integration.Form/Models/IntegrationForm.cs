using System.Collections.Generic;
using System.Linq;
using Sdl.Web.Common.Models;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Integration.Form.Models
{
    /// <summary>
    /// Integration Form
    /// </summary>
    [SemanticEntity(EntityName = "IntegrationForm", Vocab = CoreVocabulary, Prefix = "e")]
    public class IntegrationForm : EntityModel
    {
        const string POST_URL_PREFIX = "/api/integration/form/";

        public string FormId => WebRequestContext.PageModel?.Id + "_" + Id;

        [SemanticProperty("e:title")]
        public string Title { get; set; }

        [SemanticProperty("e:content")]
        public RichText Content { get; set; } 
        
        [SemanticProperty("e:image")]
        public MediaItem Image { get; set; }

        [SemanticProperty("e:formType")]
        public string FormType { get; set; }

        [SemanticProperty("e:objectType")]
        // TODO: Rename to EntityType???
        public string ObjectType { get; set; }

        [SemanticProperty("e:objectKey")]
        public EclItemValue ObjectKey { get; set; }
        
        [SemanticProperty("e:fields")]
        public List<FormField> Fields { get; set; }

        [SemanticProperty("e:staticFields")]
        public List<StaticFormField> StaticFields { get; set; }
        
        [SemanticProperty("e:additionalFields")]
        public List<IntegrationField> AdditionalFields { get; set; }
        
        [SemanticProperty("e:requiredTickboxLabel")]
        public RichText RequiredTickboxLabel { get; set; }

        // TODO: Below are extensions to the integration form -----
        [SemanticProperty("e:privacyStatement")]
        public RichText PrivacyStatement { get; set; }
        
        [SemanticProperty("e:requiredTickboxErrorMessage")]
        public string RequiredTickboxErrorMessage { get; set; }
        // ---------------------------------------------------------

        [SemanticProperty("e:submitLabel")]
        public string SubmitLabel { get; set; }

        [SemanticProperty("e:successUrl")]
        public Link SuccessUrl { get; set; }

        [SemanticProperty("e:errorUrl")]
        public Link ErrorUrl { get; set; }
        
        public string EntityId { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public string NamespaceId => Fields?.First().ExternalField.NamespaceId;
        public string PostUrl => POST_URL_PREFIX + FormType.ToLower() + "/" + NamespaceId +  "/" + ObjectType  + (EntityId != null ? "/" + EntityId : ObjectKey != null ? "/" + ObjectKey.Value : "");
        
    }
}