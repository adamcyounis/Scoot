using System.Collections.Generic;
namespace Retro {
    [System.Serializable]
    public class Group {
        //group variables
        public string myBoxType;
        public Shape layerKind;
        public List<Layer> layers;

        //POINT VARIABLES

        //HITBOX VARIABLES
        public enum CollisionType { Collider, Trigger, NoCollide }
        public CollisionType collisionType;

        //editor variables
        public bool expanded;
        public bool visible;

        public Group(string s, Shape layerKind_) {
            layerKind = layerKind_;
            myBoxType = s;
            expanded = true;
            visible = true;
            collisionType = CollisionType.Trigger;
            layers = new List<Layer>();
        }
    }


}