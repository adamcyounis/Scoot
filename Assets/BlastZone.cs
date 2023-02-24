using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastZone : MonoBehaviour {
    public float shakeAmount = 0.3f;
    public bool isRoof = false;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnTriggerEnter2D(Collider2D other) {

        if (other.GetComponent<Retro.ColliderInfo>() is Retro.ColliderInfo c) {
            if (c.GetAnimator().GetComponent<Character>() is Character ch) {
                if (isRoof && !(ch.state is Stun)) {
                    return;
                } else {
                    //maybe there's a spawn manager that kills and resets the character to a specific spawn point?
                    ch.stocksRemaining -= 1;
                    GameManager.gm.shaker.Shake(shakeAmount, 0.3f);
                    ch.gameObject.SetActive(false);

                    if (ch.stocksRemaining > 0) {
                        StartCoroutine(RespawnWithDelay(ch));
                    } else {
                        Debug.Log("Player Defeated!");
                    }

                }

            }
        }
    }

    public IEnumerator RespawnWithDelay(Character ch) {
        yield return new WaitForSeconds(1f);
        ch.life.percent = 0;
        ch.transform.position = transform.parent.position;
        ch.body.velocity = Vector2.zero;
        ch.gameObject.SetActive(true);

    }
}
