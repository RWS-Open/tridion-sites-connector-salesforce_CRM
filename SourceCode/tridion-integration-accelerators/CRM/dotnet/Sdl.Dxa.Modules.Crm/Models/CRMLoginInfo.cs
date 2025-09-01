using Sdl.Web.Common.Models;
using Sdl.Web.Modules.CRM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tridion.ContentDelivery.AmbientData;

namespace Sdl.Web.Modules.CRM.Models
{
    [SemanticEntity(EntityName = "CRMLoginInfo", Vocab = CoreVocabulary, Prefix = "e")]
    public class CRMLoginInfo : EntityModel
    {
        [SemanticProperty("e:welcomeLabel")]
        public string WelcomeLabel { get; set; }
        
        [SemanticProperty("e:usernameExpression")]
        public string UsernameExpression { get; set; }
        
        [SemanticProperty("e:loginLink")]
        public Link LoginLink { get; set; }

        [SemanticProperty("e:updateProfileLink")]
        public Link UpdateProfileLink { get; set; }

        public string UserWelcomeText
        {
            get
            {
                return ADFContentSubstituter.ProcessContent(UsernameExpression, AmbientDataContext.CurrentClaimStore.GetAll());
            }
        }
    }
}