using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStreetWrapper.Models
{
    public class ProductImageModel
    {
        public ProductImageModel(string itemID, string tag, string colorNums, string altImages)
        {
            ItemID = itemID;
            Colors = string.IsNullOrEmpty(colorNums) ? new string[0] : colorNums.Split('|').Select(c => c.Trim()).ToArray();
            AltImages = string.IsNullOrEmpty(altImages) ? new string[0] : altImages.Split('|').Select(a => a.Trim().Replace(".jpg", string.Empty)).Distinct().ToArray();

            if (string.IsNullOrEmpty(tag) && Colors.Length > 0)
                DefaultColor = Colors[0];
            else if (tag.Contains("|"))
                DefaultColor = tag.Split('|')[0];
            else
                DefaultColor = tag;

            AnalyzeVariations();
        }

        public string ItemID { get; private set; }
        public string DefaultColor { get; private set; }
        public string[] Colors { get; private set; }
        public string[] AltImages { get; private set; }

        public List<VariationImageModel> Variations { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            Variations.ForEach(v => builder.AppendLine(v.ToString()));

            return builder.ToString();
        }

        private void AnalyzeVariations()
        {
            var variations = new List<VariationImageModel>();
            foreach(var image in AltImages)
            {
                var imageParts = image.Split('_');
                if (imageParts.Length < 2)
                    continue;

                if(image.EndsWith("_f") || image.EndsWith("_b"))
                {
                    variations.Add(new VariationImageModel() { Color = imageParts[1], Style = ItemID, Type = "variation", Name = image });
                }
                else if(imageParts[1] == DefaultColor)
                {
                    variations.Add(new VariationImageModel() { Color = imageParts[1], Style = ItemID, Type = "master", Name = image });
                }
            }

            foreach(var color in Colors)
            {
                if(!variations.Any(v => v.Color == color))
                {
                    variations.Add(new VariationImageModel() { Color = color, Style = ItemID, Type = "variation", Name = $"{ItemID}_{color}_f" });
                }
            }

            Variations = variations.OrderByDescending(v => v.Type).ThenBy(v => v.Color).ThenBy(v => v.Name).ToList();
        }
    }

    public class VariationImageModel
    {
        public string Style { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"'{Style}','{Color}',{Type},{Name}";
        }
    }
}
