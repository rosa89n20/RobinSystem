using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
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
    public float area = 5f;
    #endregion

    #region Private Variables
    private Rect beginPosition;
    private SerializedProperty groundLayer;
    private SerializedProperty mouseRayIgnore;
    #endregion

    #region Main Function
    void OnEnable()
    {
        robin = (Robin)target;
        menuState = MenuState.Character;

        groundLayer = serializedObject.FindProperty("groundLayer");
        mouseRayIgnore = serializedObject.FindProperty("mouseRayIgnore");
    }

    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.box);
        titleStyle.stretchWidth = true;
        titleStyle.normal.textColor = Color.white;

        //GUI.skin = Resources.Load("RobinEditorSkin", typeof(GUISkin)) as GUISkin;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Character"))
            menuState = MenuState.Character;
        if (GUILayout.Button("Checker"))
            menuState = MenuState.Checker;
        if (GUILayout.Button("Attack"))
            menuState = MenuState.Attack;
        if (GUILayout.Button("Output"))
            menuState = MenuState.Output;
        EditorGUILayout.EndHorizontal();

        if (menuState == MenuState.Character)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Character Setup", titleStyle);
            ShowProgressBar(robin.character.healthPoint, robin._maxHp, "Health");
            ShowProgressBar(robin.character.manaPoint, robin._maxMana, "Mana");
            robin.character.rotateSpeed = EditorGUILayout.FloatField("Rotate Speed", robin.character.rotateSpeed);
            EditorGUILayout.EndVertical();
        }
        else if (menuState == MenuState.Checker)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Checker Setup", titleStyle);
            ShowBlock("Ground Checker", 3);
            EditorGUI.PropertyField(BlockContent(1), groundLayer, new GUIContent("Ground Layer"));
            robin.checker.groundRayLength = EditorGUI.FloatField(BlockContent(2), "Ray Length", robin.checker.groundRayLength);
            robin.checker.groundRayOffset = EditorGUI.FloatField(BlockContent(3), "Ray Offset", robin.checker.groundRayOffset);
            ShowBlock("Mouse Checker", 2);
            EditorGUI.PropertyField(BlockContent(1), mouseRayIgnore, new GUIContent("Ignore Layer"));
            robin.checker.mouseRayLength = EditorGUI.FloatField(BlockContent(2), "Ray Length", robin.checker.mouseRayLength);
            ShowBlock("Highlight System", 4);
            robin.checker.useHighlightingSystem = EditorGUI.Toggle(BlockContent(1), "Use it?", robin.checker.useHighlightingSystem);
            robin.checker.highlightColor = EditorGUI.ColorField(BlockContent(2), "Color", robin.checker.highlightColor);
            robin.checker.highlightRayLength = EditorGUI.FloatField(BlockContent(3), "Ray Length", robin.checker.highlightRayLength);
            robin.checker.highlightRayOffset = EditorGUI.FloatField(BlockContent(4), "Ray Offset", robin.checker.highlightRayOffset);
            EditorGUILayout.EndVertical();
        }
        else if (menuState == MenuState.Attack)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Attack Setup", titleStyle);
            robin.attack.attackRange = EditorGUILayout.FloatField( "Range", robin.attack.attackRange);
            robin.attack.attackAngle = EditorGUILayout.FloatField( "Angle", robin.attack.attackAngle);
            if (GUI.changed)
                EditorUtility.SetDirty(robin);
            EditorGUILayout.EndVertical();
        }
        else if (menuState == MenuState.Output)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Output Stream", titleStyle);
            robin.output.pointerPosition = EditorGUILayout.Vector3Field("Pointer Position", robin.output.pointerPosition);
            robin.output.isGrounded = EditorGUILayout.Toggle("Is Grounded?",robin.output.isGrounded);
            robin.output.hoverEnemy = EditorGUILayout.ObjectField("On Hover Enemy",robin.output.hoverEnemy,typeof(GameObject),false) as GameObject;
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(50);
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidArc(robin.transform.position, robin.transform.up, robin.transform.forward, robin.attack.attackAngle * 0.5f, robin.attack.attackRange);
        Handles.DrawSolidArc(robin.transform.position, robin.transform.up, robin.transform.forward, -robin.attack.attackAngle * 0.5f, robin.attack.attackRange);
        if (robin.output.isGrounded)
            Handles.color = new Color(0, 1, 0, 0.2f);
        else
            Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidDisc(robin.transform.position + new Vector3(0, robin.checker.groundRayOffset, 0), robin.transform.up, Mathf.Max(robin.transform.lossyScale.x, robin.transform.lossyScale.z) * 2);
        if (robin.output.isGrounded)
            Handles.color = Color.green;
        else
            Handles.color = Color.red;
        //Handles.DrawLine(robin.transform.position + new Vector3(0, robin.checker.groundRayOffset, 0), robin.transform.position + new Vector3(0, robin.checker.groundRayOffset-robin.transform.lossyScale.y, 0));
        Handles.DrawWireDisc(robin.transform.position + new Vector3(0, robin.checker.groundRayOffset, 0), robin.transform.up, Mathf.Max(robin.transform.lossyScale.x, robin.transform.lossyScale.z) * 2);
        Handles.color = Color.white;
    }
    #endregion

    #region Utility Function
    private void ShowProgressBar(float current, float max, string text)
    {
        //float scale=current/max;
        beginPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        //GUI.Box(beginPosition,"");
        //GUI.Box(new Rect(beginPosition.x,beginPosition.y,beginPosition.width*scale,beginPosition.height),"");
        //GUI.Label(beginPosition, text + " : " + current + " / " + max);
        EditorGUI.ProgressBar(beginPosition, current / max, text + " : " + current + " / " + max);
    }
    private void ShowBlock(string title, int lineUnit)
    {
        int line = lineUnit + 1;
        beginPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight * line + 4f, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
        GUI.Box(new Rect(beginPosition.x, beginPosition.y, beginPosition.width, EditorGUIUtility.singleLineHeight * line + 4f), "", GUI.skin.box);
        GUI.Label(new Rect(beginPosition.x + 2f, beginPosition.y + 2f, beginPosition.width, beginPosition.height), title);
    }
    private Rect BlockContent(int lineUnit)
    {
        float paddingLeft = 12f;
        float paddingRight = 14f;
        return new Rect(beginPosition.x + paddingLeft, beginPosition.y + EditorGUIUtility.singleLineHeight * lineUnit + 2f, beginPosition.width - paddingRight, EditorGUIUtility.singleLineHeight);
    }
    #endregion
}
