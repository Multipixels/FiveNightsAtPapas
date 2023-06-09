using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    GameObject bullet;

    public GameObject pauseButton;

    AudioSource audioSource;
    public AudioClip shoot;
    public AudioClip hurt;
    public AudioClip shootFail;
    public AudioClip reload;

    bool isShooting = false;

    Rigidbody2D rb;

    bool canShoot = false;

    float acceleration;
    float turn;
    float turnSpeed = 120f;
    float typicalDrag;

    float shootingInterval = 0.75f;
    float shootingCooldown;

    int health = 100;
    int ammo = 6;

    bool carUpgrade = false;
    bool gunUpgrade = false;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        typicalDrag = 5f;
        rb.drag = typicalDrag;
        audioSource = GetComponent<AudioSource>();
    }

    public void Controls(InputAction.CallbackContext context) {
        if (context.action.name == "Accelerate") {
            float val = context.ReadValue<float>();
            acceleration = val;
        } else if (context.action.name == "Turn") {
            float val = context.ReadValue<float>();
            turn = -val;
        } else if (context.action.name == "Shoot") {
            if (canShoot) isShooting = context.action.IsPressed();
            else isShooting = false;
        } else if (context.action.name == "Interact") {
            
        } else {
            Debug.Log("This shouldn't be happening.");
        }
    }

    private void Update() {
        if (isShooting && shootingCooldown <= 0f && ammo > 0 && Time.deltaTime != 0 && !isPlayerHoveringPause()) {
            GameObject bObj = Instantiate(bullet);
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue()) - transform.position;
            bObj.transform.position = transform.position;
            bObj.GetComponent<Bullet>().Init(worldPosition.normalized);
            shootingCooldown = shootingInterval;
            ammo--;
            audioSource.PlayOneShot(shoot);
        } else if (isShooting && shootingCooldown <= 0f && ammo == 0 && Time.deltaTime != 0 && !isPlayerHoveringPause()) {
            audioSource.PlayOneShot(shootFail);
            shootingCooldown = shootingInterval;
        }
    }

    private bool isPlayerHoveringPause() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);

        for(int i = 0; i < raycastResultList.Count; i++) {
            if (raycastResultList[i].gameObject.name == "PauseButton") return true;
        }

        return false;
    }

    void FixedUpdate() {
        rb.AddForce(transform.up * acceleration, ForceMode2D.Impulse);

        float percentage = Mathf.Abs(rb.velocity.magnitude / (2 * rb.drag / rb.mass));

        rb.MoveRotation(rb.rotation + Time.deltaTime * turn * turnSpeed * percentage * acceleration);

        shootingCooldown -= Time.deltaTime;
    }

    public void UpdateFriction(float friction) {
        rb.drag = typicalDrag * (1 + friction);
    }

    public void CanShoot(bool canShoot) {
        this.canShoot = canShoot;
    }

    public int GetHealth() {
        return health;
    }

    public int GetAmmo() {
        return ammo;
    }

    public void Heal() {
        health = 100;
    }

    public void Damage() {
        health -= 20;
        if (health >= 0) audioSource.PlayOneShot(hurt);
    }

    public void Refill(int ammo) {
        if (ammo != this.ammo) {
            audioSource.PlayOneShot(reload);
            this.ammo = ammo;
        }
    }

    public void UpgradeGun() {
        gunUpgrade = true;
        shootingInterval = 0.4f;
    }

    public void UpgradeCar() {
        carUpgrade = true;
        typicalDrag = 3f;
        rb.drag = typicalDrag;
    }

    public void SetAmmo(int ammo) {
        this.ammo = ammo;
    }

    public void SetHealth(int health) {
        this.health = health;
    }
}
