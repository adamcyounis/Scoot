using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System.Linq;
using System;


public class Life : MonoBehaviour {
    public class LifeEvent : UnityEvent<CollisionInfo> { }
    [HideInInspector]
    public LifeEvent hurtConfirmEvent = new LifeEvent();
    [HideInInspector]

    public LifeEvent checkDeferredCollisionEvent = new LifeEvent();
    [HideInInspector]

    public UnityEvent<float, float> healthEvent = new UnityEvent<float, float>();
    public UnityEvent dieEvent = new UnityEvent();

    [HideInInspector]
    public Character character;

    public bool armoured = false;
    public bool invincible = false;

    [HideInInspector]
    public float invulnerableStartTime;

    [HideInInspector]
    public float invulnerableDuration = 0.0f;

    public float percent; //the amount of HP we have right now

    [HideInInspector]
    public float prevPercent; //the amount of HP we had last frame

    [HideInInspector]
    public Vector3 spawnPos;
    [HideInInspector]
    public Vector3 initLocalScale;
    [HideInInspector]
    public Quaternion initRotation;
    [HideInInspector]
    public Retro.RetroAnimator animator;


    public List<CollisionInfo> collisions;
    public float timeAtLastSpentEnergy;

    public static float knockbackFloor = 70;
    public int team;

    public float kbFactor = 1;
    // Use this for initialization
    void Awake() {
        spawnPos = transform.position;
        initLocalScale = transform.localScale;
        initRotation = transform.rotation;

        if (GetComponent<Character>() is Character c) {
            character = c;
            animator = c.animator;
            c.life = this;

        }

        if (animator == null) {
            animator = GetComponent<Retro.RetroAnimator>();
        }

        collisions = new List<CollisionInfo>();

    }
    void Start() {
        percent = 0;
        invulnerableStartTime = 0;
        character.animator.boxManager.collisionEvent.AddListener(HandleCollisionEvent);

    }


    private void FixedUpdate() {

        ResolveCollisions();
    }

    // Update is called once per frame
    void Update() {

    }


    public void LateUpdate() {
        prevPercent = percent;
    }

    public void RegisterCollision(Retro.Collision c) {
        collisions.Add(new CollisionInfo(this, c));
    }

    void ResolveCollisions() {
        if (collisions.Count > 0) {
            CollisionInfo selectedCollision = SelectCollision();
            checkDeferredCollisionEvent.Invoke(selectedCollision);

            if (selectedCollision.handlers.Count == 0) {
                ApplyCollision(selectedCollision);
            }
            //send hitconfirm to animator.
            selectedCollision.collision.collider.GetAnimator().boxManager.hitConfirmEvent.Invoke(selectedCollision);

            collisions.Clear();
        }
    }

    public void ApplyCollision(CollisionInfo col) {
        bool banned = col.collision.collider.IsBanned(animator); //make sure there's actually a valid collider 

        if (IsVulnerable() && !banned) {
            if (character.state is ShieldState s) {
                s.Hit(col.damage);

            } else {
                ApplyHitEffect(col);

                ApplyDamage(col);
                if (character.body != null) {
                    ApplyKnockback(col);
                }
                if (!col.ignoreHurt) {
                    hurtConfirmEvent.Invoke(col);
                }
            }
            ApplyHitstop(col);
            //ban the collider
            col.collision.collider.Ban(animator);
            if (percent < 0) percent = 0;

        }
    }

