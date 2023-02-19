using System;
using UnityEngine;
namespace Retro {
    [Serializable]
    public class FrameData {
        public Rect rect {
            get { return new Rect(position, size); }
            set {
                position = value.position;
                size = value.size;
            }
        }

        public static FrameData emptyData;

        public readonly string guid;
        public string keyFrameId;
        public Vector2 position;
        public Vector2 size;

        public Vector2 forwardHandle;
        public Vector2 backHandle;

        public BoxProps props;

        static FrameData() {
            emptyData = new FrameData();
        }
        public FrameData() {
            guid = Guid.NewGuid().ToString();
            position = new Vector2(16, 16);
            forwardHandle = new Vector2(10, 10);
            backHandle = new Vector2(-10, -10);

            size = new Vector2(0, 0);
            props = new BoxProps();
        }

        public FrameData(Vector2 position_, Vector2 size_) {
            guid = Guid.NewGuid().ToString();
            position = position_;
            forwardHandle = new Vector2(10, 10);
            backHandle = new Vector2(-10, -10);
            size = size_;
            props = new BoxProps();
        }

        public static FrameData Clone(FrameData d) {
            FrameData f = new FrameData(d.position, d.size);
            f.forwardHandle = d.forwardHandle;
            f.backHandle = d.backHandle;
            f.props = new BoxProps();
            foreach (string k in d.props.Keys) {
                f.props.Add(k, BoxProperty.Clone(d.props[k]));
            }
            return f;
        }

    }


}