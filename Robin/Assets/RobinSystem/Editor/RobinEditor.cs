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
        //DrawDefaultInspector();
        bodyObject = EditorGUILayout.ObjectField("Body",bodyObject,typeof(GameObject)) as GameObject;
        EditorGUILayout.Vector3Field("Input",robin.inputDirection);
    }
    #endregion

    #region Utility Function
    #endregion
}
