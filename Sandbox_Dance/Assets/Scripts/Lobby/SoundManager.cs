using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource effectAudio;

    public AudioClip[] effectSounds;
    public AudioClip[] bgmClips;


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
    }


    public void SetBGM(bool check)
    {
        bgmSource.mute = check;
        PlayerPrefs.SetInt("BGM_Mute", (check) ? 1 : 0);
    }

    public void SetEffect(bool check)
    {
        PlayerPrefs.SetInt("Effect_State", (check) ? 1 : 0);
    }

    public void PlayEffect(int i)
    {
        effectAudio.PlayOneShot(effectSounds[i], PlayerPrefs.GetInt("Effect_State"));
    }

    public void PlayBGM(int i)
    {
        bgmSource.clip = bgmClips[i];
        bgmSource.Play();
    }
}
