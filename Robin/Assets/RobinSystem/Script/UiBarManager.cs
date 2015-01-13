using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiBarManager : MonoBehaviour
{
    public RectTransform bar;
    public Text text;
    public Vector2 value = new Vector2(100, 100);
    private float _defaultBarWidth;

    void Awake()
    {
        _defaultBarWidth = bar.sizeDelta.x;
    }

    void Update()
    {
        if (text)
            text.text = Mathf.RoundToInt(value.x) + "/" + Mathf.RoundToInt(value.y);
    }

    public void SetValue(Vector2 point)
    {
        value = point;
        float _s = value.x / value.y;
        bar.sizeDelta = new Vector2(_defaultBarWidth * _s, bar.sizeDelta.y);
    }

    public void SetScale(Vector2 point)
    {
        value = point;
        float _s = value.x / value.y;
        bar.localScale = new Vector3(_s, 1, 1);
    }
}
