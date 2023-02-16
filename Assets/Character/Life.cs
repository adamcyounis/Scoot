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

    public UnityEvent<float, float> energyEvent = new UnityEvent<float, float>();
    [HideInInspector]

    public UnityEvent<float, float> healthEvent = new UnityEvent<float, float>();
    public UnityEvent dieEvent = new UnityEvent();

    [HideInInspector]
    public Character character;

    public bool targetable = true;
    [HideInInspector] public bool dying = false;
    public bool armoured = false;
    public bool invincible = false;

    [HideInInspector]
    public float armour = 0.5f;

    [HideInInspector]
    public float resist = 0;

    [HideInInspector]
    public float invulnerableStartTime;

    [HideInInspector]
    public float invulnerableDuration = 0.0f;

    [HideInInspector]
    public int health; //the amount of HP we have right now

    [HideInInspector]
    public int prevHealth; //the amount of HP we had last frame

    [FormerlySerializedAs("maxHealth")]
    [SerializeField]
    public int baseHealth; //the total amount of HP we have without modifiers
    public int extraHealth; //"additional" health provided by modifiers
    [HideInInspector]
    public float multHealth; //"multiplier" on base + extra health
    public int maxHealth; //total health after all calculations

    public float energy;
    [HideInInspector]
    public float prevEnergy; //the amount of HP we had last frame

    [FormerlySerializedAs("maxEnergy")]
    [SerializeField]
    public int baseEnergy; //the total amount of EN we have without modifiers
    public int extraEnergy; //"additional" energy provided by modifiers
    [HideInInspector]
    public float multEnergy; //"multiplier" on base + extra health
    public int maxEnergy; //total energy after all calculations

    [HideInInspector]
    public bool gainedEnergy;
    public readonly float energyRecoveryDelay = 1;
    public float poise;
    private Vector2 damageVector;


    [HideInInspector]
    public Vector3 spawnPos;
    [HideInInspector]
    public Vector3 initLocalScale;
    [HideInInspector]
    public Quaternion initRotation;

    public Retro.RetroAnimator animator;

    bool hitThisFrame = false;

    public static float deathStun = 0.4f;
    public float normalHP {
        get { return (float)health / (float)maxHealth; }
    }

    public List<CollisionInfo> collisions;
    public float timeAtLastSpentEnergy;
    // Use this for initialization
    void Awake() {
        spawnPos = transform.position;
        initLocalScale = transform.localScale;
        initRotation = transform.rotation;

        if (GetComponent<Character>() is Character c) {
            character = c;
            animator = c.animator;
        }

        if (animator == null) {
            animator = GetComponent<Retro.RetroAnimator>();
        }

        collisions = new List<CollisionInfo>();


    }
    void Start() {
        CalculateStats(extraHealth, multHealth, extraEnergy, multEnergy);
        health = baseHealth;
        energy = maxEnergy;
        invulnerableStartTime = 0;

    }


    private void FixedUpdate() {

        ResolveCollisions();
    }

    // Update is called once per frame
    void Update() {
        if (IsReadyToDie() && !dying && !animator.IsInHitStop()) {
            if (health < 0) health = 0;
            dying = true;
            SendMessage("PrepareToDie", SendMessageOptions.DontRequireReceiver);
            dieEvent.Invoke();
        }
    }


    public void CalculateStats(int extraHP, float multHP, int extraEn, float multEn) {
        extraHealth = extraHP;
        multHealth = multHP;
        maxHealth = (int)((baseHealth + extraHealth) * (1 + multHealth));

        extraEnergy = extraEn;
        multEnergy = multEn;
        maxEnergy = (int)((baseEnergy + extraEnergy) * (1 + multEnergy));
    }

    public void LateUpdate() {
        hitThisFrame = false;
        gainedEnergy = false;
        prevHealth = health;
        prevEnergy = energy;
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

            ApplyDamage(col);
            ApplyHitstop(col);
            ApplyAttackVector(col);

            if (!col.ignoreHurt) {
                hurtConfirmEvent.Invoke(col);
            }

            //ban the collider
            col.collision.collider.Ban(animator);
            if (health < 0) health = 0;

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
        float prev = health;
        health -= info.damage;
        healthEvent.Invoke(prev, health);

    }

    void ApplyHitstop(CollisionInfo info) {
    }

    void ApplyAttackVector(CollisionInfo info) {
        Retro.ColliderInfo collidee = info.collision.collidee;
        Retro.ColliderInfo collider = info.collision.collider;

        //get angle if we don't have one already
        Vector2 damageVector = info.damageVector != Vector2.zero ? info.damageVector : (Vector2)Helpers.GetCollisionVector(collidee.col, collider.col);

        this.damageVector = damageVector;

        //add velocities to camera
        //Engine.engine.ExplodeParticles(animator.spriteRenderer.sprite, transform.position, damageVector, 0);

        //Add velocity to our rigidbody equal to the damagevector * knockback;
        //this happens only when we aren't resolving this with Stun Behaviour
        if (character == null) {
            animator.rigidBody.velocity = damageVector * info.knockback;
        }
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

    public void SetArmoured(bool v, float a) {
        armoured = v;
        armour = a;
    }

    public void GiveHealth(float amount) {
        float prev = health;

        health += (int)amount;
        if (health > maxHealth) health = maxHealth;

        healthEvent.Invoke(prev, health);
    }


    public void StartRespawn() {
        transform.position = spawnPos;
        transform.localScale = initLocalScale;
        transform.rotation = initRotation;

        healthEvent.Invoke(health, maxHealth);

        health = maxHealth;
        energy = maxEnergy;

        dying = false;
    }

    public bool IsReadyToDie() {
        return (health <= 0 && !invincible);
    }

    public bool IsVulnerable() {
        return (Time.time - invulnerableStartTime > invulnerableDuration) ? true : false;
    }

    public Vector2 GetDamageVector() {
        return (hitThisFrame) ? damageVector : Vector2.zero;
    }

}
