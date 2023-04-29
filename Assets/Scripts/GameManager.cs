using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    GameObject target;
    [SerializeField]
    GameObject targetLocationArrow;
    [SerializeField]
    GameObject zombie;

    [SerializeField]
    TextMeshProUGUI statusText;
    [SerializeField]
    Image nightTimeFilter;

    bool playerInteraction;

    float daytimeLength = 10f;
    float daytime;
    float nighttimeLength = 10f;
    float nighttime;

    int zombieCount = 0;

    bool nearPizza;
    bool nearTarget;
    int targetHouse;
    Vector2 targetLocation;

    enum GameState {
        GetOrder,
        Deliver,
        GetBack,
        NightPrep,
        Night,
        NightGetBack
    }

    GameState gameState;

    private void Start() {
        gameState = GameState.GetOrder;
        daytime = daytimeLength;
        SetTarget(pizzaPlace.transform.position);
    }

    void Update() {
        daytime -= Time.deltaTime;
        nighttime -= Time.deltaTime;

        camera.transform.position = player.transform.position + new Vector3(0, 0, -10);
        nearTarget = (new Vector2(player.transform.position.x, player.transform.position.y) - targetLocation).magnitude <= targetRadius;

        Vector2 screenPos = camera.WorldToScreenPoint(target.transform.position);
        if ((screenPos.x < 0 || screenPos.y < 0 || screenPos.x > camera.pixelWidth || screenPos.y > camera.pixelHeight) && target.activeSelf) {
            targetLocationArrow.SetActive(true);

            float angle = -Mathf.Atan2(player.transform.position.y - targetLocation.y
                                      , -player.transform.position.x + targetLocation.x);

            targetLocationArrow.transform.position = player.transform.position + 1.5f * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        } else {
            targetLocationArrow.SetActive(false);
        }

        switch (gameState) {
            case GameState.GetOrder:
                if (nearTarget) ShowText("Press 'E' to grab the order");
                else HideText();

                if (daytime <= 0) {
                    gameState = GameState.GetBack;
                }

                if (playerInteraction && nearTarget) {
                    HideText();

                    targetHouse = Random.Range(0, houses.Count);
                    SetTarget(houses[targetHouse].transform.position);

                    gameState = GameState.Deliver;
                }

                break;

            case GameState.Deliver:

                if (nearTarget) ShowText("Press 'E' to deliver the order.");
                else HideText();

                if (playerInteraction && nearTarget) {
                    HideText();
                    SetTarget(pizzaPlace.transform.position);

                    gameState = GameState.GetOrder;
                }

                break;

            case GameState.GetBack:

                if (nearTarget) ShowText("Press 'E' to prepare for the night.");
                else HideText();

                if (playerInteraction && nearTarget) {
                    HideText();
                    HideTarget();

                    gameState = GameState.NightPrep;
                }

                break;

            case GameState.NightPrep:

                nightTimeFilter.enabled = true;
                nighttime = nighttimeLength;
                gameState = GameState.Night;
                player.CanShoot(true);

                break;

            case GameState.Night:

                if (nighttime <= 0f) {
                    gameState = GameState.NightGetBack;
                }

                break;

            case GameState.NightGetBack:

                if (zombieCount == 0 && nearTarget) {
                    ShowText("Press 'E' to go to sleep.");
                }
                else if (zombieCount == 0) {
                    ShowText("I can head back now.");
                    SetTarget(pizzaPlace.transform.position);
                }

                if (playerInteraction && nearTarget) {
                    HideText();
                    nightTimeFilter.enabled = false;
                    daytime = daytimeLength;
                    player.CanShoot(false);
                    gameState = GameState.GetOrder;
                }

                break;
        }

        playerInteraction = false;
    }

    public void PlayerControllerUpdateFriction(float friction) {
        player.UpdateFriction(friction);
    }

    public void PlayerInteract(InputAction.CallbackContext context) {
        if(context.action.IsPressed()) playerInteraction = true;
    }

    private void ShowText(string text) {
        statusText.text = text;
        statusText.enabled = true;
    }

    private void HideText() {
        statusText.enabled = false;
    }

    private void SetTarget(Vector3 location) {
        targetLocation = location;
        target.transform.position = targetLocation;
        target.SetActive(true);
    }

    private void HideTarget() {
        target.SetActive(false);
    }
    private void SummonZombie() {

        GameObject z = Instantiate(zombie);
        z.GetComponent<Zombie>().player = player.transform;

    }

    public void ZombieDeath() {
        Debug.Log("zombie died");
    }
}
