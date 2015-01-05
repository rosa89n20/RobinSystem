using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public GameObject damageText;
    public static GameObject _damageText;

    void Awake()
    {
        _damageText = damageText;
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
