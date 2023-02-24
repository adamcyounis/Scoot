using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CPUInputController : InputController {

    public Life target;
    public Vector2 tolerance;

    float timeAtLastAttack;
    float timeSinceAttack => Time.time - timeAtLastAttack;

    public Transform centreStage;
    public float stageDistanceTolerance = 3;
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

        } else if (target != null) {
            NavigateToTarget();
            AttackTarget();
        }
    }

    void GetBackOnStage() {
        Vector2 v = VectorToCentreStage();
        movement.x = v.x;
        if (v.y > 0) {
            jumpHeld = true;
            jumpPressed = true;
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
            }
        }

    }

    void AttackTarget() {
        if (tolerance.magnitude > VectorToTarget().magnitude) {
            if (timeSinceAttack > 0.3f) {
                attackHeld = true;
                attackPressed = true;

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
