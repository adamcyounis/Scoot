using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCamera : MonoBehaviour {
    public List<Transform> subjects;
    public Camera cam;
    public float scaleFactor;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        Bounds bounds = GetSubjectBounds();
        float targetSize = Mathf.Max(bounds.size.x, bounds.size.y) * scaleFactor;

        targetSize = Mathf.Clamp(targetSize, 1.8f, 3.6f);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVel, 0.4f);
        Vector2 targetPos = (Vector2)bounds.center + (Vector2.down * bounds.size.y * 0.25f);
        Vector2 newPos = Vector2.SmoothDamp(transform.position, targetPos, ref posVel, 0.4f);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }
    Vector2 posVel;
    float sizeVel;

    Bounds GetSubjectBounds() {

        Vector2 min;
        Vector2 max;
        min = subjects[0].position;
        max = subjects[0].position;

        foreach (Transform t in subjects) {
            if (t.position.x < min.x) {
                min.x = t.position.x;
            }

            if (t.position.y < min.y) {
                min.y = t.position.y;
            }

            if (t.position.x > max.x) {
                max.x = t.position.x;
            }

            if (t.position.y > max.y) {
                max.y = t.position.y;
            }
        }
        return new Bounds((max + min) / 2f, max - min);
    }
}
