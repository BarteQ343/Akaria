using CsvHelper;

using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine.Localization.Tables;

namespace Phrase
{
    public class PhraseCsvColumns : CsvColumns, IKeyColumn
    {
        int m_KeyNameIndex, m_KeyIdIndex, m_DescriptionIndex, m_MaxCharsIndex;

        string m_KeyPrefix = null;
        private const string k_KeyName = "key_name";
        private const string k_KeyId = "key_id";
        private const string k_Description = "comment";
        private const string k_MaxChars = "max_characters_allowed";
        SharedTableData m_SharedTableData;

        public string KeyPrefix
        {
            get => m_KeyPrefix;
            set => m_KeyPrefix = value;
        }

        public override void ReadBegin(StringTableCollection collection, CsvReader reader)
        {
            m_SharedTableData = collection.SharedData;
            m_KeyNameIndex = reader.GetFieldIndex(k_KeyName, isTryGet: true);
            m_KeyIdIndex = reader.GetFieldIndex(k_KeyId, isTryGet: true);
            m_DescriptionIndex = reader.GetFieldIndex(k_Description, isTryGet: true);
            m_MaxCharsIndex = reader.GetFieldIndex(k_MaxChars, isTryGet: true);
        }

        public override void ReadRow(SharedTableData.SharedTableEntry keyEntry, CsvReader reader)
        {
            // Get the metadata or add one
            var metadata = keyEntry.Metadata.GetMetadata<PhraseMetadata>();
            if (metadata == null)
            {
                metadata = new PhraseMetadata();
                keyEntry.Metadata.AddMetadata(metadata);
            }

            if (m_KeyIdIndex != -1)
            {
                metadata.KeyId = reader.GetField(m_KeyIdIndex);
            }
            if (m_DescriptionIndex != -1)
            {
                metadata.Description = reader.GetField(m_DescriptionIndex);
            }
            if (m_MaxCharsIndex != -1)
            {
                if (reader.TryGetField<int>(m_MaxCharsIndex, out var maxChars))
                {
                    metadata.MaxLength = maxChars;
                }
                else
                {
                    metadata.MaxLength = 0;
                }
            }
        }

        public override void WriteBegin(StringTableCollection collection, CsvWriter writer)
        {
            writer.WriteField(k_KeyName);
            writer.WriteField(k_Description);
            writer.WriteField(k_MaxChars);
        }

        public override void WriteRow(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, CsvWriter writer)
        {
            string keyName = keyEntry.Key;
            if (!string.IsNullOrEmpty(m_KeyPrefix))
                keyName = m_KeyPrefix + keyName;
            writer.WriteField(keyName);

            var metadata = keyEntry.Metadata.GetMetadata<PhraseMetadata>();
            if (metadata != null)
            {
                writer.WriteField(metadata.Description, true);
                if (metadata.MaxLength > 0)
                {
                    writer.WriteField(metadata.MaxLength);
                }
                else
                {
                    writer.WriteField(string.Empty);
                }
                return;
            }

            // Write empty entries
            writer.WriteField(string.Empty);
            writer.WriteField(string.Empty);
        }

        public SharedTableData.SharedTableEntry ReadKey(CsvReader reader)
        {
            string keyName = reader.GetField(m_KeyNameIndex);
            if (string.IsNullOrEmpty(keyName))
                return null;
            return m_SharedTableData.GetEntry(keyName) ?? m_SharedTableData.AddKey(keyName);
        }
    }
}
