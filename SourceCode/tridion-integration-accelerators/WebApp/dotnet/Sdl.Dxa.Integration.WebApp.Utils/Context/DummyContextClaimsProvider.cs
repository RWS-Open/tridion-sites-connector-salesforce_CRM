using System.Collections.Generic;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Interfaces;

namespace Sdl.Dxa.Integration.WebApp.Utils.Context
{
    /// <summary>
    /// Dummy Context Claims Provider
    /// Bypass the Context Service to simplify development setup.
    /// </summary>
    public class DummyContextClaimsProvider : IContextClaimsProvider
    {
        public IDictionary<string, object> GetContextClaims(string aspectName, Localization localization)
        {
            return new Dictionary<string, object>
            {
                {"device.mobile", false},
                {"device.tablet", false},
                {"browser.displayWidth", 1024}
            };
        }

        public string GetDeviceFamily()
        {
            return "desktop";
        }
    }
}