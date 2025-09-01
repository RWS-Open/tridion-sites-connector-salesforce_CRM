using System.Collections.Generic;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.ECommerce.Models
{
    public class Product : EclItem
    {
        // TODO: This is a simple basic model. Needs to be redone for the proper implementation...
        
        public string Title {get;set;}
        public RichText Description { get; set; }
        public IList<string> Images { get; set; }
        public string Price { get; set; }

        public void enrichWithExternalData(DynamicEntity entity)
        {
            // TODO: Here it would be nice with some kind of semantic mapping solution...
            // This is something to add to the common module
            
            Title = (string) entity.GetField("title");
            Description = new RichText((string) entity.GetField("description"));
            var price = entity.GetField("price") as NestedObject;
            if (price != null)
            {
                Price = (string) price.GetField("formattedValue");
            }
            Images = new List<string>();
            var productImages = entity.GetField("images") as List<NestedObject>;
            foreach (var productImage in productImages)
            {
                Images.Add((string) productImage.GetField("imageReference"));
            }

        }
    }
}