﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public GameObject damageText;
    static GameObject _damageText;

    
    public Texture2D normal;
    public Texture2D hover;

    static Texture2D _normalCursor;
    static Texture2D _hoverCursor;

    void Awake()
    {
        _damageText = damageText;
        _normalCursor = normal;
        _hoverCursor = hover;
    }

    public static void CreateDamageText(GameObject target, string text, Color color, Vector3 force)
    {
        if (_damageText != null)
        {
            GameObject t = Instantiate(_damageText, target.transform.position + Vector3.up, Quaternion.identity) as GameObject;
            t.transform.FindChild("Text").GetComponent<DamageText>().SetText(text, color, force);
        }
    }

    
}
