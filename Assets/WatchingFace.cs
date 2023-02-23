using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchingFace : MonoBehaviour {

    public Transform darks;
    public Vector2 eyeRange;
    public Vector2 targetRange;

    public Transform target;
    // Start is called before the first frame update
    void Start() {
        if (target == null && FindObjectOfType<Coots>() is Coots c) {
            target = c.transform;
        }
    }

    // Update is called once per frame
    void Update() {
        if (target != null) {
            Vector2 delta = (target.transform.position - transform.position).normalized;
            float x = Helpers.Map(delta.x, -targetRange.x, targetRange.x, -eyeRange.x, eyeRange.x, true);
            float y = Helpers.Map(delta.y, -targetRange.y, targetRange.y, -eyeRange.y, eyeRange.y, true);

            darks.transform.localPosition = new Vector2(x, y);
        }

    }
}
