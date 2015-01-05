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
    public MenuState menuState;
    public enum MenuState { Character, Checker, Attack, Output };
    #endregion

    #region Private Variables

    #endregion

    #region Main Function
    void OnEnable()
    {
        robin = (Robin)target;
        menuState = MenuState.Character;
    }

    public override void OnInspectorGUI()
    {
        Rect defaultPosition;
        //GUIStyle boxStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
        //boxStyle.fontSize = 14;
        //boxStyle.normal.textColor = Color.white;
        //boxStyle.alignment = TextAnchor.MiddleCenter;
        //boxStyle.font = Resources.Load("Minecraftia", typeof(Font)) as Font;
        GUI.skin = Resources.Load("RobinEditorSkin", typeof(GUISkin)) as GUISkin;

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0, 0, 0, 0.4f);
        if (GUILayout.Button("Character"))
            menuState = MenuState.Character;
        if (GUILayout.Button("Checker"))
            menuState = MenuState.Checker;
        if (GUILayout.Button("Attack"))
            menuState = MenuState.Attack;
        if (GUILayout.Button("Output"))
            menuState = MenuState.Output;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        if (menuState == MenuState.Character)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Character Setup", GUI.skin.box);
            //EditorGUILayout.FloatField("Health", robin.character.healthPoint);
            defaultPosition = GUILayoutUtility.GetRect(1, 1, 1, 20);
            GUI.backgroundColor = Color.red;
            EditorGUI.ProgressBar(defaultPosition, robin.character.healthPoint / robin._maxHp, "Health");
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(50);
        DrawDefaultInspector();
    }
    #endregion

    #region Utility Function
    #endregion
}
