using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectPool : MonoBehaviour {

    public Retro.RetroAnimator effectPrefab;
    public List<Retro.Sheet> sheets;
    Queue<Retro.RetroAnimator> pool = new Queue<Retro.RetroAnimator>();
    public static HitEffectPool p;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void SpawnEffect(Vector2 position, Vector2 angle, float scaleFactor) {
        Retro.RetroAnimator anim;
        if (pool.Count > 0) {
            anim = pool.Dequeue();
        } else {
            GameObject g = GameObject.Instantiate(effectPrefab.gameObject);
            anim = g.GetComponent<Retro.RetroAnimator>();
            anim.finished.AddListener(RespondToEffectFinished);
        }

        anim.gameObject.SetActive(true);
        anim.Play(sheets[Random.Range(0, sheets.Count)], 15, false, true);

        anim.spriteRenderer.transform.localScale = Vector3.one * (scaleFactor / 30f);
        //anim.transform.right = angle;
        anim.spriteRenderer.flipX = angle.x < 0;
        anim.spriteRenderer.flipY = angle.y < 0;
        anim.transform.position = position;
    }

    public void RespondToEffectFinished(Retro.RetroAnimator anim, Retro.Sheet sheet) {
        anim.gameObject.SetActive(false);
        pool.Enqueue(anim);

    }
}
