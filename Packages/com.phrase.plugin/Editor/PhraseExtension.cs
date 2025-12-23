using UnityEditor.Localization;

namespace Phrase
{
    public enum TableIdentifierType
    {
        None,
        KeyPrefix,
        Tag
    }


    [StringTableCollectionExtension]
    public class PhraseExtension : CollectionExtension
    {
        public PhraseProvider m_provider;

        public TableIdentifierType m_identifierType = TableIdentifierType.None;

        public string m_identifier;
    }
}