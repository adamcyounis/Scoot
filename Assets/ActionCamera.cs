using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCamera : MonoBehaviour {
    public List<Transform> subjects;
    public Camera cam;
    public float scaleFactor;

    [Range(1f, 1.8f)]
    public float minSize;

    [Range(1.8f, 3.6f)]
    public float maxSize;
    [Range(0f, 0.5f)]
    public float animTime = 0.2f;

    // Start is called before the first frame update
    void Start() {
        GameSystem.system.newCharacter.AddListener(AddNewPlayer);
        foreach (Character c in FindObjectsOfType<Character>()) {
            subjects.Add(c.transform);
        }
    }

    private void OnDestroy() {
        GameSystem.system.newCharacter.RemoveListener(AddNewPlayer);
    }

    void AddNewPlayer(Transform t) {
        subjects.Add(t);
    }

    // Update is called once per frame
    void Update() {
        Vector2 targetPos = default;
        float targetSize = default;

        if (subjects.Count > 0) {
            if (subjects.Count > 1) {
                Bounds bounds = GetSubjectBounds();
                targetSize = Mathf.Max(bounds.size.x, bounds.size.y) * scaleFactor;

                targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
                targetPos = (Vector2)bounds.center /*+ (Vector2.down * bounds.size.y * 0.25f)*/;

            } else {

                targetSize = 2.2f;
                targetPos = subjects[0].position;
            }


            Vector2 newPos = Vector2.SmoothDamp(transform.position, targetPos, ref posVel, 0.4f);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVel, 0.4f);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }

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
