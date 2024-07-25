using UnityEngine;

public class ForestTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // Start the forest investigation dialogue
            var dialogueController = DialogueController.GetInstance();
            dialogueController.EnterDialogueMode(Resources.Load<TextAsset>("Ink/forest_investigation"));

            // Change the quest_completed variable to true
            var story = dialogueController.currentStory;
            if (story != null)
            {
                story.variablesState["quest_completed"] = true;
                Debug.Log("quest_completed variable set to: " + story.variablesState["quest_completed"]);
            }

            // Disable the trigger
            gameObject.SetActive(false);
        }
    }
}
