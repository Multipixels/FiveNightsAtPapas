using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    UnityEvent interact;

    Rigidbody2D rb;

    float acceleration;
    float turn;
    float turnSpeed = 180f;
    float typicalDrag = 4f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Controls(InputAction.CallbackContext context) {
        if (context.action.name == "Accelerate") {
            float val = context.ReadValue<float>();
            acceleration = val;
        } else if (context.action.name == "Turn") {
            float val = context.ReadValue<float>();
            turn = -val;
        } else if (context.action.name == "Interact") {
            
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

    public void UpdateFriction(float friction) {
        rb.drag = typicalDrag * (1 + friction);
    }

}
