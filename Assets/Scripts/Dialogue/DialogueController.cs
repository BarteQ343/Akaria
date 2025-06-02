using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    [Header("Player objects")]
    [SerializeField] private PlayerController _controller;
    [SerializeField] Animator anim;
    [SerializeField] private Rigidbody2D r2d;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;

    [Header("Globals Ink File")]
    [SerializeField] private TextAsset loadGlobalsFile; 
    [Header("Dialogue Scripts")]
    [SerializeField] private DialogueVariables dialogueVariables;

    private TextMeshProUGUI[] choicesText;
    public Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private static DialogueController instance;
    private EventSystem eventSystem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
            Destroy(this.gameObject);
            return;
        }
    
            if (dialogueVariables == null)
        {
            dialogueVariables = FindObjectOfType<DialogueVariables>();
            if (dialogueVariables == null)
            {
                Debug.LogError("DialogueVariables not found in scene!");
            }
        }
    }

    public static DialogueController GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }

        eventSystem = EventSystem.current;
    }

    private void Update()
    {
        bool isGrounded = _controller.checkIsGrounded();
        if (!dialogueIsPlaying)
        {
            return;
        }
        else if (isGrounded)
        {
            r2d.constraints = RigidbodyConstraints2D.FreezeAll; 
            anim.ResetTrigger("Fall");
            anim.ResetTrigger("Run");
            anim.ResetTrigger("Jump");
            anim.ResetTrigger("Stop");
            anim.SetTrigger("Stop");
        }

        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.E)) || (Input.GetKeyDown(KeyCode.Return)))
        {
            Debug.Log($"Key pressed: {Input.inputString}, Time: {Time.time}");
            ContinueStory();
            Debug.Log("Story Continued");
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        r2d.constraints = RigidbodyConstraints2D.FreezePositionX;
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        dialogueVariables.StartListening(currentStory);

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        Debug.Log("ExitDialogueMode called");
        yield return new WaitForSeconds(0.01f);
        r2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
        
        if (currentStory != null && dialogueVariables != null)
        {
            dialogueVariables.StopListening(currentStory);
        }

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        speakerText.text = "";
        Debug.Log("Exit Dialogue Mode");
    }

    private void ContinueStory()
    {
         Debug.Log($"ContinueStory called, Time: {Time.time}, canContinue: {currentStory.canContinue}");
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            List<string> tags = currentStory.currentTags;
            ParseTags(tags);
            Debug.Log($"After continue, canContinue: {currentStory.canContinue}");
            DisplayChoices();
        }
        else 
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void ParseTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            if (tag.StartsWith("speaker:"))
            {
                string speakerName = tag.Substring("speaker:".Length).Trim();
                speakerText.text = speakerName;
                Debug.Log($"Speaker: {speakerName}");
            }
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        Debug.Log("Choices Displayed");

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given that the UI can support. Number of choices given: " 
            + currentChoices.Count);
        }

        int index = 0;
        foreach(Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
            Debug.Log(choice.text);
        }
        
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        if(currentChoices.Count > 0)
        {
            eventSystem.SetSelectedGameObject(choices[0].gameObject);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        Debug.Log("Choice made");
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue == null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }
}
