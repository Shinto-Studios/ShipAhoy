using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public float speed = 20f;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.up * speed;
    }

    void OnTriggerEnter(Collider hitInfo)
    {
        Debug.Log(hitInfo.name);

        if (hitInfo.gameObject.tag  == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}
