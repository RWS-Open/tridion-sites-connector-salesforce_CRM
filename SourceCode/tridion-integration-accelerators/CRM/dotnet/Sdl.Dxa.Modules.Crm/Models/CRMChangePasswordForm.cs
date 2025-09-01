using Sdl.Web.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sdl.Web.Modules.CRM.Models
{
    [SemanticEntity(EntityName = "CRMChangePasswordForm", Vocab = CoreVocabulary, Prefix = "e")]
    public class CRMChangePasswordForm : EntityModel
    {
        [SemanticProperty("e:title")]
        public string Title { get; set; }

        [SemanticProperty("e:content")]
        public RichText Content { get; set; }

        [SemanticProperty("e:passwordLabel")]
        public string PasswordLabel { get; set; }

        [SemanticProperty("e:submitLabel")]
        public string SubmitLabel { get; set; }

        [SemanticProperty("e:minLength")]
        public int MinLength { get; set; }
    }
}