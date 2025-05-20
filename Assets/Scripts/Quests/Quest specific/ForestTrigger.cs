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
            dialogueController.EnterDialogueMode(Resources.Load<TextAsset>("Dialogue/Dialogues/forest_investigation"));

            // Disable the trigger
            gameObject.SetActive(false);
        }
    }
}
