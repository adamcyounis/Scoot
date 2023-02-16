using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Retro {
    public class HitboxManager {
        Dictionary<BoxCollider2D, Frame> activeFrameObjects; //a list of hitboxes active this frame
        Dictionary<BoxCollider2D, Frame> oldFrames;//a list of hitboxes active last frame
        Dictionary<Layer, ColliderInfo> layerColInfo; //collider info for all active hitboxes
        Dictionary<Collider2D, List<RetroAnimator>> bannedColliders;
        public Dictionary<Layer, CurveInstance> curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();
        public FramePropertyEventCollection frameEvents = new FramePropertyEventCollection();

        public BoxCollisionEvent collisionEvent;
        public HitConfirmEvent hitConfirmEvent;
        public RetroAnimator animator;
        PropertyModifier modifier;

        Sheet mySheet => animator.mySheet;
        Transform transform => animator.transform;
        public HitboxManager(RetroAnimator animator_) {
            animator = animator_;
            activeFrameObjects = new Dictionary<BoxCollider2D, Frame>();
            layerColInfo = new Dictionary<Layer, ColliderInfo>();
            bannedColliders = new Dictionary<Collider2D, List<RetroAnimator>>();
            collisionEvent = new BoxCollisionEvent();
            hitConfirmEvent = new HitConfirmEvent();
            curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();
        }

        public void Initialise() { //Setup a new animation to be played.
            oldFrames = new Dictionary<BoxCollider2D, Frame>();
            if (activeFrameObjects != null) foreach (KeyValuePair<BoxCollider2D, Frame> b in activeFrameObjects) oldFrames.Add(b.Key, b.Value); //make a copy of the list of colliders..
            activeFrameObjects = new Dictionary<BoxCollider2D, Frame>();
            layerColInfo = new Dictionary<Layer, ColliderInfo>();
            curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();

            for (int i = 0; i < mySheet.groups.Count; i++) { //for every group in my sheet...

                Group group = mySheet.groups[i];
                if (group.layerKind == Shape.Box && group.collisionType != Group.CollisionType.NoCollide) {

                    GameObject existingGroup = FindSceneObjectsMatchingGroup(group);

                    if (existingGroup != null) {
                        int l = 0;

                        //Refresh every boxcollider in our existing gameobject with each of our new hitboxes.
                        for (int g = 0; g < existingGroup.GetComponents<BoxCollider2D>().Length; g++) {
                            if (l < group.layers.Count) {
                                ColliderInfo info = existingGroup.GetComponents<ColliderInfo>()[g];
                                InstantiateHitbox(info, group, group.layers[l]);
                                oldFrames.Remove(info.col);
                                l++;

                            } else { //otherwise, if we had some leftover, disable the remaining old ones.
                                existingGroup.GetComponents<BoxCollider2D>()[g].enabled = false;
                            }
                        }

                        //if we have any new hitboxes left...
                        if (l < group.layers.Count) {
                            for (int n = l; n < group.layers.Count; n++) { //add new colliders on the object.
                                AddNewHitbox(existingGroup, group, group.layers[n]);
                            }
                        }

                    } else { //Create new hitbox gameobjects from scratch
                        GameObject newObject = CreateHitboxGroupInScene(group);
                        for (int j = 0; j < group.layers.Count; j++) {
                            AddNewHitbox(newObject, group, group.layers[j]);
                        }
                    }
                }
            }

            //disable every collider from the previous frame that wasn't involved in anything we just did.
            foreach (KeyValuePair<BoxCollider2D, Frame> b in oldFrames) {
                b.Key.enabled = false;
                ClearBannedAnimators(b.Key);
            }
        }

        GameObject FindSceneObjectsMatchingGroup(Retro.Group group) {
            for (int k = 0; k < transform.childCount; k++) {
                if (transform.GetChild(k).name.Equals(group.myBoxType)) {//if i find one...
                    return transform.GetChild(k).gameObject;
                }
            }
            return null;
        }

        GameObject CreateHitboxGroupInScene(Group group) {
            //setup a new group object child under the gameobject
            GameObject newObject = new GameObject();
            newObject.name = group.myBoxType;
            newObject.transform.position = new Vector3(0, 0, 0);
            newObject.transform.SetParent(transform, false);
            newObject.layer = animator.preferences.boxDictionary[group.myBoxType].physicsLayer;
            return newObject;
        }

        void AddNewHitbox(GameObject host, Group group, Layer layer) {
            //declare new objects
            ColliderInfo info = host.AddComponent<ColliderInfo>();
            BoxCollider2D c = host.AddComponent<BoxCollider2D>();
            if (host.GetComponent<PlatformEffector2D>() != null) {
                c.usedByEffector = true;
            }
            Frame f = layer.frames[0];
            FrameData data = layer.GetFrameData(0);
            info.Setup(mySheet, mySheet.groups.IndexOf(group), group.layers.IndexOf(layer), GetModifiedProperties(data.props), c, animator); //add the new collider to our ColliderInfo component
            InstantiateHitbox(info, group, layer);
            c.enabled = f.IsKeyFrame();
        }

        void InstantiateHitbox(ColliderInfo info, Group group, Layer layer) {
            //declare new objects
            FrameData frameData = layer.GetFrameData(0);
            Frame frame = layer.frames[0];
            //don't need to setup a new BoxCollider2D because there's one in the existingCollider...

            bool rounded = animator.preferences.boxDictionary[group.myBoxType].isRounded;

            if (frameData.props.ContainsKey("forceRounded")) {
                rounded = frameData.props["forceRounded"].boolVal;
            }

            info.Setup(mySheet, mySheet.groups.IndexOf(group), group.layers.IndexOf(layer));
            //setup collider properties
            SetColliderProperties(info.col,
                frameData.rect,
                mySheet.spriteList[0],
                rounded,
                (group.collisionType == Group.CollisionType.Trigger),
                animator.preferences.boxDictionary[group.myBoxType].material,
                animator.spatialOffset
            );

            activeFrameObjects.Add(info.col, frame);
            layerColInfo.Add(layer, info);
        }


        public void SetFrame(int newFrame) {

            //we actually need to process point groups first if they're going to offset the box positions
            List<Group> pointGroups = mySheet.groups.Where(x => x.layerKind == Shape.Point).ToList();
            List<Group> boxGroups = mySheet.groups.Where(x => x.layerKind == Shape.Box).ToList();
            List<Group> sortedGroups = new List<Group>(pointGroups);
            sortedGroups.AddRange(boxGroups);

            for (int i = 0; i < sortedGroups.Count; i++) {
                Group group = sortedGroups[i];

                for (int j = 0; j < group.layers.Count; j++) { //for every layer in every group...

                    Layer layer = group.layers[j];
                    Frame frame = layer.frames[newFrame];

                    switch (layer.kind) {
                        case Shape.Box:
                            if (group.collisionType != Group.CollisionType.NoCollide) {
                                ProcessBoxFrame(group, layer, newFrame);
                            } else {
                                //do something with this, because clearly, we want to use it for something other than being a collider.
                            }
                            break;

                        case Shape.Point:
                            ProcessPointFrame(group, layer, newFrame);
                            break;
                    }
                }
            }
        }

        public void UpdateCurves() {
            foreach (Layer layer in curveInstancesByLayer.Keys) {
                CurveInstance c = curveInstancesByLayer[layer];

                //TODO: be careful here. on looping boxes you might still get some weirdness. with this solution on wraparound
                float modTime = animator.looping ? animator.time % animator.duration : animator.time;
                float startTime = (float)(c.startIndex) / animator.frameRate;
                //float endTime = (float)(c.endIndex + 1) / animator.frameRate;
                float t = (modTime - startTime) / c.duration;
                if (t >= 0 && t < 1) {
                    //get the time of the first frame, and the time of the last frame
                    curveInstancesByLayer[layer].Process(layer, t, Time.fixedDeltaTime);
                }
            }
            FramePropertyHandler.ApplyCurveProperties(animator);
        }

        void ProcessPointFrame(Group group, Layer layer, int index) {
            Frame startFrame = layer.GetCurrentKeyFrameOrPrevious(index);
            Frame nextKeyFrame = layer.GetNextKeyFrame(index);

            //TODO:Redefine a curve in Retrobox, these tests are weird

            bool hasTwoKeyframes = (startFrame != null && nextKeyFrame != null);
            bool isLastFrame = startFrame == layer.frames[layer.frames.Count - 1];
            bool isCurve = hasTwoKeyframes || isLastFrame;

            if (layer.frames[index].IsEmpty() || !isCurve) {
                Frame f = layer.frames[index];
                if (curveInstancesByLayer.ContainsKey(layer)) {
                    curveInstancesByLayer.Remove(layer);
                    return;
                }

            } else {
                if (isCurve) {
                    if (!curveInstancesByLayer.ContainsKey(layer)) {
                        curveInstancesByLayer.Add(layer, new CurveInstance(animator, animator.mySheet, layer, layer.frames.IndexOf(startFrame), animator.transform.position));
                    }

                    //curveInstancesByLayer[layer].ProcessByFrame(layer, index);
                    FramePropertyHandler.ApplyCurveProperties(animator);

                    //TODO:
                    //How do other gameobjects and scripts look for / track the updates?
                    //  - By Name... "Velocity Curve"
                    //RetroAnimatorCurve curve = core.animator.manager.GetCurve("Velocity");
                    //Can they subscribe to an event that triggers when these are updated?
                }
            }
        }


        void ProcessBoxFrame(Group group, Layer layer, int frameIndex) {
            ColliderInfo info = layerColInfo[layer];
            Frame frame = layer.frames[frameIndex];
            FrameData data = layer.GetFrameData(frameIndex);

            bool rounded = info.col.edgeRadius > 0;
            if (data.props.ContainsKey("forceRounded")) {
                rounded = data.props["forceRounded"].boolVal;
            }
            //update our collider in the scene (feed "istrigger" back to itself, no change)
            SetColliderProperties(
                info.col,
                data.rect,
                mySheet.spriteList[layer.frames.IndexOf(frame)],
                rounded,
                info.col.isTrigger,
                animator.preferences.boxDictionary[group.myBoxType].material,
                animator.spatialOffset
            );

            //update the current "hitbox" object in the dictionary
            activeFrameObjects[info.col] = frame;

            //unban any keyframes, enable them
            if (frame.IsKeyFrame()) {
                info.SetFrame(animator.playingFrame, GetModifiedProperties(data.props));//update our "ColliderInfo" object in the scene
                info.col.enabled = true;
                ClearBannedAnimators(info.col);

            } else {
                info.SetFrame(animator.playingFrame);
            }

            //disable empty frames
            if (frame.kind == Frame.Kind.Empty) {
                layerColInfo[layer].col.enabled = false;
            }
        }

        public void ClearFrame() { //update the colliders / triggers to the given frame
            foreach (KeyValuePair<BoxCollider2D, Frame> b in activeFrameObjects) {
                b.Key.enabled = false;
                ClearBannedAnimators(b.Key);
            }
        }

        public void SetColliderProperties(BoxCollider2D col, Rect r, Sprite s, bool rounded, bool trigger, PhysicsMaterial2D material, Vector2 offset) {
            float radius = 0.02f;
            if (rounded) {
                col.edgeRadius = radius;
                r = RoundHitbox(r, radius);
            } else {
                col.edgeRadius = 0;
            }

            Rect mappedRect = MapBoxRectToTransform(r, s);

            col.offset = mappedRect.min + offset;
            col.size = mappedRect.size;

            col.isTrigger = trigger;
            col.sharedMaterial = material;
        }
        public void SetModifier(PropertyModifier p) {
            modifier = p;
        }

        public BoxProps GetModifiedProperties(BoxProps props) { //take some boxProperties, apply modifier functions to them

            BoxProps boxProps = new BoxProps();

            foreach (KeyValuePair<string, BoxProperty> sbp in props) {
                boxProps.Add(sbp.Key, BoxProperty.Clone(sbp.Value));
            }
            if (modifier != null) {
                if (boxProps.ContainsKey(modifier.modName)) {
                    if (boxProps[modifier.modName].stringVal.Equals(modifier.modValue)) {
                        foreach (string s in modifier.modifiers.Keys) {
                            BoxProperty p;
                            if (boxProps.ContainsKey(s)) { //if we already have a property with that name
                                p = boxProps[s];
                                p.SetPropertyValues(modifier.modifiers[p.name](p));

                            } else { //otherwise make a new property
                                p = new BoxProperty(s, PDataType.Bool);
                                p.SetPropertyValues(modifier.modifiers[p.name](p));
                                boxProps.Add(s, p); //add it to props

                            }
                        }
                    }
                }
            }
            return boxProps;

        }
        public void BanAnimator(Collider2D collider, RetroAnimator otherAnimator) {
            if (bannedColliders.ContainsKey(collider)) { //if the hitbox has been banned before
                if (!bannedColliders[collider].Contains(otherAnimator)) { //if we haven't already got the record of the animator(double checking)
                    bannedColliders[collider].Add(otherAnimator); //add the animator to the banned list for this hitbox
                }
            } else { //otherwise, add a new record for this hitbox, and put the other animator in as the first item in the list.
                bannedColliders.Add(collider, new List<RetroAnimator>() { otherAnimator });
            }
        }

        public void ClearBannedAnimators(Collider2D collider) {
            if (bannedColliders.ContainsKey(collider)) {
                bannedColliders.Remove(collider);
            }
        }

        public bool IsBanned(Collider2D collider, RetroAnimator animator) {
            if (bannedColliders.ContainsKey(collider)) { //if the hitbox has been banned before
                if (bannedColliders[collider].Contains(animator)) { //if we haven't already got the record of the animator(double checking)
                    return true;
                }
            }
            return false;
        }
        public Dictionary<Collider2D, List<RetroAnimator>> GetBannedColliders() {
            return bannedColliders;
        }

        public void ApplySpatialOffset(Vector2 offset) {

            foreach (ColliderInfo c in layerColInfo.Values) {
                Sheet s = c.GetSheet();
                Group group = c.GetGroup();
                Layer layer = c.GetRetroLayer();
                int frame = c.GetFrame();

                FrameData data = layer.GetFrameData(frame);
                SetColliderProperties(
                   c.col,
                   data.rect,
                   mySheet.spriteList[frame],
                   c.col.edgeRadius > 0,
                   c.col.isTrigger,
                   animator.preferences.boxDictionary[group.myBoxType].material,
                   offset
               );

            }
        }

        public static bool HasColliderOfType(string name, Sheet s, int frame) {
            foreach (Group g in s.groups) {
                if (g.myBoxType == name) {
                    foreach (Layer l in g.layers) {
                        bool followingHitbox = false;
                        for (int i = 0; i < l.frames.Count; i++) {
                            if (l.frames[i].kind == Frame.Kind.KeyFrame) {
                                followingHitbox = true;
                            } else if (l.frames[i].kind == Frame.Kind.Empty) {
                                followingHitbox = false;
                            }

                            if (i == frame && followingHitbox) {
                                return true;
                            }

                        }
                    }
                }
            }
            return false;
        }

        Rect RoundHitbox(Rect r, float radius) {
            radius *= 100;
            r.x += radius;
            r.y += radius;
            r.width -= radius * 2;
            r.height -= radius * 2;
            return r;
        }

        public static Rect MapBoxRectToTransform(Rect r, Sprite s) {
            Vector2 offset = new Vector2((r.x + r.width * 0.5f - s.pivot.x) / s.pixelsPerUnit, (s.rect.height - r.y - r.height * 0.5f - s.pivot.y) / s.pixelsPerUnit);
            Vector2 size = new Vector2(r.width / s.pixelsPerUnit, r.height / s.pixelsPerUnit);
            return new Rect(offset, size);
        }

        public Frame.Kind GetColliderType(Collider2D col) {
            return activeFrameObjects[(BoxCollider2D)col].kind;
        }

        public static BoxProps GetColliderProperties(Collider2D col) {
            foreach (ColliderInfo info in col.GetComponents<ColliderInfo>()) {
                if (info.col == col) {
                    return info.GetProperties();
                }
            }
            return null;
        }

        public static string GetColliderBoxType(Collider2D col) {
            foreach (ColliderInfo info in col.GetComponents<ColliderInfo>()) {
                if (info.col == col) {
                    return info.GetBoxType();
                }
            }
            return null;
        }

    }
}
