using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Retro {
    public class CurveInstance {

        public RetroAnimator animator;
        public Sheet sheet;
        public FrameData startFrame;
        public int startIndex;
        public int endIndex;
        public int durationInFrames => endIndex - startIndex;
        public float duration => ((float)durationInFrames) / animator.frameRate;
        Vector3 startWorldPos;

        //dictionary of <string, vector2> for each of the curve Properties to be evaluated...?
        public Dictionary<string, Vector2> properties;

        public CurveInstance(RetroAnimator animator_, Sheet sheet_, Layer layer, int startIndex_, Vector3 startWorldPos_) {
            animator = animator_;
            sheet = sheet_;
            startIndex = startIndex_;
            startFrame = layer.GetFrameData(startIndex_);
            endIndex = layer.frames.IndexOf(layer.GetNextKeyFrame(startIndex));
            startWorldPos = startWorldPos_;
            properties = new Dictionary<string, Vector2>();

            foreach (string s in startFrame.props.Keys) {
                if (startFrame.props[s].curveVal != null) {
                    properties.Add(s, Vector2.zero);
                }
            }
        }

        public void ProcessByFrame(Layer l, int frameIndex) {
            float t1 = (float)(frameIndex + 1 - startIndex) / (float)durationInFrames;
            float t0 = (float)(frameIndex - startIndex) / (float)durationInFrames;

            Process(l, t1, t1 - t0);
        }

        public void Process(Layer l, float t, float tDelta) {
            Sprite sprite = sheet.spriteList[0];

            foreach (string s in startFrame.props.Keys) {

                Curve tCurve = startFrame.props[s].curveVal;
                float propTime1 = tCurve.EvaluateForX(t).y;
                float propTime0 = tCurve.EvaluateForX(t - tDelta / duration).y;

                Curve sCurve = l.CurveFromKeyFrames(startIndex);
                //this is in sprite pixel space, not centred or anything
                Vector2 spatialVal1 = sCurve.Evaluate(propTime1);
                Vector2 spatialVal0 = sCurve.Evaluate(propTime0);

                //pixel delta between samples
                Vector2 spaceDelta = spatialVal1 - spatialVal0;

                //make it face the way the animator is facing...
                float facingDirection = animator.transform.localScale.x > 0 ? 1 : -1;
                spaceDelta = new Vector2(spaceDelta.x * facingDirection, -spaceDelta.y);
                //divide by PPU to scale into world spa
                properties[s] = spaceDelta / sprite.pixelsPerUnit / tDelta;

            }
        }
    }
}