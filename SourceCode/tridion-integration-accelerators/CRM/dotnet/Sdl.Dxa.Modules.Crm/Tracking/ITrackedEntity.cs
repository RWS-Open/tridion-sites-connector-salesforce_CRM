using System.Collections.Generic;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    public interface ITrackedEntity
    {
        List<string> TrackedCategories { get; }
    }
}