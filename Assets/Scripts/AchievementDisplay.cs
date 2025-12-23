using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementDisplay : MonoBehaviour
{
    [SerializeField] private Transform parent = null;
    [SerializeField] private GameObject achievementPrefab = null;
    
    [SerializeField] private Sprite lockedSprite = null;
    [SerializeField] private Sprite hiddenSprite = null;

    [SerializeField] private string lockedText = "Locked";
    [SerializeField] private string hiddenText = "Hidden";
    
    private List<GameObject> achievementsGameObjects = new ();
    
    private void OnEnable()
    {
        foreach (AchievementScriptableData achievement in AchievementManager.GetAchievements())
        {
            GameObject _newPrefab = Instantiate(achievementPrefab, parent);
            Image _imageComponent = _newPrefab.GetComponentInChildren<Image>();
            achievementsGameObjects.Add(_newPrefab);
            
            if (achievement.IsUnlocked())
            {
                _imageComponent.sprite = achievement.GetIcon();
                _newPrefab.GetComponentInChildren<TextMeshProUGUI>().text = achievement.GetDisplayName();
            }
            else
            {
                if (achievement.IsHidden())
                {
                    _imageComponent.sprite = hiddenSprite;
                    _newPrefab.GetComponentInChildren<TextMeshProUGUI>().text = hiddenText;
                }
                else
                {
                    _imageComponent.sprite = lockedSprite;
                    _newPrefab.GetComponentInChildren<TextMeshProUGUI>().text = lockedText;
                }
            }
        }
    }

    private void OnDisable()
    {
        foreach (GameObject _achievementsGO in achievementsGameObjects)
        {
            Destroy(_achievementsGO);
        }
        
        achievementsGameObjects.Clear();
    }
}