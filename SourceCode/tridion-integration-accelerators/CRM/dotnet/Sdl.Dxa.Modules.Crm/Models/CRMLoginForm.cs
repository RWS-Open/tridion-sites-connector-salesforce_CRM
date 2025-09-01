using Sdl.Web.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sdl.Web.Modules.CRM.Models
{
    [SemanticEntity(EntityName = "CRMLoginForm", Vocab = CoreVocabulary, Prefix = "e")]
    public class CRMLoginForm : EntityModel
    {
        const string LOGIN_URL = "/authentication/login";

        [SemanticProperty("e:title")]
        public string Title { get; set; }

        [SemanticProperty("e:content")]
        public RichText Content { get; set; }

        [SemanticProperty("e:usernameLabel")]
        public string UsernameLabel { get; set; }

        [SemanticProperty("e:passwordLabel")]
        public string PasswordLabel { get; set; }

        [SemanticProperty("e:submitLabel")]
        public string SubmitLabel { get; set; }

        public string LoginUrl
        {
            get
            {
                var returnUrl = HttpContext.Current.Request.Params.Get("ReturnUrl");
                if ( returnUrl != null )
                {
                    return LOGIN_URL + "?ReturnUrl=" + returnUrl; // TODO: URL encode here???
                }
                else
                {
                    return LOGIN_URL;
                }
            }
        }
    }
}