using Sdl.Dxa.Integration.Client.Models;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    public class EclItemValue : EclItem
    {
        public string Value => new EclUri(EclUri).Id;
    }
}