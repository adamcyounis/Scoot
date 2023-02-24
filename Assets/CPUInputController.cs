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
    // Start is called before the first frame update
    void Start() {
        character = GetComponent<Character>();
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
            if (v.y > 0) {
                jumpHeld = true;
                jumpPressed = true;

            } else {
                movement.y = -1;
                timeAtLastAction = Time.time;
            }
        }

    }

    void AttackTarget() {
        if (tolerance.magnitude > VectorToTarget().magnitude) {
            if (timeSinceLastAction > 0.3f) {
                attackHeld = true;
                attackPressed = true;
                timeAtLastAction = Time.time;
                if (Random.value > 0.6f) {
                    movement.y = -1;
                }
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
        if (target == null) {
            Life[] lives = FindObjectsOfType<Life>().Where(x => x.team != character.life.team).ToArray();

            if (lives.Length > 0) {
                target = lives[Random.Range(0, lives.Length)];
            }
        }
    }
}
