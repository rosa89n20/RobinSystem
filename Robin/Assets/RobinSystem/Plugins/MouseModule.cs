using UnityEngine;
using UnityEditor;
using System.Collections;
using HighlightingSystem;
using JrDevAssets;
using System.Collections.Generic;

public class MouseModule : MonoBehaviour
{
    #region Editor
    public MenuState menuState = MenuState.Basic;
    public enum MenuState { Basic, Cursor, Output };
    #endregion

    public Camera handleCamera;
    public LayerMask groundLayer;
    public float mouseRayLength = 10f;
    public Vector3 mousePosition;
    public GameObject[] mouseHits;
    public GameObject hoverEnemy;
    public enum MouseCursorType { Normal, Hover, Active }
    //public Texture2D NormalCursor { get { return MouseControlModule.normalCursor; } set { MouseControlModule.normalCursor = value; } }
    //public Texture2D HoverCursor { get { return MouseControlModule.hoverCursor; } set { MouseControlModule.hoverCursor = value; } }
    //public Texture2D ActiveCursor { get { return MouseControlModule.activeCursor; } set { MouseControlModule.activeCursor = value; } }

    //static Texture2D normalCursor;
    //static Texture2D hoverCursor;
    //static Texture2D activeCursor;

    private RaycastHit[] hits;

    public Texture2D normalCursor;
    public Texture2D hoverCursor;
    public Texture2D activeCursor;

    void Awake()
    {
        if (!handleCamera)
            handleCamera = Camera.main;
    }

    public RaycastHit[] CastRay(Camera camera, float length)
    {
        Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(mouseRay, length);
        return hits;
    }

    public void SetCursor(MouseCursorType type)
    {
        Texture2D cursor;
        switch (type)
        {
            case MouseCursorType.Normal:
                cursor = normalCursor;
                break;
            case MouseCursorType.Hover:
                cursor = hoverCursor;
                break;
            case MouseCursorType.Active:
                cursor = activeCursor;
                break;
            default:
                cursor = normalCursor;
                break;
        }
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }

    GameObject CompareTag(GameObject[] hits, string tag)
    {
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].tag == tag)
                return hits[i].gameObject;
        return null;
    }

    public bool OnHover(string tag)
    {
        if (hoverEnemy = CompareTag(mouseHits, tag))
            return true;
        else
            return false;
    }

    Vector3 CheckTargetedPosition(RaycastHit[] hits, LayerMask layer)
    {
        for (int i = 0; i < hits.Length; i++)
            if (Robin.CheckLayer(hits[i].collider.gameObject, layer))
                return hits[i].point; ;
        return gameObject.transform.position;
    }

    private GameObject[] GetMouseHits(RaycastHit[] hits)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < hits.Length; i++)
            list.Add(hits[i].collider.gameObject);
        return list.ToArray();
    }

    public void MouseInteract()
    {
        RaycastHit[] hits = CastRay(handleCamera, mouseRayLength);
        mouseHits = GetMouseHits(hits);
        mousePosition = CheckTargetedPosition(hits, groundLayer);
    }
}
