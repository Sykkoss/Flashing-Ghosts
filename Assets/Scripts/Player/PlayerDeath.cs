﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public AudioClip[] scaredSounds;
    public AudioClip[] deathSounds;
    public float scaredTime;
    public bool playerIsScared; // Will allow Boos and spawners to pause when player is scared
    public bool playerIsDead; // Will allow Boos and spawners to stop when player is dead
    /* Camera Zoom */
    public float minCameraZoom;
    public float maxCameraZoom;
    public float cameraZoomSpeed;
    /* Invincible */
    public float invincibleTime;
    public float flashingInvincibleRate; // Should be lower than 'invincibleTime'

    private bool isInvincible;


    private Animator animator;
    private Flashlight flashlightManager;
    private PlayerInventory playerInventory;
    private PlayerMovement playerMovement;
    private AudioSource audioSource;

    private void Start()
    {
        playerIsScared = false;
        playerIsDead = false;
        isInvincible = false;
        flashlightManager = GetComponent<Flashlight>();
        animator = GetComponent<Animator>();
        playerInventory = GetComponent<PlayerInventory>();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isInvincible && collision.CompareTag("Enemy"))
        {
            playerIsScared = true;
            flashlightManager.SetIsScared(true); // Disable flashlight control
            playerMovement.SetIsScared(true); // Disable movement
            AnimatorSetBool("Scared");
            playerInventory.LooseHP(1);
            collision.GetComponent<Enemy>().AttackPlayer();
            isInvincible = true;
            StartCoroutine(ZoomInCameraToPlayer(maxCameraZoom)); // Zoom camera to player
            if (playerInventory.lifes > 0)
                StartCoroutine(ScaredTimer()); // Exit "Scared" mode after 'scaredTime'
            else
            {
                playerIsDead = true;
                audioSource.clip = deathSounds[Random.Range(0, deathSounds.Length)]; // Play a random sound from death sounds
                audioSource.Play();
            }
        }
    }

    private IEnumerator ScaredTimer()
    {
        audioSource.clip = scaredSounds[Random.Range(0, scaredSounds.Length)]; // Play a random sound from scared sounds
        audioSource.Play();
        yield return new WaitForSeconds(scaredTime); // Wait for player not to be scared again
        AnimatorSetBool("Left");
        flashlightManager.SetIsScared(false); // Enable flashlight control
        playerMovement.SetIsScared(false); // Enable movements
        playerIsScared = false;
        StartCoroutine(ZoomOutCamera()); // Zoom out camera to default position and zooms
        StartCoroutine(Invincible()); // Flash Sprite when player is invincible
    }

    private IEnumerator ZoomInCameraToPlayer(float orthographicSize)
    {
        float timeGoal = Time.time + scaredTime;
        Vector3 playerPosition;

        while (Time.time < timeGoal)
        {
            playerPosition = transform.position;
            playerPosition.z = Camera.main.transform.position.z;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerPosition, Time.deltaTime * cameraZoomSpeed);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, orthographicSize, Time.deltaTime * cameraZoomSpeed);
            yield return new WaitForSeconds(0f);
        }
    }

    private IEnumerator ZoomOutCamera()
    {
        Vector3 zeroPos = Vector3.zero;
        float timeGoal = Time.time + invincibleTime;

        zeroPos.z = Camera.main.transform.position.z;
        while (Time.time < timeGoal)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, zeroPos, Time.deltaTime * cameraZoomSpeed);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, minCameraZoom, Time.deltaTime * cameraZoomSpeed);
            yield return new WaitForSeconds(0f);
        }
        Camera.main.transform.position = new Vector3(0f, 0f, zeroPos.z);
        Camera.main.orthographicSize = minCameraZoom;
    }

    private IEnumerator Invincible()
    {
        float timeGoal = Time.time + invincibleTime;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        while (spriteRenderer != null && Time.time < timeGoal)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashingInvincibleRate);
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void AnimatorSetBool(string boolToSet)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            animator.SetBool(parameter.name, false);
        }
        animator.SetBool(boolToSet, true);
    }
}
