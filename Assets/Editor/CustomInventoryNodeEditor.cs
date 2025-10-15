using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

/*This is a workaround for the fact that Unity made their own custom editor for the button component, so inheriting from that component causes
 serialized fields to not show up. Solution: inherit from the button editor and write your serialized fields manually.*/

[CustomEditor(typeof(InventoryNode))]
public class CustomInventoryNodeEditor : ButtonEditor
{
    SerializedProperty itemImageOverlayProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        itemImageOverlayProperty = serializedObject.FindProperty("itemImageOverlay");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //Draw serizliaed fields below
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Additional Attributes", EditorStyles.boldLabel);

        //The actual attributes being shown
        EditorGUILayout.PropertyField(itemImageOverlayProperty);

        //Apply modified propertied
        serializedObject.ApplyModifiedProperties();
    }
}
