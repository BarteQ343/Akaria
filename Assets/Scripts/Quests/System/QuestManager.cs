using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<string> questNames = new();
    public static QuestManager questManager;
    private void Awake()
    {
        if (questManager != null)
        {
            Destroy(gameObject);
        }
        questManager = this;
        DontDestroyOnLoad(gameObject);
    }
}
