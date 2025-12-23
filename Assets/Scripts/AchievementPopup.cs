using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private Image icon = null;
    [SerializeField] private GameObject popupGameObject = null;

    private void OnEnable()
    {
        AchievementManager.OnAchievementUnlocked += onAchievemenetUnlocked;
    }

    private void OnDisable()
    {
        AchievementManager.OnAchievementUnlocked -= onAchievemenetUnlocked;
    }

    private void onAchievemenetUnlocked(AchievementScriptableData _achievement) =>
        StartCoroutine(ShowPopup(_achievement));

    IEnumerator ShowPopup(AchievementScriptableData _achievement)
    {
        if (popupGameObject == null) yield break;

        nameText.text = _achievement.GetDisplayName();
        icon.sprite = _achievement.GetIcon();
        popupGameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        popupGameObject.SetActive(false);
    }
}