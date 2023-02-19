using UnityEngine;
namespace Retro {
    [System.Serializable]
    public class BoxProperty {
        public string name;
        public PDataType dataType;
        public int intVal;
        public float floatVal;
        public bool boolVal;
        public string stringVal;
        public Vector2 vectorVal;
        public Curve curveVal;

        public BoxProperty(string name_, PDataType dataType_) {
            name = name_;
            dataType = dataType_;
            intVal = 0;
            floatVal = 0;
            boolVal = false;
            stringVal = "";
            vectorVal = new Vector2();
            curveVal = Curve.Linear;
        }

        public static BoxProperty Clone(BoxProperty b) {
            BoxProperty c = new BoxProperty(b.name, b.dataType);
            c.intVal = b.intVal;
            c.floatVal = b.floatVal;
            c.boolVal = b.boolVal;
            c.stringVal = b.stringVal;
            c.vectorVal = b.vectorVal;
            c.curveVal = Curve.Clone(b.curveVal);
            return c;
        }

        public void SetPropertyValues(BoxProperty p) {
            dataType = p.dataType;
            intVal = p.intVal;
            boolVal = p.boolVal;
            floatVal = p.floatVal;
            stringVal = p.stringVal;
            vectorVal = p.vectorVal;
            curveVal = Curve.Clone(p.curveVal);
        }
    }


}