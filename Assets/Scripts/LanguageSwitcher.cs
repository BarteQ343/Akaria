using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public void SwitchLang(string lang)
    {
        if (lang == "polish")
        {
            OneClickLocalization.OCL.SetLanguage(SystemLanguage.Polish);
        }
        if (lang == "english")
        {
            OneClickLocalization.OCL.SetLanguage(SystemLanguage.English);
        }
        if (lang == "japanese")
        {
            OneClickLocalization.OCL.SetLanguage(SystemLanguage.Japanese);
        }
    }
}
