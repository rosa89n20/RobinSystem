using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour
{
    private TextMesh _text;
    public bool alwaysFacingCamera;
    public float lifetime = 1f;
    private float _time;
    private bool _hasForce;

    void Awake()
    {
        if (GetComponent<TextMesh>() != null)
            _text = GetComponent<TextMesh>();
    }

    void Start()
    {
        //rigidbody.velocity=Vector3.up * 5f;
        //rigidbody.velocity = new Vector3(Random.Range(-5, 6), 5, 0);

        if (_text != null)
        {
            if (!alwaysFacingCamera)
                transform.parent.LookAt(Camera.main.transform.position);
            //iTween.ColorTo(gameObject,iTween.Hash("a",0,"delay",0.3f,"time",1f));
        }
    }

    void Update()
    {
        if (_time > lifetime)
            Destroy(transform.parent.gameObject);
        else
            _time += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (_text != null)
        {
            if (alwaysFacingCamera)
                transform.parent.LookAt(Camera.main.transform.position);
            _text.color -= new Color(0, 0, 0, 1f * Time.deltaTime);
        }
    }

    public void SetText(string text, Color color, Vector3 force)
    {
        if (_text != null)
        {
            _text.text = text;
            _text.color = color;
            if (!_hasForce)
            {
                rigidbody.velocity = force;
                _hasForce = true;
            }
        }
    }

    void OnTriggerEnter(Collider trigger)
    {
        if (trigger.gameObject.layer == LayerMask.NameToLayer("Ground"))
            Destroy(transform.parent.gameObject);
    }
}
