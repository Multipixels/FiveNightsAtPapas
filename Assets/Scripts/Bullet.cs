using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float speed;

    public void Init(Vector2 direction) {
        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Zombie" || other.gameObject.tag == "House") {
            Destroy(gameObject);
        }
    }
}
