using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float speed;

    public void Init(Vector2 direction) {
        GetComponent<Rigidbody2D>().velocity = direction * speed;
        float angle = -Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.Euler(0, 0, angle * 180 / Mathf.PI);
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Zombie" || other.gameObject.tag == "House") {
            Destroy(gameObject);
        }
    }
}
