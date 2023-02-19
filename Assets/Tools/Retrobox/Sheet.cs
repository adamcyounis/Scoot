using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace Retro {
    public enum Shape { Box, Point }

    [System.Serializable]
    public class Sheet : ScriptableObject {
        [SerializeField]
        private string version = "1.0";
        public int count => spriteList.Count;
        public List<Sprite> spriteList;
        public List<Properties> propertiesList;
        public List<Group> groups;
        public string GetVersion() {
            return version;
        }

        public void SetVersion(System.Object o, string v) {
            if (o.GetType().Equals("RetroboxEditor")) version = v;
        }
    }

    [System.Serializable]
    public class FrameDataById : SerializableDictionary<string, FrameData> { }

    [Serializable]
    public class BoxProps : SerializableDictionary<string, BoxProperty> { }

    public enum PDataType { Bool, String, Int, Float, Vector2, Curve }


}