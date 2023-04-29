using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    PlayerController player;
    [SerializeField]
    Camera camera;

    // Update is called once per frame
    void Update() {
        camera.transform.position = player.transform.position + new Vector3(0, 0, -10);
    }
}
