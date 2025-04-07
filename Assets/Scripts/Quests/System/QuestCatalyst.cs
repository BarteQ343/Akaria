using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestCatalyst : MonoBehaviour
{
  [SerializeField] private string quest;
  [SerializeField] private GameObject notification;
  private bool questAdded = false;

  public void CreateQuest()
  {
    if (quest != null && !questAdded)
    {
        questAdded = !questAdded;
        QuestManager.questManager.questNames.Add(quest);
    }

    if (notification != null && !questAdded)
    {
        notification.SetActive(true);
    }
  }

  public void CompleteQuest()
  {
    if (quest != null && QuestManager.questManager.questNames.Contains(quest))
    {
        QuestManager.questManager.questNames.Remove(quest);
    }
  }
}