    CollisionInfo SelectCollision() {
        CollisionInfo selectedCollision = collisions[0];
        CollisionInfo shieldCol = null;//TODO: multiple shields?

        CollisionInfo mostVulnerableCollision = collisions[0];
        foreach (CollisionInfo c in collisions) {
            //TODO: replace "Shield" with "priority"
            if (c.collision.collidee.GetBoxType() == "EnemyShield") {//we are shield and selected is shield

                if (shieldCol == null || c.damage < shieldCol.damage) {//pick the shield with the least damage
                    shieldCol = c;
                }
            } else {
                if (c.damage > mostVulnerableCollision.damage) { //neither of us are shields
                    mostVulnerableCollision = c;
                }
            }
        }

        if (shieldCol != null) {
            if (shieldCol.poise > 0) { //shield break
                selectedCollision = shieldCol;
                //TODO: Add callback to my animator to tell me I got shield broken
            } else {
                selectedCollision = mostVulnerableCollision;
            }
        } else {
            selectedCollision = mostVulnerableCollision;
        }

        return selectedCollision;
    }

    void ApplyDamage(CollisionInfo info) {
        float prev = percent;
        percent += info.damage;
        healthEvent.Invoke(prev, percent);
    }

    void ApplyHitstop(CollisionInfo info) {
        //GameManager.gm.HitStop();
        float mod = (info.damage / 10f);
        mod = Mathf.Max(mod, 0.1f);
        animator.HitStop(0.12f * mod);

        info.collision.collider.GetAnimator().HitStop(0.08f * mod);
        if (character.shaker != null) {
            character.shaker.Shake(0.1f * (mod / 2f), 0.12f * mod);
        }

    }

    void ApplyKnockback(CollisionInfo info) {
        Retro.ColliderInfo collidee = info.collision.collidee;
        Retro.ColliderInfo collider = info.collision.collider;

        //get angle if we don't have one already
        Vector2 direction = info.damageVector != Vector2.zero ? info.damageVector : (Vector2)Helpers.GetCollisionVector(collidee.col, collider.col);

        //add velocities to camera
        //Engine.engine.ExplodeParticles(animator.spriteRenderer.sprite, transform.position, damageVector, 0);

        //Add velocity to our rigidbody equal to the damagevector * knockback;
        //this happens only when we aren't resolving this with Stun Behaviour
        animator.rigidBody.velocity = (direction * info.knockback * ((knockbackFloor + percent) * 0.003f) * kbFactor);
        animator.rigidBody.velocity += Vector2.up * ((knockbackFloor + percent) * 0.007f) * kbFactor;

    }
    void ApplyHitEffect(CollisionInfo col) {
        Retro.RetroAnimator anim = col.collision.collider.GetAnimator();
        HitEffectPool.p.SpawnEffect(anim.transform.position, col.damageVector, col.damage, anim.life.character);
    }

    public void SetInvulnerable(float t) {
        invulnerableStartTime = Time.time;
        invulnerableDuration = t;
    }


    public void Invulnerable() {
        SetInvulnerable(0.1f);
    }

    public void MakeVulnerable() {
        invulnerableStartTime = Time.time - invulnerableDuration;
    }

    public void SetArmoured(bool v) {
        armoured = v;
    }

    public void StartRespawn() {
        transform.position = spawnPos;
        transform.localScale = initLocalScale;
        transform.rotation = initRotation;
        healthEvent.Invoke(percent, 0);

    }

    public bool IsReadyToDie() {
        return (percent <= 0 && !invincible);
    }

    public bool IsVulnerable() {
        return (Time.time - invulnerableStartTime > invulnerableDuration) ? true : false;
    }

    public void HandleCollisionEvent(Retro.Collision c) {
        string ce = LayerMask.LayerToName(c.collidee.GetPhysicsLayer());
        string cr = LayerMask.LayerToName(c.collider.GetPhysicsLayer());
        bool oneHitTwo = ce.Equals("Hit") && cr.Equals("Hurt");
        bool twoHitOne = ce.Equals("Hurt") && cr.Equals("Hit");

        bool hitMe = oneHitTwo && c.collider.GetAnimator().life == this ||
         twoHitOne && c.collidee.GetAnimator().life == this;
        bool differentTeams = c.collidee.GetAnimator().life.team != c.collider.GetAnimator().life.team;
        if (hitMe && differentTeams) {
            RegisterCollision(c);
        }
    }
}

