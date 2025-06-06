#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion
using UnityEditor;
using UnityEngine;

namespace Atom.GOAP_Raw.Editors
{
    [CustomPropertyDrawer(typeof(GOAPState))]
    public class GOAPStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel++;
            var key = property.FindPropertyRelative(nameof(GOAPState.key));
            var value = property.FindPropertyRelative(nameof(GOAPState.value));
            GUI.Box(position, "");
            float width = position.width;

            position.width = 30;
            value.boolValue = EditorGUI.Toggle(position, "", value.boolValue);

            position.x += position.width;
            position.width = width - 35;
            key.stringValue = EditorGUI.TextField(position, key.stringValue);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 20; }
    }
}
