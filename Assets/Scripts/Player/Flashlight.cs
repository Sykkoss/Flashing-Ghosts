﻿using System.Collections;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public float flashKnockbackForce;
    public float flashKnockbackSpeed;

    private bool isScared;
    private bool isFlashing;
    private Animator animator;

    /* Lights */
    public float maxConeLightIntensity;
    public float maxSpotLightIntensity;

    private GameObject lightsGO;
    private Light coneLight;
    private Light spotLight;
    /* Lights */

    private void Start()
    {
        isScared = false;
        isFlashing = false;
        animator = GetComponent<Animator>();
        lightsGO = transform.GetChild(0).gameObject;
        coneLight = lightsGO.transform.GetChild(0).GetComponent<Light>();
        spotLight = lightsGO.transform.GetChild(1).GetComponent<Light>();
    }

    void Update()
    {
        if (!isScared)
        {
            if (!isFlashing && Input.GetMouseButtonDown(0))
                StartCoroutine(Flash());
            RotateLights();
            RotatePlayer();
        }
    }

    private IEnumerator Flash()
    {
        float coneLightNormalIntensity = coneLight.intensity;
        float spotLightNormalIntensity = spotLight.intensity;

        isFlashing = true;
        //// Idle animation
        //// Disable movement
        //// Play light load up sound
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(PlayerKnockBack()); // Push player opposite from mouse (flash knockback)
        coneLight.intensity = maxConeLightIntensity;
        spotLight.intensity = maxSpotLightIntensity;
        KillGhostsInRange(); // Kill ghosts in flashlight range
        StartCoroutine(DecreaseLightsIntensity(coneLightNormalIntensity, spotLightNormalIntensity));
    }

    private IEnumerator PlayerKnockBack()
    {
        Vector2 finalKnockbackPoint = GetPointRelativeToLightsRotation(false, flashKnockbackForce);
        Vector3 newPos = new Vector3(finalKnockbackPoint.x, finalKnockbackPoint.y, transform.position.z);

        while (Vector3.Distance(transform.position, newPos) > 1f)
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * flashKnockbackSpeed);
            yield return new WaitForSeconds(0f);
        }
    }

    private void KillGhostsInRange()
    {
        Collider2D[] ghostsInRange = GetEnemiesInRange(); // Get ghosts in flashlight range

        foreach (Collider2D ghost in ghostsInRange)
        {
            if (ghost != null)
                Destroy(ghost.gameObject); //// Should call a ghost method for it to die (animation and all)
        }
    }

    private Collider2D[] GetEnemiesInRange()
    {
        Collider2D[] enemiesColliders;
        Vector2 boxSize = new Vector2(5.3f, 1.7f); // The box will be 5.3f wide (x) and 1.7f height (y)
        Vector2 boxPosition = GetPointRelativeToLightsRotation(true, 3f); // 3f corresponds to a point, with the given size (boxSize(5.3, 1.7)), that is right in front of flashlight

        // Cast a box at given position and return all colliders detected on specific mask
        enemiesColliders = Physics2D.OverlapBoxAll(boxPosition, boxSize, lightsGO.transform.rotation.eulerAngles.z + 180f, LayerMask.GetMask("Enemy"));
        return enemiesColliders;
    }

    public Vector2 GetPointRelativeToLightsRotation(bool getFrontPoint, float offsetFromPlayer)
    {
        float angleOffset = (getFrontPoint) ? (180f) : (0f); // If front point wanted, adds 180° to actual lights rotation
        float lightsRotationInRadians = ((lightsGO.transform.rotation.eulerAngles.z + angleOffset) * Mathf.PI) / 180f;
        Vector2 boxPosition = new Vector2(
            transform.position.x + (offsetFromPlayer * Mathf.Cos(lightsRotationInRadians)),
            transform.position.y + (offsetFromPlayer * Mathf.Sin(lightsRotationInRadians))
        ); // Get boxPosition thanks to flashlight rotation

        return boxPosition;
    }

    private IEnumerator DecreaseLightsIntensity(float coneLightNormalIntensity, float spotLightNormalIntensity)
    {
        float flashOutSpeed = 5f;

        yield return new WaitForSeconds(0.3f);
        while (coneLight.intensity > (coneLightNormalIntensity + 10f) || spotLight.intensity > (spotLightNormalIntensity + 0.2f))
        {
            coneLight.intensity = Mathf.Lerp(coneLight.intensity, coneLightNormalIntensity, Time.deltaTime * flashOutSpeed);
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, spotLightNormalIntensity, Time.deltaTime * flashOutSpeed);
            yield return new WaitForSeconds(0f);
        }
        coneLight.intensity = coneLightNormalIntensity;
        spotLight.intensity = spotLightNormalIntensity;
        isFlashing = false;
    }

    public void SetIsScared(bool newIsScared)
    {
        if (newIsScared)
            lightsGO.SetActive(false); // Disable flashlight when scared
        else if (!newIsScared)
            lightsGO.SetActive(true);
        isScared = newIsScared;
    }

    private void RotatePlayer()
    {
        float lightsRotation = lightsGO.transform.eulerAngles.z;

        if (lightsRotation <= 45 || lightsRotation > 315)
            SetDirectionInAnimator("Left");
        else if (lightsRotation > 45 && lightsRotation <= 135)
            SetDirectionInAnimator("Down");
        else if (lightsRotation > 135 && lightsRotation <= 225)
            SetDirectionInAnimator("Right");
        else if (lightsRotation > 225 && lightsRotation <= 315)
            SetDirectionInAnimator("Up");
    }

    private void SetDirectionInAnimator(string direction)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            animator.SetBool(parameter.name, false);
        }
        animator.SetBool(direction, true);
    }

    private void RotateLights()
    {
        float newAngle;
        Vector3 newRotation = new Vector3(0, 0);

        newAngle = GetNewAngle();
        newRotation.z = newAngle - 180; // Allows to face mouse position instead of being at the opposite
        lightsGO.transform.eulerAngles = new Vector3(0, 0, newAngle - 180);
    }

    private float GetNewAngle()
    {
        Vector3 object_pos;
        Vector3 mouse_pos;

        mouse_pos = Input.mousePosition;
        object_pos = Camera.main.WorldToScreenPoint(lightsGO.transform.position);
        mouse_pos.x -= object_pos.x;
        mouse_pos.y -= object_pos.y;
        return Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
    }
}
