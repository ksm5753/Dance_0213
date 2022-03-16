using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using static LanguageParse;

public class LanguageChanger : MonoBehaviour
{
    public static LanguageChanger instance_;
    private void Awake()
    {
        if (instance_ == null)
        {
            instance_ = this;
        }

        else Destroy(this);
    }
    [System.Serializable]
    public struct ImageChanger 
    {
        public Image targetImage;
        public Image[] images;
    }
    [System.Serializable]
    public struct TextChanger 
    {
        public Text targetText;
        public string textKey;
    }
    [System.Serializable]
    public struct SpineChanger
    {
        public SkeletonGraphic targetImage;
        public SkeletonDataAsset []images;
    }

    public TextChanger[] textChanger;
    public ImageChanger[] imageChanger;
    public SpineChanger[] spineChanger;

    // Start is called before the first frame update
    void Start()
    {
        instance.LocalizeChnaged += LocalizeChanged;
    }

    private void OnDestroy()
    {
        instance.LocalizeChnaged -= LocalizeChanged;
    }

    string Localize(string key)
    {
        int keyIndex = instance.Languages[0].value.FindIndex(x => x.ToLower() == key.ToLower());
        return instance.Languages[instance.curLangIndex].value[keyIndex];
    }

    public void LocalizeChanged()
    {
        for (int i = 0; i < textChanger.Length; i++)
        {
            textChanger[i].targetText.text = Localize(textChanger[i].textKey);
        }
        
        if(imageChanger.Length > 0)
        {
            for (int i = 0; i < imageChanger.Length; i++)
            {
                imageChanger[i].targetImage.sprite = imageChanger[i].images[PlayerPrefs.GetInt("LangIndex")].sprite;
            }
        }
        
        if(spineChanger.Length > 0)
        {
            for (int i = 0; i < spineChanger.Length; i++)
            {
                spineChanger[i].targetImage.skeletonDataAsset = spineChanger[i].images[PlayerPrefs.GetInt("LangIndex")];
            }
        }
    }
}
