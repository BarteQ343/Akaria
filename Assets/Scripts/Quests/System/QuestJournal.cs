using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;


public class QuestJournal : MonoBehaviour
{
    public QuestDatabase questDatabase; // ScriptableObject containing all QuestData
    public DialogueVariables dialogueVariables;

    public GameObject journalUI;
    public GameObject questListContainer;
    public GameObject questEntryPrefab;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questGoalsText;
    public TextMeshProUGUI questRewardsText;

    private Dictionary<string, int> previousQuestStatuses = new Dictionary<string, int>();

    void Start()
    {
        dialogueVariables = FindObjectOfType<DialogueVariables>();

        if (dialogueVariables == null)
        {
            Debug.LogError("DialogueVariables not found in scene.");
            return;
        }

        StartCoroutine(WaitForVariables());
    }

    private System.Collections.IEnumerator WaitForVariables()
    {
        // Wait until DialogueVariables has initialized the dictionary
        while (dialogueVariables.variables == null)
            yield return null;

        Debug.Log("QuestJournal checked the variables, here they are:");
        foreach (var kvp in dialogueVariables.variables)
        {
            Debug.Log($"{kvp.Key} = {kvp.Value}");
        }

    foreach (var quest in questDatabase.quests)
    {
        previousQuestStatuses[quest.questID] = GetQuestStatus(quest.questID);
    }
    }  

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }

        UpdateQuestStatuses();
    }

    void ToggleJournal()
    {
        journalUI.SetActive(!journalUI.activeSelf);
        if (journalUI.activeSelf)
        {
            RefreshQuestList();
        }
    }

    void RefreshQuestList()
    {
        foreach (Transform child in questListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in questDatabase.quests)
        {
            int status = GetQuestStatus(quest.questID);
            if (quest.visibleWhenStatuses.Contains(status))
            {
                GameObject entry = Instantiate(questEntryPrefab, questListContainer.transform);
                entry.GetComponentInChildren<TextMeshProUGUI>().text = quest.displayName;

                var button = entry.GetComponent<Button>();
                button.onClick.AddListener(() => DisplayQuestDetails(quest));
            }
        }
    }

    void DisplayQuestDetails(QuestData quest)
    {
        questNameText.text = quest.displayName;
        questDescriptionText.text = quest.description;
        questGoalsText.text = string.Join("\n", quest.goals);
        questRewardsText.text = string.Join(", ", quest.rewards);
    }

    int GetQuestStatus(string questID)
    {
    if (dialogueVariables.variables.TryGetValue(questID, out var value))
    {
        if (value is IntValue intVal)
        {
            return intVal.value;
        }
    }

    Debug.LogWarning($"Quest variable '{questID}' not found or not an int.");
    return 0;
    }

    void UpdateQuestStatuses() 
    {
        foreach (var quest in questDatabase.quests)
        {
            int currentStatus = GetQuestStatus(quest.questID);
            if (previousQuestStatuses[quest.questID] != currentStatus)
            {
                if (quest.visibleWhenStatuses.Contains(currentStatus))
                    ShowPopup("Quest updated: " + quest.displayName);
                else if (quest.completedWhenStatuses.Contains(currentStatus))
                    ShowPopup("Quest completed: " + quest.displayName);

                previousQuestStatuses[quest.questID] = currentStatus;
            }
        }
    }

    void ShowPopup(string message)
    {
        Debug.Log("POPUP: " + message);
        // Replace with actual UI popup implementation
    }
}

// ScriptableObject holding a list of quests
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "RPG/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestData> quests;
}

[System.Serializable]
public class QuestData
{
    public string questID; // Matches Ink global variable
    public string displayName;
    [TextArea] public string description;
    public string[] goals;
    public string[] rewards;
    public List<int> visibleWhenStatuses; // When to show in journal
    public List<int> completedWhenStatuses; // When to mark as complete
}
