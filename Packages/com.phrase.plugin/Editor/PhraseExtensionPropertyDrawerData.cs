using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Phrase
{
    [CustomPropertyDrawer(typeof(PhraseExtension))]
    class TablePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty m_provider;

        private SerializedProperty m_identifier;

        private SerializedProperty m_identifierType;

        private PhraseProvider provider => m_provider.objectReferenceValue as PhraseProvider;

        private Texture icon;

        private float height = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var extension = property.GetActualObjectForSerializedProperty<PhraseExtension>(fieldInfo);
            var collection = extension.TargetCollection as StringTableCollection;

            EditorGUI.BeginProperty(position, label, property);
            position.yMin += EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            icon = EditorGUIUtility.FindTexture("Packages/com.phrase.plugin/Editor/Icons/phrase.png");

            EditorGUI.LabelField(position, EditorGUIUtility.TrTextContent("Phrase", icon), EditorStyles.boldLabel);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            m_identifier = property.FindPropertyRelative("m_identifier");
            m_identifierType = property.FindPropertyRelative("m_identifierType");
            m_provider = property.FindPropertyRelative("m_provider");
            EditorGUI.PropertyField(position, m_provider);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, m_identifierType, new GUIContent("Only keys matching", "Optionally, pull only keys matching the given prefix or tag."));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            switch (extension.m_identifierType)
            {
                case TableIdentifierType.KeyPrefix:
                    EditorGUI.PropertyField(position, m_identifier, new GUIContent("Prefix", "Only keys with this prefix will be pulled. When pushed, all the keys from this table will be prefixed with this string"));
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    break;
                case TableIdentifierType.Tag:
                    EditorGUI.PropertyField(position, m_identifier, new GUIContent("Tag Name", "Only keys with this tag will be pulled. When pushed, all the keys from this table will be tagged with this tag"));
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    break;
            }

            if (m_provider.objectReferenceValue != null)
            {
                if (provider != null)
                {
                    var buttonWidth = position.width / 2 - EditorGUIUtility.standardVerticalSpacing;
                    var buttonPosition = position;
                    buttonPosition.width = buttonWidth;
                    if (GUI.Button(buttonPosition, new GUIContent($"Push {provider.LocaleIdsToPush.Count} locale(s)", "Push the table content to Phrase")))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(provider.PushAll(new List<StringTableCollection> { collection }));
                    }

                    buttonPosition.x += buttonWidth + EditorGUIUtility.standardVerticalSpacing;
                    if (GUI.Button(buttonPosition, new GUIContent($"Pull {provider.LocaleIdsToPull.Count} locale(s)", "Pull the table content from Phrase")))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(provider.Pull(new List<StringTableCollection> { collection }));
                    }
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            height = position.y;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }
    }
}