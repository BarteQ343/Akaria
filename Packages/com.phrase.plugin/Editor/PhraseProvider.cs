// Uncomment the following line when publishing the plugin to the Asset Store
#define PHRASE_VS_ATTRIBUTION

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEngine;

using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine.Localization.Settings;

using Unity.EditorCoroutines.Editor;

using static Phrase.PhraseClient;
#if PHRASE_VS_ATTRIBUTION
using static Phrase.VSAttribution.VSAttribution;
#endif

namespace Phrase
{
    [System.Serializable]
    public enum AuthType
    {
        Token,
        Oauth
    }
    public class KeyScreenshotMeta
    {
        public string name;
        public PhraseMetadata metadata;
    }

    [CreateAssetMenu(fileName = "Phrase", menuName = "Localization/Phrase")]
    public partial class PhraseProvider : ScriptableObject
    {
        public string m_Environment = "EU";

        public string m_ApiUrl = null;

        [System.NonSerialized]
        public bool m_OauthInProgress = false;

        public string m_OauthToken = null;
        public string m_OauthRefreshToken = null;
        public string m_OauthOrganizationId = null;

        public string m_ApiKey;

        public List<Project> Projects = new List<Project>();

        public List<Locale> Locales = new List<Locale>();

        public List<string> LocaleIdsToPull = new List<string>();

        public List<string> LocaleIdsToPush = new List<string>();

        public string m_selectedAccountId = null;

        public string m_selectedProjectId = null;

        public AuthType m_authenticationType = AuthType.Token;

        public bool UseOauth => m_authenticationType == AuthType.Oauth;

        public string Token => UseOauth ? m_OauthToken : m_ApiKey;

        private PhraseClient Client => new PhraseClient(this);

        void OnEnable()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
        }

        private string StringsAppHost
        {
            get
            {
                switch (m_Environment)
                {
                    case "EU":
                        return "https://app.phrase.com";
                    case "US":
                        return "https://us.app.phrase.com";
                    default:
                        return Regex.IsMatch(m_ApiUrl, "localhost:3000") ? "http://localhost:3000" : "https://app.phrase-qa.com";
                }
            }
        }

        public void Log(string message)
        {
            if (m_Environment == "Custom")
            {
                Debug.Log($"[Phrase] {message}");
            }
        }

        public void LogError(string message)
        {
            if (m_Environment == "Custom")
            {
                Debug.LogError($"[Phrase] {message}");
            }
        }

        public void SetOauthToken(string token)
        {
            m_OauthToken = token;
            FetchProjects();
        }

        public async Task<bool> RefreshToken()
        {
            if (UseOauth)
            {
                return await PhraseOauthAuthenticator.RefreshToken(this);
            }
            return false;
        }

        public bool IsProjectSelected => Projects.Count > 0 && m_selectedProjectId != null;

        public bool HasLocaleMismatch => MissingLocalesLocally().Count > 0 || MissingLocalesRemotely().Count > 0;
        public bool IsLoadingProjects { get; private set; } = false;
        public bool IsLoadingLocales { get; private set; } = false;

        public bool IsLoading => IsLoadingProjects || IsLoadingLocales;

        public IEnumerator FetchProjectsAsync()
        {
            IsLoadingProjects = true;
            yield return new WaitForEndOfFrame();

            Client.UpdateProjectsList(Projects);
            EditorUtility.SetDirty(this);
            IsLoadingProjects = false;
        }

