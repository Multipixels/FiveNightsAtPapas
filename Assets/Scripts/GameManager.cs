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
    TextMeshProUGUI getOrderText;
    [SerializeField]
    TextMeshProUGUI deliverOrderText;
    [SerializeField]
    TextMeshProUGUI nightPrepText;
    [SerializeField]
    Image nightTimeFilter;
    [SerializeField]
    TextMeshProUGUI zombiesRemainText;
    [SerializeField]
    TextMeshProUGUI sleepText;

    float daytime = 60f;
    float nighttime = 60f;

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
        targetLocation = pizzaPlace.transform.position;
        target.transform.position = pizzaPlace.transform.position;
        target.SetActive(true);
    }

    void Update() {
        camera.transform.position = player.transform.position + new Vector3(0, 0, -10);
        nearTarget = (new Vector2(player.transform.position.x, player.transform.position.y) - targetLocation).magnitude <= targetRadius;

        switch (gameState) {
            case GameState.GetOrder:
                nightTimeFilter.enabled = false;
                getOrderText.enabled = nearTarget;

                if(daytime <= 0) {
                    gameState = GameState.GetBack;
                }

                break;
            case GameState.Deliver:
                deliverOrderText.enabled = nearTarget;
                break;
            case GameState.GetBack:
                nightPrepText.enabled = nearTarget;
                break;
            case GameState.NightPrep:
                nightTimeFilter.enabled = true;
                nighttime = 60f;
                gameState = GameState.Night;
                break;
            case GameState.Night:
                if (nighttime <= 0f) {
                    gameState = GameState.NightGetBack;
                }
                break;
            case GameState.NightGetBack:
                if (zombieCount >= 1) {
                    zombiesRemainText.enabled = nearTarget;
                } else {
                    sleepText.enabled = nearTarget;
                }
                break;
        }

        Vector2 screenPos = camera.WorldToScreenPoint(target.transform.position);

        if ((screenPos.x < 0 || screenPos.y < 0 || screenPos.x > camera.pixelWidth || screenPos.y > camera.pixelHeight) && target.activeSelf) {
            targetLocationArrow.SetActive(true);

            float angle = -Mathf.Atan2(player.transform.position.y - targetLocation.y
                                      , -player.transform.position.x + targetLocation.x);

            targetLocationArrow.transform.position = player.transform.position + 1.5f * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

        } else {
            targetLocationArrow.SetActive(false);
        }

        daytime -= Time.deltaTime;
        nighttime -= Time.deltaTime;

    }

    public void PlayerControllerUpdateFriction(float friction) {
        player.UpdateFriction(friction);
    }

    public void PlayerInteract() {
        if(gameState == GameState.GetOrder && nearTarget) {
            gameState = GameState.Deliver;
            getOrderText.enabled = false;
            targetHouse = Random.Range(0, houses.Count);
            targetLocation = houses[targetHouse].transform.position;
            target.transform.position = targetLocation;
            target.SetActive(true);
        } else if(gameState == GameState.Deliver && nearTarget) {
            gameState = GameState.GetOrder;
            deliverOrderText.enabled = false;
            targetLocation = pizzaPlace.transform.position;
            target.transform.position = targetLocation;
        } else if(gameState == GameState.GetBack && nearTarget) {
            gameState = GameState.NightPrep;
            nightPrepText.enabled = false;
            target.SetActive(false);
        } else if(gameState == GameState.NightGetBack && nearTarget && zombieCount == 0) {
            sleepText.enabled = false;
            targetLocation = pizzaPlace.transform.position;
            target.transform.position = targetLocation;
            target.SetActive(true);
            daytime = 60f;
            gameState = GameState.Deliver;
        }
    }

    private void SummonZombie() {

        GameObject z = Instantiate(zombie);
        z.GetComponent<Zombie>().player = player.transform;

    }

    public void ZombieDeath() {
        Debug.Log("zombie died");
    }
}
