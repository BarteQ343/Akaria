using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class DialogueVariables : MonoBehaviour
{
    public TextAsset loadGlobalsJSON; // Assign in Inspector
    public Dictionary<string, Ink.Runtime.Object> variables { get; private set; }

    private Story globalVariablesStory;
    private const string saveVariablesKey = "INK_VARIABLES";

    void Awake()
    {
        if (loadGlobalsJSON == null)
        {
            Debug.LogError("DialogueVariables: No globals JSON assigned!");
            return;
        }

        // create the story
        globalVariablesStory = new Story(loadGlobalsJSON.text);

        // if we have saved data, load it
        if (PlayerPrefs.HasKey(saveVariablesKey))
        {
            string jsonState = PlayerPrefs.GetString(saveVariablesKey);
            globalVariablesStory.state.LoadJson(jsonState);
        }

        // initialize the dictionary
        variables = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
            Debug.Log("Initialized global dialogue variable: " + name + " = " + value);
        }
    }

    public void SaveVariables()
    {
        if (globalVariablesStory != null)
        {
            VariablesToStory(globalVariablesStory);
            PlayerPrefs.SetString(saveVariablesKey, globalVariablesStory.state.ToJson());
        }
    }

    public void StartListening(Story story)
    {
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VariableChanged;
    }

    private void VariableChanged(string name, Ink.Runtime.Object value)
    {
        if (variables.ContainsKey(name))
        {
            variables[name] = value;
        }
    }

    private void VariablesToStory(Story story)
    {
        foreach (var variable in variables)
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }
}
