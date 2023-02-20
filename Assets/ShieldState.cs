using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldState : State {

    public SpriteRenderer shieldSprite;
    public float shieldAmount = 100;
    float maxShield;
    bool active;
    public Retro.Sheet s_shield;

    public void Start() {
        maxShield = shieldAmount;
    }
    // Start is called before the first frame update
    public override void Enter() {
        shieldSprite.enabled = true;
        animator.Play(s_shield, 10, true, true);
        active = true;
    }

    // Update is called once per frame
    void Update() {
        UpdateShieldSprite();
    }

    void FixedUpdate() {
        if (!active && shieldAmount < maxShield) {
            shieldAmount += 0.5f;
        }
    }

    void UpdateShieldSprite() {
        shieldSprite.color = new Color(1, 1, 1, shieldAmount / maxShield);
        shieldSprite.transform.localScale = Vector3.one * (shieldAmount / maxShield);
    }

    public override void Do() {
        shieldSprite.enabled = true;
        if (!ShouldShield()) {
            Complete("complete!");
        }
    }

    public override void Exit() {
        base.Exit();
        shieldSprite.enabled = false;
        active = false;


    }

    public void Hit(float damage) {
        shieldAmount -= damage;
    }

    public bool ShouldShield() {
        return Input.GetButton("Shield") && core.selfAwareness.grounded;
    }
}
