using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource effectAudio;

    public AudioClip[] effectSounds;
    public AudioClip[] bgmClips;

    bool viveOn;

    private static SoundManager instance = null;

    public static SoundManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this.GetComponent<SoundManager>();
        }
    }

    void Update()
    {
        bgmSource.mute = PlayerPrefs.GetInt("BGM_Mute") == 1 ? false : true;
        effectAudio.mute = PlayerPrefs.GetInt("Effect_Mute") == 1 ? false : true;
        viveOn = PlayerPrefs.GetInt("Vibrate_Mute") == 1 ? false : true;
    }

    public void Vibrate()
    {
        if (!viveOn)
        {
            Debug.Log("BBB");
            Handheld.Vibrate();
        }
    }

    public void SetBGM(bool check)
    {
        bgmSource.mute = check;
        PlayerPrefs.SetInt("BGM_Mute", (check) ? 1 : 0);
    }

    public void PlayBGM(int num)
    {
        bgmSource.clip = bgmClips[num];
        bgmSource.Play();
    }

    public void SetEffect(bool check)
    {
        effectAudio.mute = check;
        PlayerPrefs.SetInt("Effect_Mute", (check) ? 1 : 0);
    }

    public void SetVibrate(bool check)
    {
        PlayerPrefs.SetInt("Vibrate_Mute", (check) ? 1 : 0);
    }

    public void PlayEffect(int num)
    {
        effectAudio.PlayOneShot(effectSounds[num]);
    }
}
