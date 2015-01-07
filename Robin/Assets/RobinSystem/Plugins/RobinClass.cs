using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterSetup
{
    public float healthPoint;
    public float manaPoint;
    public Animator mecanim;
    public float rotateSpeed;
}
[System.Serializable]
public class CheckerSetup
{
    public LayerMask ground;
    public float groundRayLength;
    public float groundRayOffset;
    public LayerMask mouseRayIgnore;
    public float mouseRayLength;
    public bool useHighlightingSystem;
    public Color highlightColor;
    public float highlightRayLength;
    public float highlightRayOffset;
}
[System.Serializable]
public class AttackSetup
{
    public float attackRange;
    public float attackAngle;
    public float damage;
    public MonoBehaviour GacSyatem;
    public GameObject attackEffect;
}
[System.Serializable]
public class OutputSetup
{
    public Vector3 pointerPosition;
    public GameObject pointerHit;
    public bool isGrounded;
    public GameObject hoverEnemy;
    public bool isLockOn;
    public GameObject lockOnEnemy;
}