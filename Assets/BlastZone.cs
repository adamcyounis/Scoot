using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class BlastZone : MonoBehaviour {
    public Level level;
    public float shakeAmount = 0.3f;
    public bool isRoof = false;

    public List<AudioClip> callouts => GameSystem.system.callouts;
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
                        SoundSystem.system.PlaySFX(callouts[Random.Range(0, callouts.Count)]);

                    } else {
                        if (GameManager.gm.cam.subjects.Contains(ch.transform)) {
                            GameManager.gm.cam.subjects.Remove(ch.transform);
                        }
                        GameSystem.system.PlayerDefeated(ch);
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
        level.SpawnCharacter(ch);
    }
}
