using System.Collections;
using System.Collections.Generic;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;

    float acceleration;
    float turn;
    float turnSpeed = 180f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Controls(InputAction.CallbackContext context) {
        float val = context.ReadValue<float>();

        if (context.action.name == "Accelerate") {
            acceleration = val;
        } else if (context.action.name == "Turn") {
            turn = -val;
        } else {
            Debug.Log("This shouldn't be happening.");
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        rb.AddForce(transform.up * acceleration, ForceMode2D.Impulse);

        float percentage = Mathf.Abs(rb.velocity.magnitude / (2 * rb.drag / rb.mass));

        rb.MoveRotation(rb.rotation + Time.deltaTime * turn * turnSpeed * percentage);
    }
}