        public void FetchProjects()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(FetchProjectsAsync());
        }

        public IEnumerator FetchLocalesAsync()
        {
            if (m_selectedProjectId == null || m_selectedProjectId == "")
            {
                yield break;
            }
            IsLoadingLocales = true;
            yield return new WaitForEndOfFrame();
            Client.UpdateLocalesList(m_selectedProjectId, Locales);
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            LocaleIdsToPull.Clear();
            EditorUtility.SetDirty(this);
            IsLoadingLocales = false;
        }

        public void FetchLocales()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(FetchLocalesAsync());
        }

        public static List<StringTableCollection> AllStringTableCollections()
        {
            return AssetDatabase
                .FindAssets("t:StringTableCollection")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<StringTableCollection>)
                .ToList();
        }

        /// <summary>
        /// Returns the StringTableCollections that are connected to Phrase provider
        /// </summary>
        /// <returns></returns>
        public static List<StringTableCollection> ConnectedStringTableCollections()
        {
            return AllStringTableCollections()
                .Where(collection => collection.Extensions.Any(e => e is PhraseExtension))
                .ToList();
        }

        public static PhraseProvider FindFor(StringTableCollection collection)
        {
            if (collection == null)
            {
                return null;
            }
            var extension = collection.Extensions.FirstOrDefault(e => e is PhraseExtension) as PhraseExtension;
            return extension?.m_provider;
        }

        /// <summary>
        /// Returns the Unity locales that are missing in Phrase
        /// </summary>
        /// <returns></returns>
        public List<UnityEngine.Localization.Locale> MissingLocalesRemotely()
        {
            return LocalizationSettings.AvailableLocales.Locales
                .Where(l => !Locales.Any(pl => pl.code == l.Identifier.Code))
                .ToList();
        }

        /// <summary>
        /// Returns the Unity locales that are existing in Phrase
        /// </summary>
        /// <returns></returns>
        public List<UnityEngine.Localization.Locale> AvailableLocalesRemotely()
        {
            return LocalizationSettings.AvailableLocales.Locales
                .Where(l => Locales.Any(pl => pl.code == l.Identifier.Code))
                .ToList();
        }

        /// <summary>
        /// Returns the Phrase locales that are missing in the Unity project
        /// </summary>
        /// <returns></returns>
        public List<Locale> MissingLocalesLocally()
        {
            return Locales
                .Where(pl => !LocalizationSettings.AvailableLocales.Locales.Any(l => l.Identifier.Code == pl.code))
                .ToList();
        }

        /// <summary>
        /// Returns the Phrase locales that are existing in the Unity project
        /// </summary>
        /// <returns></returns>
        public List<Locale> AvailableLocalesLocally()
        {
            return Locales
                .Where(pl => LocalizationSettings.AvailableLocales.Locales.Any(l => l.Identifier.Code == pl.code))
                .ToList();
        }

        public void CreatePhraseLocale(UnityEngine.Localization.Locale locale)
        {
            Client.CreateLocale(m_selectedProjectId, locale.Identifier.Code, locale.Identifier.Code);
        }

        public IEnumerator PushAll(List<StringTableCollection> collections)
        {
            int collectionsIndex = 1;
            int collectionsTotal = collections.Count;
            foreach (StringTableCollection collection in collections)
            {
                var phraseExtension = collection.Extensions.FirstOrDefault(e => e is PhraseExtension) as PhraseExtension;
                if (phraseExtension != null)
                {
                    List<Locale> locales = Locales;
                    if (LocaleIdsToPush != null)
                    {
                        locales = Locales.Where(l => LocaleIdsToPush.Contains(l.code)).ToList();
                    }
                    int localesIndex = 1;
                    int localesTotal = locales.Count;
                    foreach (var locale in locales)
                    {
                        float progress = (float)(collectionsIndex - 1) / collectionsTotal + (float)(localesIndex - 1) / localesTotal / collectionsTotal;
                        string info = $"Pushing from table {collection.name} ({collectionsIndex}/{collectionsTotal}), locale {locale.name} ({localesIndex}/{localesTotal})";
                        EditorUtility.DisplayProgressBar("Pushing Translations", info, progress);
                        yield return new WaitForEndOfFrame();

                        var matchingStringTable = collection.StringTables.FirstOrDefault(st => st.LocaleIdentifier.Code == locale.code);
                        if (matchingStringTable == null)
                        {
                            break;
                        }
                        const string dir = "Temp/";
                        string path = dir + matchingStringTable.name + ".csv";
                        string tag = phraseExtension.m_identifierType == TableIdentifierType.Tag ? phraseExtension.m_identifier : null;
                        using (var stream = new StreamWriter(path, false, new UTF8Encoding(false)))
                        {
                            string keyPrefix = phraseExtension.m_identifierType == TableIdentifierType.KeyPrefix ? phraseExtension.m_identifier : null;
                            Csv.Export(stream, collection, ColumnMappings(locale, keyPrefix));
                        }
                        Client.UploadFile(path, m_selectedProjectId, locale.id, locale.name, false, tag);
                        if (File.Exists(path)) File.Delete(path);
                    }
                }
                collectionsIndex++;
            }
            EditorUtility.ClearProgressBar();
        }

        public void PullAll()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(Pull(ConnectedStringTableCollections()));
        }

        private List<CsvColumns> ColumnMappings(Locale locale, string keyPrefix)
        {
            return new List<CsvColumns>
            {
                new PhraseCsvColumns {
                    KeyPrefix = keyPrefix
                },
                new LocaleColumns {
                    LocaleIdentifier = locale.code,
                    FieldName = locale.name
                },
            };
        }

        private async void PullSingleLocale(StringTableCollection collection, Locale selectedLocale)
        {
            Log("Downloading locale " + selectedLocale.code);
            var phraseExtension = collection.Extensions.FirstOrDefault(e => e is PhraseExtension) as PhraseExtension;
            string tag = phraseExtension.m_identifierType == TableIdentifierType.Tag ? phraseExtension.m_identifier : null;
            string keyPrefix = phraseExtension.m_identifierType == TableIdentifierType.KeyPrefix ? phraseExtension.m_identifier : null;
            var csvContent = await Client.DownloadLocale(m_selectedProjectId, selectedLocale.id, tag, keyPrefix);
            using (var reader = new StringReader(csvContent))
            {
                var columnMappings = ColumnMappings(selectedLocale, keyPrefix);
                Csv.ImportInto(reader, collection, columnMappings);
            }
        }

        public IEnumerator Pull(List<StringTableCollection> collections)
        {
            int collectionsIndex = 1;
            int collectionsTotal = collections.Count;
            AssetDatabase.StartAssetEditing();
            foreach (var collection in collections)
            {
                int localesIndex = 1;
                int localesTotal = collection.StringTables.Count;

                foreach (var stringTable in collection.StringTables)
                {
                    float progress = (float)(collectionsIndex - 1) / collectionsTotal + (float)(localesIndex - 1) / localesTotal / collectionsTotal;
                    string info = $"Table {collection.name} ({collectionsIndex}/{collectionsTotal}), locale {stringTable.LocaleIdentifier.Code} ({localesIndex}/{localesTotal})";
                    Log($"Progress: {progress}, {info}");
                    EditorUtility.DisplayProgressBar("Pulling Translations", info, progress);
                    yield return new WaitForEndOfFrame();
                    // Find the locale
                    var selectedLocale = Locales.FirstOrDefault(l => l.code == stringTable.LocaleIdentifier.Code);
                    if (selectedLocale != null)
                    {
                        if (!LocaleIdsToPull.Contains(selectedLocale.id))
                        {
                            continue;
                        }

                        PullSingleLocale(collection, selectedLocale);
                    }
                    else
                    {
                        Log("No Phrase locale found for string table " + stringTable.LocaleIdentifier.Code);
                    }
                    localesIndex++;
                }
                collectionsIndex++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }

        public async void UploadScreenshot(List<KeyScreenshotMeta> keyMetadataPairs, string path)
        {
            var metadataWithNoKeyId = keyMetadataPairs.Where(m => string.IsNullOrEmpty(m.metadata.KeyId)).ToList();
            if (metadataWithNoKeyId.Count > 0)
            {
                Debug.Log("Screenshot upload: KeyId is not present for at least one of the selected keys, fetching metadata from Phrase.");
                var phraseKeys = await Client.GetKeysByName(m_selectedProjectId, metadataWithNoKeyId.Select(n => n.name).ToList());
                foreach (var metadata in metadataWithNoKeyId)
                {
                    var matchingKey = phraseKeys.FirstOrDefault(k => k.name == metadata.name);
                    if (matchingKey != null)
                    {
                        metadata.metadata.KeyId = matchingKey.id;
                    }
                }
            }

            var metadataList = keyMetadataPairs.Select(m => m.metadata).ToList();
            string name = "screenshot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            Screenshot screenshot = await Client.UploadScreenshot(m_selectedProjectId, name, path);

            if (screenshot != null)
            {
                foreach (var metadata in metadataList)
                {
                    if (metadata.ScreenshotMarkerId != null)
                    {
                        Client.DeleteScreenshotMarker(m_selectedProjectId, metadata.ScreenshotId, metadata.ScreenshotMarkerId);
                    }


                    if (metadata.KeyId == "" || metadata.KeyId == null)
                    {
                        continue;
                    }

                    ScreenshotMarker marker = await Client.CreateScreenshotMarker(m_selectedProjectId, screenshot.id, metadata.KeyId);

                    if (marker != null)
                    {
                        metadata.ScreenshotMarkerId = marker.id;
                    }

                    metadata.ScreenshotId = screenshot.id;
                    metadata.ScreenshotUrl = screenshot.screenshot_url;
                }
            }
        }

        public string KeyUrl(string keyId)
        {
            return $"{StringsAppHost}/editor/v4/accounts/{m_selectedAccountId}/projects/{m_selectedProjectId}?keyId={keyId}";
        }
    }

    [CustomEditor(typeof(PhraseProvider))]
    public class PhraseProviderEditor : Editor
    {
        bool m_showTables = false;

        bool m_showConnection = false;

        bool m_showLocalesMissing = false;
        bool m_showLocalLocalesMissing = false;
        bool m_showRemoteLocalesMissing = false;

        bool m_showLocalesToPull = false;
        bool m_showLocalesToPush = false;

        static readonly string[] m_environmentOptions = { "EU", "US", "Custom" };

        bool selectAllLocalesToCreateLocally = false;
        bool selectAllLocalesToCreateRemotely = false;

        bool selectedAllLocalesToPull = false;
        bool selectedAllLocalesToPush = false;

        List<string> selectedLocalesToCreateLocally = new List<string>();
        List<string> selectedLocalesToCreateRemotely = new List<string>();

        private PhraseProvider phraseProvider => target as PhraseProvider;

        private string searchQuery = "";
        private string localeSearchQuery = string.Empty;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (phraseProvider.IsLoading)
            {
                EditorGUILayout.HelpBox(GetLoadingMessage(), MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(phraseProvider.IsLoading);
            ShowConnectionSection();
            ShowProjectSection();
            ShowLocaleMismatchSection();
            ShowConnectedTablesSection();
            ShowPushPullSection();

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private string TruncateWithEllipsis(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input;
            }
            return input.Substring(0, maxLength) + "...";
        }

        private void ShowConnectionSection()
        {
            m_showConnection = EditorGUILayout.BeginFoldoutHeaderGroup(m_showConnection, "Phrase Connection");

            if (m_showConnection)
            {
                phraseProvider.m_Environment = m_environmentOptions[EditorGUILayout.Popup("Environment", System.Array.IndexOf(m_environmentOptions, phraseProvider.m_Environment), m_environmentOptions)];
                if (phraseProvider.m_Environment == "Custom")
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ApiUrl"));
                }
                else
                {
                    switch (phraseProvider.m_Environment)
                    {
                        case "EU":
                            phraseProvider.m_ApiUrl = "https://api.phrase.com/v2/";
                            break;
                        case "US":
                            phraseProvider.m_ApiUrl = "https://api.us.app.phrase.com/v2/";
                            break;
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_authenticationType"));
                EditorGUILayout.BeginHorizontal();

                if (phraseProvider.UseOauth)
                {
                    string buttonLabel = phraseProvider.m_OauthInProgress ? "Logging in..." : "Log in using OAuth";
                    if (GUILayout.Button(buttonLabel))
                    {
                        PhraseOauthAuthenticator.Authenticate(phraseProvider);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ApiKey"));
                }

                if (GUILayout.Button("Fetch Projects", GUILayout.Width(95)))
                {
                    phraseProvider.FetchProjects();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ShowProjectSection()
        {
            var allProjects = phraseProvider.Projects;
            var filteredProjects = allProjects.ToArray();

            // Only show Project-Filter if there are more than 10 projects
            if (allProjects.Count > 10)
            {
                EditorGUILayout.BeginHorizontal();
                searchQuery = EditorGUILayout.TextField("Filter by Projectname", searchQuery);
                // Filter projects based on the search query
                filteredProjects = allProjects
                    .Where(p => string.IsNullOrEmpty(searchQuery) || p.name.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();
                EditorGUILayout.LabelField($"{filteredProjects.Length}/{phraseProvider.Projects.Count}", GUILayout.Width(65));
                EditorGUILayout.EndHorizontal();
            }

            filteredProjects = filteredProjects.OrderBy(p => p.name).ToArray();

            // Truncate project names more aggressively to control popup width
            string[] projectNames = filteredProjects
                .Select(p => TruncateWithEllipsis(p.name, 30))
                .ToArray();

            int selectedProjectIndex = System.Array.IndexOf(filteredProjects, phraseProvider.Projects.FirstOrDefault(p => p.id == phraseProvider.m_selectedProjectId));

            // Ensure valid index
            if (selectedProjectIndex == -1 && filteredProjects.Length > 0)
            {
                selectedProjectIndex = 0; // Default to first project if no selection matches
            }

            selectedProjectIndex = EditorGUILayout.Popup("Select Project", selectedProjectIndex, projectNames);

            if (selectedProjectIndex >= 0 && selectedProjectIndex < filteredProjects.Length)
            {
                var selectedProject = filteredProjects[selectedProjectIndex];
                if (phraseProvider.m_selectedProjectId != selectedProject.id)
                {
                    phraseProvider.m_selectedProjectId = selectedProject.id;
                    phraseProvider.m_selectedAccountId = selectedProject.account.id;
                    phraseProvider.FetchLocales();
                    #if PHRASE_VS_ATTRIBUTION
                    SendAttributionEvent("ProjectAccess", "Phrase", phraseProvider.m_selectedAccountId);
                    #endif
                }
            }
        }

        private void ShowLocaleMismatchSection()
        {
            if (phraseProvider.IsProjectSelected && phraseProvider.HasLocaleMismatch)
            {
                m_showLocalesMissing = EditorGUILayout.BeginFoldoutHeaderGroup(m_showLocalesMissing, "Missing locales");
                if (m_showLocalesMissing)
                {
                    ShowLocalLocaleMissingSection();
                    ShowRemoteLocaleMissingSection();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void ShowLocalLocaleMissingSection()
        {
            if (phraseProvider.MissingLocalesLocally().Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Missing in Unity");
                if(GUILayout.Button($"{selectedLocalesToCreateLocally.Count}/{phraseProvider.MissingLocalesLocally().Count} selected", GUILayout.Width(150)))
                {
                    m_showLocalLocalesMissing = !m_showLocalLocalesMissing;
                }
                using (new EditorGUI.DisabledScope(selectedLocalesToCreateLocally.Count == 0))
                {
                    if (GUILayout.Button("Create locally", GUILayout.Width(150)))
                    {
                        string pathToSave = EditorUtility.OpenFolderPanel("Save new locales", "Assets", "");
                        if (string.IsNullOrEmpty(pathToSave))
                        {
                            return;
                        }
                        if (!pathToSave.StartsWith(Application.dataPath))
                        {
                            Debug.LogError("Path must be in the Assets folder");
                            return;
                        }
                        pathToSave = pathToSave.Substring(Application.dataPath.Length - "Assets".Length);
                        int count = 0;
                        foreach (var locale in phraseProvider.MissingLocalesLocally())
                        {
                            if (!selectedLocalesToCreateLocally.Contains(locale.code))
                            {
                                continue;
                            }
                            UnityEngine.Localization.Locale newLocale = UnityEngine.Localization.Locale.CreateLocale(locale.code);
                            AssetDatabase.CreateAsset(newLocale, $"{pathToSave}/{newLocale}.asset");
                            count++;
                        }
                        LocalizationSettings.InitializationOperation.WaitForCompletion();
                        EditorUtility.DisplayDialog("Locales created", $"{count} language(s) created and saved to {pathToSave}.", "OK");
                    }
                }
                EditorGUILayout.EndHorizontal();
                if(m_showLocalLocalesMissing)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.HelpBox("The following Phrase locales are missing in the project:", MessageType.None);
                    if (EditorGUILayout.ToggleLeft("Select all", selectAllLocalesToCreateLocally, EditorStyles.boldLabel))
                    {
                        if (!selectAllLocalesToCreateLocally)
                        {
                            selectedLocalesToCreateLocally = phraseProvider.MissingLocalesLocally().Select(l => l.code).ToList();
                            selectAllLocalesToCreateLocally = true;
                        }
                    }
                    else
                    {
                        if (selectAllLocalesToCreateLocally)
                        {
                            selectedLocalesToCreateLocally.Clear();
                            selectAllLocalesToCreateLocally = false;
                        }
                    }
                    foreach (var locale in phraseProvider.MissingLocalesLocally())
                    {
                        if (EditorGUILayout.ToggleLeft(locale.ToString(), selectedLocalesToCreateLocally.Contains(locale.code)))
                        {
                            if (!selectedLocalesToCreateLocally.Contains(locale.code))
                            {
                                selectedLocalesToCreateLocally.Add(locale.code);
                            }
                        }
                        else
                        {
                            selectedLocalesToCreateLocally.Remove(locale.code);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }

        private void ShowRemoteLocaleMissingSection()
        {
            if (phraseProvider.MissingLocalesRemotely().Count > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Missing in Phrase");
                if(GUILayout.Button($"{selectedLocalesToCreateRemotely.Count}/{phraseProvider.MissingLocalesRemotely().Count} selected", GUILayout.Width(150)))
                {
                    m_showRemoteLocalesMissing = !m_showRemoteLocalesMissing;
                }
                using (new EditorGUI.DisabledScope(selectedLocalesToCreateRemotely.Count == 0))
                {
                    if (GUILayout.Button("Create in Phrase", GUILayout.Width(150)))
                    {
                        int count = 0;
                        foreach (var locale in phraseProvider.MissingLocalesRemotely())
                        {
                            if (!selectedLocalesToCreateRemotely.Contains(locale.Identifier.Code))
                            {
                                continue;
                            }
                            phraseProvider.CreatePhraseLocale(locale);
                            count++;
                        }
                        phraseProvider.FetchLocales();
                        EditorUtility.DisplayDialog("Locales created", $"{count} locales(s) created in Phrase.", "OK");
                    }
                }
                EditorGUILayout.EndHorizontal();
                if(m_showRemoteLocalesMissing)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.HelpBox("The following locales are missing in Phrase Strings:", MessageType.None);
                    if (EditorGUILayout.ToggleLeft("Select all", selectAllLocalesToCreateRemotely, EditorStyles.boldLabel))
                    {
                        if (!selectAllLocalesToCreateRemotely)
                        {
                            selectedLocalesToCreateRemotely = phraseProvider.MissingLocalesRemotely().Select(l => l.Identifier.Code).ToList();
                            selectAllLocalesToCreateRemotely = true;
                        }
                    }
                    else
                    {
                        if (selectAllLocalesToCreateRemotely)
                        {
                            selectedLocalesToCreateRemotely.Clear();
                            selectAllLocalesToCreateRemotely = false;
                        }
                    }
                    foreach (var locale in phraseProvider.MissingLocalesRemotely())
                    {
                        if (EditorGUILayout.ToggleLeft(locale.ToString(), selectedLocalesToCreateRemotely.Contains(locale.Identifier.Code)))
                        {
                            if (!selectedLocalesToCreateRemotely.Contains(locale.Identifier.Code))
                            {
                                selectedLocalesToCreateRemotely.Add(locale.Identifier.Code);
                            }
                        }
                        else
                        {
                            selectedLocalesToCreateRemotely.Remove(locale.Identifier.Code);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

        }

        private void ShowConnectedTablesSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("String tables");
            List<StringTableCollection> allCollections = PhraseProvider.AllStringTableCollections();
            int connectedCollections = PhraseProvider.ConnectedStringTableCollections().Count;
            if(GUILayout.Button($"{connectedCollections}/{allCollections.Count} connected", GUILayout.MaxWidth(303)))
            {
                m_showTables = !m_showTables;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if (m_showTables)
            {
                foreach (StringTableCollection collection in allCollections)
                {
                    var extension = collection.Extensions.FirstOrDefault(e => e is PhraseExtension) as PhraseExtension;
                    bool selectedState = extension != null && extension.m_provider != null;
                    EditorGUILayout.BeginHorizontal();
                    bool newSelectedState = EditorGUILayout.ToggleLeft(collection.name, selectedState);
                    if (GUILayout.Button("Go to", GUILayout.Width(50)))
                    {
                        Selection.activeObject = collection;
                    }
                    if (newSelectedState != selectedState)
                    {
                        TogglePhraseExtension(collection, newSelectedState);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }

        private void ShowPushPullSection()
        {
            using (new EditorGUI.DisabledScope(!phraseProvider.IsProjectSelected))
            {
                ShowPushSection();
                ShowPullSection();
            }
        }

        private void ShowPushSection()
        {
            // Get the list of available locales
            var allLocales = phraseProvider.AvailableLocalesRemotely();

            // Determine if the search field should be displayed
            if (allLocales.Count > 10)
            {
                localeSearchQuery = EditorGUILayout.TextField("Filter by Locale", localeSearchQuery);
            }
            else
            {
                localeSearchQuery = string.Empty;
            }

            var filteredLocales = allLocales
                .Where(l => string.IsNullOrEmpty(localeSearchQuery) || l.Identifier.Code.IndexOf(localeSearchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();

            string[] availableLocaleNames = filteredLocales.Select(l => l.Identifier.Code).ToArray();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Push Languages");

            if(GUILayout.Button($"{phraseProvider.LocaleIdsToPush.Count}/{filteredLocales.Length} selected", GUILayout.Width(150)))
            {
                m_showLocalesToPush = !m_showLocalesToPush;
            }

            if (GUILayout.Button("Push to Phrase", GUILayout.Width(150)))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(phraseProvider.PushAll(PhraseProvider.ConnectedStringTableCollections()));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if(m_showLocalesToPush)
            {
                if (EditorGUILayout.ToggleLeft("Select all", selectedAllLocalesToPush, EditorStyles.boldLabel))
                {
                    if (!selectedAllLocalesToPush)
                    {
                        phraseProvider.LocaleIdsToPush.Clear();
                        phraseProvider.LocaleIdsToPush.AddRange(filteredLocales.Select(l => l.Identifier.Code));
                        selectedAllLocalesToPush = true;
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    if (selectedAllLocalesToPush)
                    {
                        phraseProvider.LocaleIdsToPush.Clear();
                        selectedAllLocalesToPush = false;
                        EditorUtility.SetDirty(target);
                    }
                }

                foreach (var locale in filteredLocales)
                {
                    bool selectedState = phraseProvider.LocaleIdsToPush.Contains(locale.Identifier.Code);
                    bool newSelectedState = EditorGUILayout.ToggleLeft(locale.ToString(), selectedState);

                    if (newSelectedState != selectedState)
                    {
                        if (newSelectedState)
                        {
                            phraseProvider.LocaleIdsToPush.Add(locale.Identifier.Code);
                        }
                        else
                        {
                            selectedAllLocalesToPush = false;
                            phraseProvider.LocaleIdsToPush.Remove(locale.Identifier.Code);
                        }
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        private void ShowPullSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pull Languages");
            if (GUILayout.Button($"{phraseProvider.LocaleIdsToPull.Count}/{phraseProvider.AvailableLocalesLocally().Count} selected", GUILayout.Width(150)))
            {
                m_showLocalesToPull = !m_showLocalesToPull;
            }

            if (GUILayout.Button("Pull to Unity", GUILayout.Width(150)))
            {
                phraseProvider.PullAll();
            }
            EditorGUILayout.EndHorizontal();

            if (m_showLocalesToPull)
            {
                if (EditorGUILayout.ToggleLeft("Select all", selectedAllLocalesToPull, EditorStyles.boldLabel))
                {
                    if (!selectedAllLocalesToPull)
                    {
                        phraseProvider.LocaleIdsToPull.Clear();
                        phraseProvider.LocaleIdsToPull.AddRange(phraseProvider.AvailableLocalesLocally().Select(l => l.id));
                        selectedAllLocalesToPull = true;
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    if (selectedAllLocalesToPull)
                    {
                        phraseProvider.LocaleIdsToPull.Clear();
                        selectedAllLocalesToPull = false;
                        EditorUtility.SetDirty(target);
                    }
                }

                foreach (var locale in phraseProvider.AvailableLocalesLocally())
                {
                    bool selectedState = phraseProvider.LocaleIdsToPull.Contains(locale.id);
                    bool newSelectedState = EditorGUILayout.ToggleLeft(locale.ToString(), selectedState);
                    if (newSelectedState != selectedState)
                    {
                        if (newSelectedState)
                        {
                            phraseProvider.LocaleIdsToPull.Add(locale.id);
                        }
                        else
                        {
                            selectedAllLocalesToPull = false;
                            phraseProvider.LocaleIdsToPull.Remove(locale.id);
                        }
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUI.indentLevel--;
            }

        }

        /// <summary>
        /// Attaches or detaches the PhraseExtension to the StringTableCollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="selectedState"></param>
        private void TogglePhraseExtension(StringTableCollection collection, bool selectedState)
        {
            PhraseExtension extension = collection.Extensions.FirstOrDefault(e => e is PhraseExtension) as PhraseExtension;
            if (selectedState)
            {
                if (extension == null)
                {
                    extension = new PhraseExtension();
                    collection.AddExtension(extension);
                }
                extension.m_provider = target as PhraseProvider;
            }
            else
            {
                if (extension != null)
                {
                    collection.RemoveExtension(extension);
                }
            }
            EditorUtility.SetDirty(collection);
        }

        private string GetLoadingMessage()
        {
            if (phraseProvider.IsLoadingProjects)
            {
                return "Loading projects...";
            }
            if (phraseProvider.IsLoadingLocales)
            {
                return "Loading locales...";
            }
            return "";
        }
    }
}
