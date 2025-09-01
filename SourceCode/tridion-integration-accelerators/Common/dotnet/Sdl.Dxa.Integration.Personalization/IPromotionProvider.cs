using System.Collections.Generic;
using Sdl.Dxa.Integration.Personalization.Models;

namespace Sdl.Dxa.Integration.Personalization
{
    public interface IPromotionProvider
    {
        
        // Providers to XO, Early Birds etc
        // Could this also be an interesting scenario for TIE acting as a rule engine??
        
        

        IEnumerable<IPromotion> GetPromotions(
            /*
             * Context
             * - Localization
             * - Page
             * - Entity session
             */
            
            );

    }
}