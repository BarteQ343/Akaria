using System.Collections;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
#if ENABLE_PROPERTY_VARIANTS
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
#endif

namespace Phrase
{
  class PhraseEditor : EditorWindow
  {
    [MenuItem("Window/Phrase")]
    public static void ShowWindow()
    {
      var window = GetWindow<PhraseEditor>("Phrase");
      window.Show();
    }

    private LocalizedString LocalizedString(Transform gameObject)
    {
      var localizeStringEvent = gameObject.GetComponent<LocalizeStringEvent>();
      if (localizeStringEvent != null)
      {
        return localizeStringEvent.StringReference;
      }

#if ENABLE_PROPERTY_VARIANTS
      var gameObjectLocalizer = gameObject.GetComponentInParent<GameObjectLocalizer>();
      if (gameObjectLocalizer != null)
      {
        var trackedObject = gameObjectLocalizer.TrackedObjects[0];
        if (trackedObject != null)
        {
          var trackedProperty = trackedObject.TrackedProperties[0];
          if (trackedProperty != null)
          {
            return (trackedProperty as LocalizedStringProperty).LocalizedString;
          }
        }
      }
#endif

      return null;
    }

    private PhraseProvider Provider(SharedTableData sharedTableData)
    {
      var stringTableCollection = PhraseProvider.ConnectedStringTableCollections().FirstOrDefault(x => x.SharedData == sharedTableData);
      return PhraseProvider.FindFor(stringTableCollection);
    }

    private SharedTableData SharedTableData(LocalizedString localizedString)
    {
      if (localizedString == null)
      {
        return null;
      }
      var guid = localizedString.TableReference.TableCollectionNameGuid.ToString("N");
      var path = AssetDatabase.GUIDToAssetPath(guid);
      return AssetDatabase.LoadAssetAtPath<SharedTableData>(path);
    }

    private SharedTableData SharedTableData(Transform gameObject)
    {
      var localizedString = LocalizedString(gameObject);
      return SharedTableData(localizedString);
    }

    private string KeyName(LocalizedString localizedString, SharedTableData sharedTableData = null)
    {
      if (sharedTableData == null) sharedTableData = SharedTableData(localizedString);
      if (localizedString != null && sharedTableData != null)
      {
        return localizedString.TableEntryReference.ResolveKeyName(sharedTableData);
      }

      return null;
    }

    private PhraseMetadata PhraseMetadata(SharedTableData sharedTableData, string keyName)
    {
      if (sharedTableData != null)
      {
        return sharedTableData.GetEntry(keyName).Metadata.GetMetadata<PhraseMetadata>();
      }

      return null;
    }

    private IEnumerator UploadScreenshots(TranslatableObject[] translatableObjects)
    {
      string screenshotPath = "Temp/phrase_screenshot.png";
      System.IO.File.Delete(screenshotPath);
      EditorApplication.ExecuteMenuItem("Window/General/Game"); // screenshot only works in game view
      ScreenCapture.CaptureScreenshot(screenshotPath);

      yield return new WaitForEndOfFrame();


      var groupedObjectsByProvider = translatableObjects
      .GroupBy(x => x.provider)
      .ToDictionary(
        g => g.Key,
        g => g.Select(x => new KeyScreenshotMeta
        {
          name = x.keyName,
          metadata = x.metadata
        }).ToList()
      );

      foreach (var group in groupedObjectsByProvider)
      {
        PhraseProvider provider = group.Key;
        provider.UploadScreenshot(group.Value, screenshotPath);
      }
      System.IO.File.Delete(screenshotPath);

      EditorUtility.DisplayDialog($"Upload Screenshot", $"Screenshot uploaded for {translatableObjects.Length} key(s)", "OK");
    }

    private Vector2 scrollPosition;

    /// <summary>
    /// Hold the game object together with its LocalizedString and PhraseMetadata
    /// </summary>
    private struct TranslatableObject
    {
      public Transform gameObject;
      public LocalizedString localizedString;
      public SharedTableData sharedTableData;
      public PhraseProvider provider;
      public string keyName;
      public PhraseMetadata metadata;
    }

    private TranslatableObject[] translatableObjects;

    public void OnGUI()
    {
      var hasScreenshots = false;

      if (translatableObjects == null || translatableObjects.Length == 0)
      {
        EditorGUILayout.HelpBox("Select a localized GameObject to edit its Phrase metadata.", MessageType.Info);
        return;
      }
      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
      foreach (var translatableObject in translatableObjects)
      {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(translatableObject.gameObject.name, EditorStyles.boldLabel);
        if (translatableObject.metadata.KeyId != null)
        {
          if (GUILayout.Button("Open in Phrase", GUILayout.Width(100)))
          {
            Application.OpenURL(translatableObject.provider.KeyUrl(translatableObject.metadata.KeyId));
          }
        }
        if (!string.IsNullOrEmpty(translatableObject.metadata.ScreenshotUrl))
        {
          if (GUILayout.Button("Open Screenshot", GUILayout.Width(150)))
          {
            Application.OpenURL(translatableObject.metadata.ScreenshotUrl);
          }
        }
        EditorGUILayout.EndHorizontal();
        if (translatableObject.keyName != null)
        {
          EditorGUI.indentLevel++;
          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField("Phrase Key", translatableObject.keyName);
          if (translatableObject.metadata.KeyId != null)
          {
            if (GUILayout.Button("Copy", GUILayout.Width(50)))
            {
              EditorGUIUtility.systemCopyBuffer = translatableObject.keyName;
            }
            if (translatableObject.metadata.ScreenshotId != null)
            {
              hasScreenshots = true;
            }
          }
          EditorGUILayout.EndHorizontal();
          translatableObject.metadata.Description = EditorGUILayout.TextField("Description", translatableObject.metadata.Description);
          translatableObject.metadata.MaxLength = EditorGUILayout.IntField(new GUIContent("Max Length", "set 0 for no limit"), translatableObject.metadata.MaxLength);
          EditorGUI.indentLevel--;
        }
      }

      GUILayout.Space(20);

      var screenshotButtonLabel = hasScreenshots ? "Update Screenshot" : "Upload Screenshot";
      if (GUILayout.Button(screenshotButtonLabel))
      {
        EditorCoroutineUtility.StartCoroutine(UploadScreenshots(translatableObjects), this);
      }

      EditorGUILayout.EndScrollView();
    }

    public void OnSelectionChange()
    {
      // This finds all selected GameObjects and their children that have a LocalizedString component
      translatableObjects = Selection.transforms
        ?.SelectMany(x => x.GetComponentsInChildren<Transform>())
        ?.Select(x => new TranslatableObject
        {
          gameObject = x,
          localizedString = LocalizedString(x),
          sharedTableData = SharedTableData(x)
        })
        ?.Where(x => x.localizedString != null && x.sharedTableData != null)
        ?.Select(x => {
          var provider = Provider(x.sharedTableData);
          var keyName = KeyName(x.localizedString, x.sharedTableData);
          var metadata = PhraseMetadata(x.sharedTableData, KeyName(x.localizedString, x.sharedTableData));
          if (metadata == null)
          {
            metadata = new PhraseMetadata();
            x.sharedTableData.GetEntry(keyName).Metadata.AddMetadata(metadata);
          }

          return new TranslatableObject
          {
            gameObject = x.gameObject,
            localizedString = x.localizedString,
            sharedTableData = x.sharedTableData,
            provider = provider,
            keyName = keyName,
            metadata = metadata
          };
        })
        ?.ToArray();
      Repaint();
    }
  }
}
