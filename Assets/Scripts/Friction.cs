using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Friction : MonoBehaviour
{
    [SerializeField]
    float frictionValue;

    [SerializeField]
    UnityEvent<float> enterTrigger;

    [SerializeField]
    UnityEvent<float> exitTrigger;

    void OnTriggerEnter2D(Collider2D other) {
        enterTrigger.Invoke(frictionValue);
    }

    private void OnTriggerExit2D(Collider2D other) {
        exitTrigger.Invoke(0);
    }
}
