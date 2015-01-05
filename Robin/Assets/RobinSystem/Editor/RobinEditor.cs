using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Robin))]
public class RobinEditor : Editor
{
    #region Public Variables
    public Robin robin;
    public GameObject bodyObject;
    #endregion

    #region Private Variables
    #endregion

    #region Main Function
    void OnEnable()
    {
        robin = (Robin)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //Rect defaultPosition;
        //GUIStyle boxStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
        //boxStyle.fontSize = 14;
        //boxStyle.normal.textColor = Color.white;
        //boxStyle.alignment = TextAnchor.MiddleCenter;

        //EditorGUILayout.BeginVertical();
        //EditorGUILayout.LabelField(new GUIContent("Character Setup"), boxStyle, GUILayout.ExpandWidth(true));

        //bodyObject = EditorGUILayout.ObjectField("Body", bodyObject, typeof(GameObject)) as GameObject;
        //EditorGUILayout.Vector3Field("Input", robin.inputDirection);
        //EditorGUILayout.EndVertical();
    }
    #endregion

    #region Utility Function
    #endregion
}
