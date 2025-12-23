using UnityEditor.AddressableAssets.GUI;
using UnityEditor;

namespace Phrase
{
    /// <summary>
    /// Extension methods for SerializedProperty. Provides a way to get the actual object from a SerializedProperty.
    /// </summary>
    static class SerializedPropertyExtensionMethods
    {
        public static TObject GetActualObjectForSerializedProperty<TObject>(this SerializedProperty property, System.Reflection.FieldInfo field)
        {
            string unused = "";
            return SerializedPropertyExtensions.GetActualObjectForSerializedProperty<TObject>(property, field, ref unused);
        }
    }
}
