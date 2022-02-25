using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LanguageParse;

public class LanguageChanger : MonoBehaviour
{
    public string textKey;
    public string[] dropdownKey;
    // Start is called before the first frame update
    void Start()
    {
        LocalizeChanged();
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

    void LocalizeChanged()
    {
        if(GetComponent<Text>().text != null)
        {
            GetComponent<Text>().text = Localize(textKey);
        }
        else if(GetComponent<Dropdown>() != null)
        {
            Dropdown dropdown = GetComponent<Dropdown>();
            dropdown.captionText.text = Localize(dropdownKey[dropdown.value]);

            for(int i = 0; i <dropdown.options.Count; i++)
            {
                dropdown.options[i].text = Localize(dropdownKey[i]);
            }
        }
    }
}
