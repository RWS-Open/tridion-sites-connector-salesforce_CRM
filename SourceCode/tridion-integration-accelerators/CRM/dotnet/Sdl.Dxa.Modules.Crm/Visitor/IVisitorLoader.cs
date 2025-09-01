using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.Crm.Visitor
{
    public interface IVisitorLoader
    {
        DynamicEntity LoadVisitor(string visitorId);
        DynamicEntity LoadVisitorByAlias(string aliasName, string aliasValue);

        DynamicEntity LoadVisitorFromSession();
        
        void ClearVisitor();
        
        // TODO: Have one implementation for CRM 
        // TODO: Here is a bit of overlap with the tracking handler

        /*
         * This could be used by an authentication controller to load visitor data
         * For example:
         
        
        [HttpPost]
        [Route("login")]
        //[RequireHttps]
        public ActionResult Login(FormCollection loginData) 
        {
            var username = loginData["username"];
            var password = loginData["password"];
            if (Membership.ValidateUser(username, password))
            {
                FormsAuthentication.SetAuthCookie(username, false); 

                // Load visitor and store into entity session
                DIRegistry.Get<IVisitorLoader>().LoadVisitorByAlias("email", username);

                var redirectUrl = Request.QueryString["ReturnUrl"];
                if ( redirectUrl == null )
                {
                    redirectUrl = WebRequestContext.Localization.GetBaseUrl();
                }               
                return Redirect(redirectUrl);
            }

            // Stay on the page & show an error
            //
            return Redirect(Request.UrlReferrer.ToString());
           
        }  

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {         
            FormsAuthentication.SignOut();
            WebSecurity.Logout();
            DIRegistry.Get<IVisitorLoader>().ClearVisitor();           
            return Redirect(WebRequestContext.Localization.GetBaseUrl());
        }
         */
    }
}