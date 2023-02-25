using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour {
    public static SoundSystem system;
    public AudioSource SFXSource;
    public AudioSource musicSource;
    public AudioSource UISource;
    float defaultSFXVolume;
    float defaultMusicVolume;
    float defaultUIVolume;

    private void Awake() {
        if (system == null) {
            system = this;
            defaultSFXVolume = SFXSource.volume;
            defaultMusicVolume = musicSource.volume;
            defaultUIVolume = UISource.volume;

        }
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }


    public void PlaySFX(AudioClip clip, float vol = 0) {
        SFXSource.PlayOneShot(clip, vol == 0 ? 1 : vol);
    }

    public void PlayMusic(AudioClip clip, float vol = 0) {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayUISound(AudioClip clip, float vol = 0) {
        UISource.PlayOneShot(clip, vol == 0 ? 1 : vol);
    }
}
