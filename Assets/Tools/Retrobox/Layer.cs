using System;
using System.Collections.Generic;
namespace Retro {
    [System.Serializable]
    public class Layer {
        public Shape kind;
        public List<Frame> frames;
        public FrameDataById frameDataById;
        public Layer(Shape kind_ = Shape.Box) {
            kind = kind_;
            frames = new List<Frame>();
            frameDataById = new FrameDataById();
        }

        public Curve CurveFromKeyFrames(int startFrame) {
            Curve c = null;
            FrameData a = null;
            FrameData b = null;

            if (frames[startFrame].IsKeyFrame()) {
                a = GetFrameData(startFrame);
                Frame n = GetNextKeyFrame(startFrame);
                if (n != null) {
                    b = GetFrameData(frames.IndexOf(n));
                    return c = new Curve(a.position, a.forwardHandle, b.backHandle, b.position);
                }
            }

            return c;
        }

        public FrameData GetFrameData(int i) {
            if (!String.IsNullOrEmpty(frames[i].dataId) &&
                frameDataById.ContainsKey(frames[i].dataId)) {

                return frameDataById[frames[i].dataId];
            } else {
                return FrameData.emptyData;
            }
        }

        public void Add(Frame f, int i = -1) {
            if (i == -1) {
                if (frames.Count > 0) {
                    i = frames.Count;
                } else {
                    i = 0;
                }
            }

            //TODO: Consider the implications of removing this
            AddKeyFrameData(f);
            frames.Insert(i, f);
            ResyncFrames(i);
        }

        public void Remove(int i) {
            Frame f = frames[i];
            RemoveKeyFrameData(f);
            frames.Remove(f);
            ResyncFrames(i);
        }

        public void ResyncFrames(int index) {

            //do we actually have a frame from which to copy?
            if (index < frames.Count - 1) {
                string dataId = index >= 0 ? frames[index].dataId : System.Guid.Empty.ToString();

                for (int i = index + 1; i < frames.Count; i++) {
                    //as long as the next frame is not a key or empty frame, 

                    if (frames[i].kind == Frame.Kind.CopyFrame) {
                        //update the frame references of all subsequence frames to the new duplicate
                        frames[i].dataId = dataId;
                    } else {
                        return;
                    }
                }
            }
            RemoveEmptyFrameData();
        }
        public void RemoveEmptyFrameData() {
            List<string> frameDataGUIDs = new List<string>();
            List<string> dataGUIDsToRemove = new List<string>();
            foreach (Frame f in frames) {
                if (!frameDataGUIDs.Contains(f.dataId)) {
                    frameDataGUIDs.Add(f.dataId);
                }
            }
            foreach (string s in frameDataById.Keys) {
                if (!frameDataGUIDs.Contains(s)) {
                    dataGUIDsToRemove.Add(s);
                }
            }
            foreach (string s in dataGUIDsToRemove) {
                frameDataById.Remove(s);
            }
        }

        public void AddKeyFrameData(Frame f) {
            //  if it's a keyframe, then create a new FrameData object, duplicating the previous frame, and tie it to the new Keyframe
            /*
              if (f.kind == Frame.Kind.KeyFrame) {
                  FrameData d;

                  if (f.data == null) {
                      d = new FrameData(new Vector2(16, 16), new Vector2(16, 16));
                  } else {
                      d = FrameData.Clone(f.data);
                  }
                  d.keyFrameId = f.guid;
                  f.dataId = d.guid;
                  f.parent.frameDataById.Add(d.guid, d);
              }
              */
        }

        public void RemoveKeyFrameData(Frame f) {
            if (f.kind == Frame.Kind.KeyFrame) {
                if (f.dataId != null && frameDataById.ContainsKey(f.dataId) && frameDataById[f.dataId].keyFrameId == f.guid) {
                    frameDataById.Remove(f.dataId);
                }
            }
        }


        public Frame GetCurrentKeyFrameOrPrevious(int startIndex) {
            if (frames[startIndex].IsKeyFrame()) {
                return frames[startIndex];
            } else return GetPreviousKeyFrame(startIndex);
        }

        public Frame GetPreviousKeyFrame(int startIndex) {
            for (int i = startIndex; i >= 0; i--) {
                if (frames[i].kind == Frame.Kind.KeyFrame) {
                    return frames[i];
                }
            }
            return null;
        }

        public Frame GetCurrentKeyFrameOrNext(int startIndex) {
            if (frames[startIndex].IsKeyFrame()) {
                return frames[startIndex];
            } else return GetNextKeyFrame(startIndex);
        }

        public Frame GetNextKeyFrame(int startIndex) {
            for (int i = startIndex + 1; i < frames.Count; i++) {
                if (frames[i].kind == Frame.Kind.KeyFrame) {
                    return frames[i];
                }
            }
            return null;
        }

    }


}