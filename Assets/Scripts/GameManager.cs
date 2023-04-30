using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioClip death;
    public AudioClip zombieDeath;
    public AudioClip pickupPizza;
    public AudioClip dropOffPizza;
  

    public AudioSource audioSource;

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
    List<GameObject> bigHouses;
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

    /////////////////////////////////////

    public GameObject summaryUI;
    public Button advertisingButton;
    public Button carButton;
    public Button gunButton;
    public Button pizzaButton;
    public Button healthButton;
    public Button ammoButton;

    public TextMeshProUGUI pizzaDeliveredText;
    public TextMeshProUGUI deliveryMoneyText;
    public TextMeshProUGUI zombiesKilledText;
    public TextMeshProUGUI lootingMoneyText;
    public TextMeshProUGUI moneyTodayText;
    public TextMeshProUGUI moneyNowText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI ammoText;

    /////////////////////////////////////

    public GameObject winUI;

    public TextMeshProUGUI winPizzaDeliveredText;
    public TextMeshProUGUI winDeliveryMoneyText;
    public TextMeshProUGUI winZombiesKilledText;
    public TextMeshProUGUI winLootingMoneyText;
    public TextMeshProUGUI winMoneyText;
    public TextMeshProUGUI winMoneyNowText;

    /////////////////////////////////////

    public GameObject deathUI;

    /////////////////////////////////////

    public GameObject pauseUI;
    public GameObject pauseButton;

    /////////////////////////////////////

    public GameObject startMenu;

    /////////////////////////////////////

    public TextMeshProUGUI healthActiveText;
    public TextMeshProUGUI ammoActiveText;
    public TextMeshProUGUI parlorAmmoText;

    /////////////////////////////////////

    public GameObject preRoads;
    public GameObject upgradedRoads;
    public GameObject preGrass;
    public GameObject upgradedGrass;
    public GameObject preNavMesh;
    public GameObject upgradedNavMesh;

    /////////////////////////////////////

    bool playerInteraction;

    float daytimeLength = 10f;
    float daytime;
    float nighttimeLength = 60f;
    float nighttime;

    float zombieSpawnInterval = 3f;
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

    int totalPizzas = 0;
    int totalPizzaMoney = 0;
    int totalZombies = 0;
    int totalZombiesMoney = 0;
    int totalMoney = 0;

    float volume = 0.5f;

    bool proceedButton = false;
    bool dead = false;

    enum GameState {
        Menu,
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
        dead = false;
        gameState = GameState.Menu;
        daytime = daytimeLength;
        SetTarget(pizzaPlace.transform.position);
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        startMenu.SetActive(true);
    }

    void Update() {

        AudioListener.volume = volume;

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
            targetLocationArrow.transform.rotation = Quaternion.Euler(0, 0, angle / Mathf.PI * 180 - 90);
        } else {
            targetLocationArrow.SetActive(false);
        }

        switch (gameState) {

            case GameState.Menu:
                break;

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

                    audioSource.PlayOneShot(pickupPizza);

                    gameState = GameState.Deliver;
                }

                break;

            case GameState.Deliver:

                if (nearTarget) ShowText("Press 'E' to deliver the order.");
                else HideText();

                if (playerInteraction && nearTarget) {
                    HideText();
                    SetTarget(pizzaPlace.transform.position);

                    audioSource.PlayOneShot(dropOffPizza);

                    todayPizzas += 1;
                    todayPizzaMoney += Random.Range(3, 5);
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

                    if ((player.GetAmmo() != 6 && !gunUpgrade) || (gunUpgrade && player.GetAmmo() != 18)) {
                        ShowText("Press 'E' to refill ammo.");
                    } else {
                        HideText();
                    }
                } else {
                    parlorAmmoText.enabled = false;
                    if (player.GetAmmo() == 0) {
                        ShowText("Refill ammo at the pizza place!");
                    } else {
                        HideText();
                    }
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
                } else if (nearTarget) {
                    parlorAmmoText.enabled = true;
                    parlorAmmoText.text = $"Ammo Storage: {ammo}";

                    if ((player.GetAmmo() != 6 && !gunUpgrade) || (gunUpgrade && player.GetAmmo() != 18)) {
                        ShowText("Press 'E' to refill ammo.");
                    } else {
                        HideText();
                    }
                } else {
                    parlorAmmoText.enabled = false;
                    if (player.GetAmmo() == 0) {
                        ShowText("Refill ammo at the pizza place!");
                    } else {
                        HideText();
                    }
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

                    totalMoney += money;
                    totalPizzaMoney += todayPizzaMoney;
                    totalZombiesMoney += todayZombiesMoney;
                    totalZombies += todayZombies;
                    totalPizzas += todayPizzas;

                    if(dayNumber != 5) {
                        OpenShopMenu();
                    } else {
                        OpenWinMenu();
                    }
                } else if (playerInteraction && nearTarget) {
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

        if (player.GetHealth() <= 0 && !dead) {
            PlayerDeath();
            dead = true;

        }

        playerInteraction = false;
        
    }

    public void PlayerControllerUpdateFriction(float friction) {
        player.UpdateFriction(friction);
    }

    public void PlayerInteract(InputAction.CallbackContext context) {
        if (context.action.IsPressed() && Time.timeScale != 0f) playerInteraction = true;
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
        target.transform.position = targetLocation + new Vector2(0, 3);
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

        if(gunUpgrade) z.transform.Find("Sprite").GetComponent<Zombie>().Init(player.transform, this, (dayNumber-1)/2);
        else z.transform.Find("Sprite").GetComponent<Zombie>().Init(player.transform, this, dayNumber);
        z.transform.position = zombieLocation;
        zombieCount += 1;
    }

    public void ZombieDeath(bool proper) {
        zombieCount -= 1;
        if (proper) todayZombies++;
        if (proper) todayZombiesMoney ++;
        if (proper) audioSource.PlayOneShot(zombieDeath);
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
            preRoads.SetActive(false);
            preGrass.SetActive(false);
            preNavMesh.SetActive(false);
            upgradedGrass.SetActive(true);
            upgradedRoads.SetActive(true);
            upgradedNavMesh.SetActive(true);
        }

        money -= 50;
        OpenShopMenu();
    }

    public void BuyHealth() {
        player.Heal();
        money -= 5;
        OpenShopMenu();
    }

    public void BuyAmmo() {
        ammo += 50;
        if(ammo >= 150) {
            ammo = 150;
        }
        money -= 5;
        OpenShopMenu();
    }

    public void NewDay() {
        proceedButton = true;
    }

    private void OpenShopMenu() {
        summaryUI.SetActive(true);

        advertisingButton.interactable = money >= 50;
        carButton.interactable = money >= 50;
        gunButton.interactable = money >= 50;
        pizzaButton.interactable = money >= 50;
        healthButton.interactable = money >= 5 && player.GetHealth() != 100;
        ammoButton.interactable = money >= 5 && ammo != 150;

        dayText.text = $"Day {dayNumber}";
        pizzaDeliveredText.text = $"Pizzas Delivered: {todayPizzas}";
        deliveryMoneyText.text = $"Money from Deliveries: {todayPizzaMoney}";
        zombiesKilledText.text = $"Zombies Killed: {todayZombies}";
        lootingMoneyText.text = $"Money from Looting: {todayZombiesMoney}";
        moneyTodayText.text = $"Earned Today: {todayPizzaMoney + todayZombiesMoney}";
        moneyNowText.text = $"${money}";
        healthText.text = $"Health: {player.GetHealth()}/100";
        ammoText.text = $"Ammo: {ammo}/150";

    }
    
    private void OpenWinMenu() {
        winUI.SetActive(true);

        winPizzaDeliveredText.text = $"Total Pizzas Delivered: {totalPizzas}";
        winDeliveryMoneyText.text = $"Total Money from Deliveries: ${totalPizzaMoney}";
        winZombiesKilledText.text = $"Total Zombies Killed: {totalZombies}";
        winLootingMoneyText.text = $"Total Money from Looting: ${totalZombiesMoney}";
        winMoneyText.text = $"Total Earned\n${totalMoney}";
        winMoneyNowText.text = $"Money Remaining\n${money}";
    }

    public void RestartGame() {
        SceneManager.LoadScene("GameScene");
        Time.timeScale = 1.0f;
    }

    public void RestartDay() {

    }

    public void ToMainMenu() {

    }

    public void StartGame() {
        Time.timeScale = 1;
        gameState = GameState.GetOrder;
        pauseButton.SetActive(true);
        startMenu.SetActive(false);
    }

    public void PlayerDeath() {
        audioSource.PlayOneShot(death);

        Time.timeScale = 0f;
        deathUI.SetActive(true);
    }

    public void TogglePause(InputAction.CallbackContext context) {
        if(!context.action.IsPressed()) return;

        if (gameState == GameState.Summary || gameState == GameState.Menu) return;

        if (Time.timeScale != 0) {
            Time.timeScale = 0f;
            pauseUI.SetActive(true);

        } else {
            Time.timeScale = 1f;
            pauseUI.SetActive(false);
        }

    }

    public void TogglePause() {

        if (gameState == GameState.Summary || gameState == GameState.Menu) return;

        if (Time.timeScale != 0) {
            Time.timeScale = 0f;
            pauseUI.SetActive(true);
            pauseButton.SetActive(false);
        
        } else {
            Time.timeScale = 1f;
            pauseUI.SetActive(false);
            pauseButton.SetActive(true);
        }

    }
}
