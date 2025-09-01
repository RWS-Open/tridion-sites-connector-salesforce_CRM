using System;
using System.Web.Configuration;
using Sdl.Dxa.Modules.Crm;
using Sdl.Dxa.Modules.ExternalContent.Models;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Modules.ExternalContent
{
    /// <summary>
    /// External Content Area Registration
    /// </summary>
    public class ExternalContentAreaRegistration : BaseAreaRegistration
    {

        public override string AreaName => "ExternalContent";

        protected override void RegisterAllViewModels()
        {
            // TODO: Will this work if a namespace provides both videos and images??
            
            TypeRegistrationHelper.BuildAndRegisterSubTypes("externalcontent-image-namespaces", typeof(ExternalImage));
            TypeRegistrationHelper.BuildAndRegisterSubTypes("externalcontent-video-namespaces", typeof(PlayableMedia));
            TypeRegistrationHelper.BuildAndRegisterSubTypes("externalcontent-normalizedcontent-namespaces", typeof(Models.ExternalContent));
            
            RegisterViewModel("ExternalImage", typeof(ExternalImage));
            RegisterViewModel("ExternalContent", typeof(Models.ExternalContent), "ExternalContent");
            
            // Default video rendering using default rendering with HTML fragment from the connector
            //
            RegisterViewModel("ExternalVideo", typeof(PlayableMedia));
            
            // Brightcove VideoCloud
            // 
            RegisterViewModel("BrightcoveJSPlayer", typeof(PlayableMedia));
            RegisterViewModel("BrightcoveIFramePlayer", typeof(PlayableMedia));

            ExternalImage.Configure(WebConfigurationManager.AppSettings);
        }

    }
}