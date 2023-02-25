using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CPUInputController : InputController {

    public Life target;
    public Vector2 tolerance;

    float timeAtLastAction;
    float timeSinceLastAction => Time.time - timeAtLastAction;

    public Transform centreStage;
    public float stageDistanceTolerance = 3;
    public float dexterity = 0.2f;

    float timeAtLastTarget = 0;
    // Start is called before the first frame update
    void Start() {
        character = GetComponent<Character>();

        if (centreStage == null) {
            centreStage = FindObjectOfType<Level>().transform;
        }
    }

    // Update is called once per frame
    void Update() {
        ManageTargets();
        ClearInputs();

        if (VectorToCentreStage().magnitude > stageDistanceTolerance) {
            GetBackOnStage();

        } else if (target != null && character.canAttack) {
            if (timeSinceLastAction > dexterity) {
                NavigateToTarget();
                AttackTarget();
            }
        }
    }

    void GetBackOnStage() {
        Vector2 v = VectorToCentreStage();
        movement.x = v.x;
        if (v.y > 0) {
            jumpHeld = true;
            jumpPressed = true;
            timeAtLastAction = Time.time;
        }
    }

    void NavigateToTarget() {
        Vector2 v = VectorToTarget();
        if (Mathf.Abs(v.x) > tolerance.x) {
            movement.x = v.normalized.x;
        }

        if (Mathf.Abs(v.y) > tolerance.y) {
            if (v.y >= 0) {
                jumpHeld = true;
                jumpPressed = true;
                movement.y = 1;
            } else {
                movement.y = -1;
                timeAtLastAction = Time.time;
            }
        }

    }

    void AttackTarget() {
        if (tolerance.magnitude > VectorToTarget().magnitude) {
            if (timeSinceLastAction > 0.3f) {
                if (Random.value > 0.7f) {
                    specialHeld = true;
                    specialPressed = true;
                } else {
                    attackHeld = true;
                    attackPressed = true;
                    if (Random.value > 0.6f) {
                        movement.y = -1;
                    }

                }
                timeAtLastAction = Time.time;
            }
        }
    }

    Vector2 VectorToTarget() {
        return target.character.position - (Vector2)character.transform.position;
    }

    Vector2 VectorToCentreStage() {
        return (Vector2)(centreStage.position - character.transform.position);
    }

    void ManageTargets() {
        if (target == null || Time.time - timeAtLastTarget > 7) {
            Life[] livingThings = FindObjectsOfType<Life>().Where(x => x.team != character.life.team).ToArray();

            if (livingThings.Length > 0) {
                target = livingThings[Random.Range(0, livingThings.Length)];
                timeAtLastTarget = Time.time;
            }
        }
    }
}
