using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    float pizzaRadius;
    [SerializeField]
    float targetRadius;

    [SerializeField]
    PlayerController player;
    [SerializeField]
    Camera camera;

    [SerializeField]
    GameObject pizzaPlace;
    [SerializeField]
    List<GameObject> houses;

    [SerializeField]
    TextMeshProUGUI getOrderText;
    [SerializeField]
    TextMeshProUGUI deliverOrderText;

    bool nearPizza;
    bool nearTarget;
    int targetHouse;
    Vector2 targetLocation;

    enum GameState {
        GetOrder,
        Deliver
    }
    GameState gameState;

    private void Start() {
        gameState = GameState.GetOrder;
    }

    // Update is called once per frame
    void Update() {
        camera.transform.position = player.transform.position + new Vector3(0, 0, -10);
        nearPizza = (player.transform.position - pizzaPlace.transform.position).magnitude <= pizzaRadius;

        switch (gameState) {
            case GameState.GetOrder:
                getOrderText.enabled = nearPizza;

                break;
            case GameState.Deliver:
                nearTarget = (new Vector2(player.transform.position.x, player.transform.position.y) - targetLocation).magnitude <= targetRadius;

                deliverOrderText.enabled = nearTarget;

                break;
        }
    
    
    }

    public void PlayerControllerUpdateFriction(float friction) {
        player.UpdateFriction(friction);
    }

    public void PlayerInteract() {
        if(gameState == GameState.GetOrder && nearPizza) {
            gameState = GameState.Deliver;
            getOrderText.enabled = false;
            targetHouse = Random.Range(0, houses.Count);
            targetLocation = houses[targetHouse].transform.position;
        } else if(gameState == GameState.Deliver && nearTarget) {
            gameState = GameState.GetOrder;
            deliverOrderText.enabled = false;
        }
    }
}
