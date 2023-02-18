using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retro {
    public class ColliderInfo : MonoBehaviour {

        private RetroAnimator animator;

        private Sheet sheet;//the sheet this collider comes from
        private int physicsLayer;
        public int group;//the group in which it's found
        public int layer;//the layer in which it's found
        public int frame; //the animation frame on which it occurs
                          //private float value; //the amount of damage (or other value) this collider inflicts
        public BoxProps props; //the list of properties owned by this hitbox
        public BoxCollider2D col; //the collider itself
        public void Setup(Sheet s, int group_, int layer_, BoxProps props_, BoxCollider2D collider_, RetroAnimator animator_) { //"Constructor"
            sheet = s;
            group = group_;
            layer = layer_;
            physicsLayer = gameObject.layer;
            frame = 0;
            //value = v;
            props = props_;
            col = collider_;
            animator = animator_;
        }

        public void Setup(Sheet s, int group_, int layer_) {
            sheet = s;
            group = group_;
            layer = layer_;
        }

        public void SetSheet(Sheet s) {
            sheet = s;
        }

        public void SetFrame(int f) { //"Update"
            frame = f;
        }


        public void SetFrame(int f, BoxProps props_) { //"Update"
            SetFrame(f);
            props = props_;
        }

        public BoxProps GetProperties() {
            return props;
        }

        public string GetBoxType() {
            if (sheet != null) {
                return sheet.groups[group].myBoxType;
            } else {
                return "";
            }
        }

        public Retro.Sheet GetSheet() {
            return sheet;
        }

        public Group GetGroup() {
            return sheet.groups[group];
        }

        public Retro.Layer GetRetroLayer() {
            return sheet.groups[group].layers[layer];
        }

        public int GetPhysicsLayer() {
            return physicsLayer;
        }

        public int GetFrame() {
            return frame;
        }

        public void OnTriggerEnter2D(Collider2D _col) {
            ColliderInfo otherCol = _col.GetComponent<ColliderInfo>();
            Collision c = new Collision(this, otherCol);
            if (otherCol) {
                if (animator != null) {
                    animator.boxManager.collisionEvent.Invoke(c);
                    if (otherCol.animator != null) {
                        otherCol.animator.boxManager.collisionEvent.Invoke(c);
                    }
                }
            }
        }

        public void Ban(RetroAnimator otherAnimator) {
            animator.boxManager.BanAnimator(col, otherAnimator);
        }

        public bool IsBanned(RetroAnimator otherAnimator) {
            return animator.boxManager.IsBanned(col, otherAnimator);
        }

        public Retro.RetroAnimator GetAnimator() {
            return animator;
        }


    }

}