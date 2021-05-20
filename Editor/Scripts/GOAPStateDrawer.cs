#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GOAP_Raw.Editors
{
    [CustomPropertyDrawer(typeof(GOAPState))]
    public class GOAPStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel++;
            var key = property.FindPropertyRelative("Key");
            var value = property.FindPropertyRelative("Value");
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
