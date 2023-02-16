using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FramePropertyHandler {

    public static void ApplyCurveProperties(Retro.RetroAnimator animator) {
        bool foundTracker = false;
        Dictionary<Retro.Layer, Retro.CurveInstance> curvesByLayer = animator.boxManager.curveInstancesByLayer;
        foreach (Retro.Layer l in curvesByLayer.Keys) {
            foreach (string s in curvesByLayer[l].properties.Keys) {

                if (s.Equals("velocityCurve")) {//This hasn't really been properly implemented yet...


                    animator.rigidBody.velocity = curvesByLayer[l].properties[s];
                }

                bool trackingPivot = s.Equals("trackPivot");
                bool propertyTrue = curvesByLayer[l].startFrame.props[s].boolVal;
                bool hasVelocityCurve = curvesByLayer[l].properties.ContainsKey("velocityCurve");

                if (trackingPivot && propertyTrue && hasVelocityCurve) {

                    foundTracker = true;
                    //get the position of the current sample point for the velocity curve
                    Retro.CurveInstance c = curvesByLayer[l];
                    Retro.Curve sCurve = l.CurveFromKeyFrames(c.startIndex); //spatial curve
                    Retro.Curve tCurve = c.startFrame.props["velocityCurve"].curveVal; //temporal curve

                    int index = Mathf.Clamp((animator.frame + 1), c.startIndex, c.endIndex);
                    float t = (index - c.startIndex) / (float)c.durationInFrames;

                    Vector2 tVal = tCurve.EvaluateForX(t);
                    Vector2 startPoint = sCurve.Evaluate(0);
                    Vector2 samplePoint = sCurve.Evaluate(tVal.y);
                    Vector2 p = animator.spriteRenderer.sprite.pivot;
                    Vector2 o = new Vector2(samplePoint.x, samplePoint.y);
                    Vector2 offset = new Vector2(p.x - o.x, -p.y + o.y) / animator.spriteRenderer.sprite.pixelsPerUnit;
                    //Debug.Log("index is " + c.startIndex + ". Setting spatial offset " + offset + " and samplePoint is " + o);
                    animator.SetSpatialOffset(offset);
                }

            }
        }
        if (!foundTracker) {
            animator.spriteRenderer.transform.localPosition = Vector2.zero;
        }
    }

    public static void ApplyFrameProperties(MonoBehaviour behaviour, Retro.RetroAnimator animator, Rigidbody2D body, bool facingRight) {

        Retro.Properties props = animator.mySheet.propertiesList[animator.GetCurrentFrame()];
        //AnimationEffectPool.current.ApplyEffects(anim);

        bool x = false;
        bool y = false;
        Vector2 force = new Vector2();

        foreach (Retro.BoxProperty b in props.frameProperties) {

            switch (b.name) {
                case "Event":
                    if (animator.boxManager.frameEvents.ContainsEvent(b.stringVal)) {
                        animator.boxManager.frameEvents.Invoke(b.stringVal);
                    } else {
                        behaviour.SendMessage(b.stringVal, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case "Velocity":
                    force = new Vector3(b.vectorVal.x, b.vectorVal.y);
                    break;
                case "VelX":
                    x = true;
                    break;
                case "VelY":
                    y = true;
                    break;
            }
        }

        if ((x || y) /*&& force != Vector2.zero*/) {
            if (!facingRight) force.x = -force.x;
            if (!x) force.x = body.velocity.x;
            if (!y) force.y = body.velocity.y;
            body.velocity = force;
        }
    }

    public static void ApplyFrameProperties(MonoBehaviour behaviour, Retro.RetroAnimator anim) { //no physics stuff

        Retro.Properties props = anim.mySheet.propertiesList[anim.GetCurrentFrame()];

        foreach (Retro.BoxProperty b in props.frameProperties) {
            switch (b.name) {
                case "Event":
                    behaviour.SendMessage(b.stringVal, SendMessageOptions.RequireReceiver);
                    //behaviour.GetType().InvokeMember(b.stringVal, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, behaviour, null);
                    break;
            }
        }
    }
}
