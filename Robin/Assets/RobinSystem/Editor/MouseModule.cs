using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MouseInteractModule))]
public class MouseModule : RobinEditor
{
    #region Public attributes
    private MenuState menuState = MenuState.Basic;
    private enum MenuState { Basic, Cursor, Output };
    #endregion

    #region Private attributes
    private SerializedProperty camera;
    private SerializedProperty groundLayer;
    private SerializedProperty length;
    private SerializedProperty position;
    private SerializedProperty hits;
    //private SerializedProperty hover;
    private SerializedProperty normalCursor;
    private SerializedProperty hoverCursor;
    private SerializedProperty activeCursor;
    #endregion

    #region Static attributes
    #endregion

    #region Main methods
    protected override void Initialize()
    {
        camera = serializedObject.FindProperty("handleCamera");
        groundLayer = serializedObject.FindProperty("groundLayer");
        length = serializedObject.FindProperty("mouseRayLength");
        position = serializedObject.FindProperty("mousePosition");
        hits = serializedObject.FindProperty("mouseHits");
        //hover = serializedObject.FindProperty("hoverEnemy");
        normalCursor = serializedObject.FindProperty("normalCursor");
        hoverCursor = serializedObject.FindProperty("hoverCursor");
        activeCursor = serializedObject.FindProperty("activeCursor");

        if (!camera.objectReferenceValue)
            camera.objectReferenceValue = Camera.main as Camera;
    }

    public override void OnInspectorGUI()
    {
        BeginEdit();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Basic"))
            menuState = MenuState.Basic;
        if (GUILayout.Button("Cursor"))
            menuState = MenuState.Cursor;
        if (GUILayout.Button("Output"))
            menuState = MenuState.Output;
        EditorGUILayout.EndHorizontal();

        //BeginSection("Setup");
        //PropertyField("Camera", camera);
        //PropertyField("Ray Length", length);
        //PropertyField("Ground Layer", groundLayer);
        //EndSection();
        if (menuState == MenuState.Basic)
        {
            ShowBlock("Basic Setup", 3);
            BlockPropertyField("Camera", 1, camera);
            BlockPropertyField("Ray Length", 2, length);
            BlockPropertyField("Ground Layer", 3, groundLayer);
        }
        else if (menuState == MenuState.Cursor)
        {
            ShowBlock("Cursor Setup", 3);
            BlockPropertyField("Normal", 1, normalCursor);
            BlockPropertyField("Hover", 2, hoverCursor);
            BlockPropertyField("Active", 3, activeCursor);
        }
        else if (menuState == MenuState.Output)
        {
            ShowBlock("Output", 1);
            BlockPropertyField("Position", 1, position);
            //BlockPropertyField("On Hover", 2, hover);
            ArrayProperty("Mouse Hits", hits.name, serializedObject);
        }
        //BeginSection("Output");
        //PropertyField("Position", position);
        EndEdit();
        //DrawDefaultInspector();
    }
    #endregion

    #region Utillity methods
    #endregion
}
