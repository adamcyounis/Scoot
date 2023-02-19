using System;
namespace Retro {
    [System.Serializable]
    public class Frame {

        public string guid;

        public enum Kind { KeyFrame, CopyFrame, Empty }
        public Kind kind;
        public string dataId;
        //public FrameData data => isValid ? parent.frameDataById[dataId] : FrameData.emptyData;

        public Frame(Kind kind_ = Kind.CopyFrame, string dataId_ = default) {
            guid = Guid.NewGuid().ToString();
            dataId = dataId_;
            kind = kind_;
        }

        public bool IsKeyFrame() {
            return kind == Kind.KeyFrame;
        }

        public bool IsCopyFrame() {
            return kind == Kind.CopyFrame;
        }

        public bool IsEmpty() {
            return dataId == null ||
            dataId.Equals(System.Guid.Empty.ToString());
        }

        public static Frame Clone(Frame f) {
            return new Frame(f.kind, f.dataId);
        }
    }


}