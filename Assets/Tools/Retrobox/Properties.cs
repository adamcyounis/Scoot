using System.Collections.Generic;
namespace Retro {
    [System.Serializable]
    public class Properties {

        public List<BoxProperty> frameProperties;

        public Properties(List<BoxProperty> properties) {
            frameProperties = new List<BoxProperty>();

            foreach (BoxProperty b in properties) {
                frameProperties.Add(BoxProperty.Clone(b));
            }
        }

        public Properties() {
            frameProperties = new List<BoxProperty>();
        }

    }


}