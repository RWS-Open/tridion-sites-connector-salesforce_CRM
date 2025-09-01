using System.Collections.Generic;
using Sdl.Dxa.Modules.Crm.Tracking;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Modules.Crm.Models
{
    [SemanticEntity(EntityName = "TrackingWidget", Vocab = CoreVocabulary, Prefix = "e")]
    public class TrackingWidget :  EntityModel, ITrackedEntity
    {
        [SemanticProperty("e:trackedCategories")]
        public List<string> TrackedCategories { get; set; }
    }
}