using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Zombie : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    private State state;
    private int health = 5;

    [SerializeField]
    GameManager gm;

    public enum State {
        Idle,
        Active
    }

    public void Init(Transform playerTarget, GameManager gm) {
        player = playerTarget;
        this.gm = gm;
        state = State.Active;
    }

    void Start() {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.isStopped = true;

        state = State.Active;
    }

    void Update() {
        if (state == State.Active) {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        } else {
            agent.isStopped = true;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Bullet") {
            health -= 1;
        } else if(other.gameObject.tag == "Player") {
            gm.ZombieDeath();
            Destroy(gameObject);
        }

        if(health <= 0) {
            gm.ZombieDeath();
            Destroy(gameObject);
        }
    }
}

