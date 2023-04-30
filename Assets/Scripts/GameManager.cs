using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    List<GameObject> zombieSpawns;

    [SerializeField]
    TextMeshProUGUI statusText;
    [SerializeField]
    Image nightTimeFilter;

    public TextMeshProUGUI pizzaDeliveredText;
    public TextMeshProUGUI deliveryMoneyText;
    public TextMeshProUGUI zombiesKilledText;
    public TextMeshProUGUI lootingMoneyText;
    public TextMeshProUGUI moneyTodayText;
    public TextMeshProUGUI moneyNowText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI ammoText;

    public TextMeshProUGUI healthActiveText;
    public TextMeshProUGUI ammoActiveText;
    public TextMeshProUGUI parlorAmmoText;

    public GameObject summaryUI;
    public Button advertisingButton;
    public Button carButton;
    public Button gunButton;
    public Button pizzaButton;
    public Button healthButton;
    public Button ammoButton;

    bool playerInteraction;

    float daytimeLength = 10f;
    float daytime;
    float nighttimeLength = 10f;
    float nighttime;

    float zombieSpawnInterval = 2f;
    float zombieSpawnTimer;

    int zombieCount = 0;

    bool nearPizza;
    bool nearTarget;
    int targetHouse;
    Vector2 targetLocation;

    int dayNumber = 1;
    int money = 0;
    int ammo = 100;

    bool advertisingUpgrade = false;
    bool carUpgrade = false;
    bool gunUpgrade = false;
    bool pizzaUpgrade = false;

    int todayPizzas = 0;
    int todayPizzaMoney = 0;
    int todayZombies = 0;
    int todayZombiesMoney = 0;

    bool proceedButton = false;

    enum GameState {
        GetOrder,
        Deliver,
        GetBack,
        NightPrep,
        Night,
        NightGetBack, 
        Summary
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
        zombieSpawnTimer -= Time.deltaTime;

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

                    todayPizzas += 1;
                    todayPizzaMoney += Random.Range(1, 3);
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
                zombieSpawnTimer = zombieSpawnInterval;
                gameState = GameState.Night;
                player.CanShoot(true);

                break;

            case GameState.Night:

                SetTarget(pizzaPlace.transform.position);
                HideTarget();

                healthActiveText.enabled = true;
                healthActiveText.text = $"Health: {player.GetHealth()}/100";
                ammoActiveText.enabled = true;

                if(!gunUpgrade) ammoActiveText.text = $"Ammo: {player.GetAmmo()}/6";
                else ammoActiveText.text = $"Ammo: {player.GetAmmo()}/18";

                if (nighttime <= 0f) {
                    gameState = GameState.NightGetBack;
                }

                if (zombieSpawnTimer <= 0f) {
                    zombieSpawnTimer = zombieSpawnInterval;
                    SummonZombie();
                }

                if (nearTarget) {
                    parlorAmmoText.enabled = true;
                    parlorAmmoText.text = $"Ammo Storage: {ammo}";

                    if ((ammo != 6 && !gunUpgrade) || (ammo != 18)) {
                        ShowText("Press 'E' to refill ammo.");
                    }
                } else {
                    parlorAmmoText.enabled = false;
                    HideText();
                }

                if (playerInteraction && nearTarget) {
                   
                    if (gunUpgrade) {
                        ammo -= (18 - player.GetAmmo());
                        player.Refill(18);
                    } else {
                        ammo -= (6 - player.GetAmmo());
                        player.Refill(6);
                    }
                }

                break;

            case GameState.NightGetBack:

                healthActiveText.text = $"Health: {player.GetHealth()}/100";
                if (!gunUpgrade) ammoActiveText.text = $"Ammo: {player.GetAmmo()}/6";
                else ammoActiveText.text = $"Ammo: {player.GetAmmo()}/18";

                if (zombieCount == 0 && nearTarget) {
                    ShowText("Press 'E' to go to sleep.");
                } else if (zombieCount == 0) {
                    ShowText("I can head back now.");
                    SetTarget(pizzaPlace.transform.position);
                }

                if (nearTarget) {
                    parlorAmmoText.enabled = true;
                    parlorAmmoText.text = $"Ammo Storage: {ammo}";
                } else {
                    parlorAmmoText.enabled = false;
                }

                if (playerInteraction && nearTarget && zombieCount == 0) {
                    HideText();
                    nightTimeFilter.enabled = false;
                    player.CanShoot(false);
                    gameState = GameState.Summary;
                    Time.timeScale = 0f;

                    healthActiveText.enabled = false;
                    ammoActiveText.enabled = false;
                    parlorAmmoText.enabled = false;

                    money += todayPizzaMoney;
                    money += todayZombiesMoney;
                    OpenShopMenu();
                } else if (playerInteraction && nearTarget) {
                    if ((ammo != 6 && !gunUpgrade) || (ammo != 18)) {
                        ShowText("Press 'E' to refill ammo.");
                    }

                    if (gunUpgrade) {
                        ammo -= (18 - player.GetAmmo());
                        player.Refill(18);
                    } else {
                        ammo -= (6 - player.GetAmmo());
                        player.Refill(6);
                    }
                }

                break;

            case GameState.Summary:

                if (proceedButton) {
                    proceedButton = false;
                    summaryUI.SetActive(false);
                    todayPizzaMoney = 0;
                    todayPizzas = 0;
                    todayZombies = 0;
                    todayZombiesMoney = 0;
                    daytime = daytimeLength;
                    dayNumber += 1;
                    zombieSpawnInterval -= 0.2f;
                    gameState = GameState.GetOrder;
                    SetTarget(pizzaPlace.transform.position);
                    Time.timeScale = 1f;
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

        int zombieIndex = Random.Range(0, zombieSpawns.Count);
        Vector2 zombieLocation = (zombieSpawns[zombieIndex].transform.position);

        Vector2 screenPos = camera.WorldToScreenPoint(zombieLocation);
        while (!(screenPos.x < 0 || screenPos.y < 0 || screenPos.x > camera.pixelWidth || screenPos.y > camera.pixelHeight)) {
            zombieIndex = Random.Range(0, zombieSpawns.Count);
            zombieLocation = (zombieSpawns[zombieIndex].transform.position);

            screenPos = camera.WorldToScreenPoint(zombieLocation);
        }
        
        GameObject z = Instantiate(zombie);

        if(gunUpgrade) z.GetComponent<Zombie>().Init(player.transform, this, (dayNumber-1)/2);
        else z.GetComponent<Zombie>().Init(player.transform, this, dayNumber);
        z.transform.position = zombieLocation;
        zombieCount += 1;
    }

    public void ZombieDeath(bool proper) {
        zombieCount -= 1;
        if (proper) todayZombies++;
        if (proper) todayZombiesMoney += Random.Range(1, 2);
    }

    public void BuyUpgrade(int up) {
        if (up == 0) {
            advertisingUpgrade = true;
        } else if (up == 1) {
            carUpgrade = true;
            player.UpgradeCar();
        } else if (up == 2) {
            gunUpgrade = true;
            player.UpgradeGun();
        } else if (up == 3) {
            pizzaUpgrade = true;
        }

        money -= 10;
        OpenShopMenu();
    }

    public void BuyHealth() {
        player.Heal();
        money -= 2;
        OpenShopMenu();
    }

    public void BuyAmmo() {
        ammo = 100;
        money -= 2;
        OpenShopMenu();
    }

    public void NewDay() {
        proceedButton = true;
    }

    private void OpenShopMenu() {
        summaryUI.SetActive(true);

        advertisingButton.interactable = money >= 10;
        carButton.interactable = money >= 10;
        gunButton.interactable = money >= 10;
        pizzaButton.interactable = money >= 10;
        healthButton.interactable = money >= 2 && player.GetHealth() != 100;
        ammoButton.interactable = money >= 2 && ammo != 100;

        dayText.text = $"Day {dayNumber}";
        pizzaDeliveredText.text = $"Pizzas Delivered: {todayPizzas}";
        deliveryMoneyText.text = $"Money from Deliveries: {todayPizzaMoney}";
        zombiesKilledText.text = $"Zombies Killed: {todayZombies}";
        lootingMoneyText.text = $"Money from Looting: {todayZombiesMoney}";
        moneyTodayText.text = $"Earned Today: {todayPizzaMoney + todayZombiesMoney}";
        moneyNowText.text = $"${money}";
        healthText.text = $"Health: {player.GetHealth()}/100";
        ammoText.text = $"Ammo: {ammo}/100";

    }
}
