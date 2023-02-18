using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInfo {


    public List<MonoBehaviour> handlers;
    public Retro.Collision collision;
    //Collidee 
    public bool ignoreHurt => collideeProps.ignoreHurt;
    //Collider
    public int damage => (int)(colliderProps.damage * collideeProps.vulnerability);
    public float knockback => colliderProps.knockback;
    public float hitstopValue => colliderProps.hitstopValue;
    public Vector2 damageVector => colliderProps.damageVector;

    public Vector2 scaledDamageVector => damageVector * knockback;

    public float poise;
    public ProcessedColliderProperties collideeProps;
    public ProcessedColliderProperties colliderProps;
    Life collideeLife;

    public CollisionInfo(Life l, Retro.Collision collision_) {
        collideeLife = l;
        collision = collision_;
        collideeProps = new ProcessedColliderProperties(collision.collidee);
        colliderProps = new ProcessedColliderProperties(collision.collider);
        float collideePoise = collideeProps.poise;
        float colliderPoise = colliderProps.damage + colliderProps.poise;
        poise = collideePoise - colliderPoise;
        handlers = new List<MonoBehaviour>();
    }
}

public struct ProcessedColliderProperties {

    //Collidee 
    public bool ignoreHurt;
    public float vulnerability;

    //Collider
    public int damage;
    public float knockback;
    public float hitstopValue;
    public Vector2 damageVector;
    public float poise;
    public Collider2D collider;
    public string colliderTag;

    public ProcessedColliderProperties(Retro.ColliderInfo info) {
        this = new ProcessedColliderProperties();
        poise = 0;
        vulnerability = 1;
        hitstopValue = 1;
        knockback = 1;
        colliderTag = default;
        collider = info.col;
        foreach (KeyValuePair<string, Retro.BoxProperty> bp in info.props) { //process properties
            ProcessProperty(bp.Value);
        }
    }

    void ProcessProperty(Retro.BoxProperty p) {

        switch (p.name) {
            case "Damage":
                damage = (int)p.floatVal;
                break;

            case "hitstop":
                hitstopValue = (p.floatVal == 0) ? 1 : p.floatVal;

                //if (Engine.IsArmin(collision.collidee.col)) {
                //    hitstopValue *= ArminBehaviour.hitStopMod;
                //}

                break;

            case "Direction":
                damageVector = (p.vectorVal).normalized * collider.transform.lossyScale;
                break;

            case "tag":
                colliderTag = p.stringVal;
                //SendMessage(p.stringVal, col, SendMessageOptions.DontRequireReceiver); //call function
                break;

            case "Knockback":
                knockback = p.floatVal;
                break;

            case "ignoreHurt":
                ignoreHurt = p.boolVal;
                break;

            case "vulnerability":
                vulnerability = p.floatVal;
                break;
            case "poise":
                poise = p.floatVal;
                break;
        }

    }



}