using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    private GameObject Player;
    [SerializeField]
    private Collider2D solidCollider;    // The solid collider of the platform
    [SerializeField]
    private float defaultPassableThreshold = 0.5f; // Default passable threshold
    [SerializeField]
    private float temporaryThresholdDuration = 0.2f; // Duration for temporary passable threshold
    [SerializeField]
    private float passableThresholdValue = 2f; // Current passable threshold
    private float passableThreshold;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        passableThreshold = defaultPassableThreshold;
    }
    // Update is called once per frame
    void Update()
    {
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Set temporary passable threshold
                passableThreshold = passableThresholdValue;

                // Start a coroutine to reset the passable threshold after the duration
                StartCoroutine(ResetPassableThreshold());
            }
            // Check if the player is below the platform's passable threshold
            if (IsPlayerBelowThreshold())
            {
                // Disable the solid collider
                solidCollider.enabled = false;
            }
            else
            {
                // Enable the solid collider
                solidCollider.enabled = true;
            }
        }

        bool IsPlayerBelowThreshold()
        {
            // Calculate the position of the player relative to the platform
            CapsuleCollider2D playerCollider = Player.GetComponent<CapsuleCollider2D>();
            Bounds playerBounds = playerCollider.bounds;
            float playerCenterY = playerBounds.center.y;

            Vector3 platformPosition = transform.position;
            float platformCenterY = platformPosition.y;

            float playerRelativeY = playerCenterY - platformCenterY;
            //Debug.Log(playerRelativeY);

            // Check if the player is below the passable threshold
            return playerRelativeY <= passableThreshold;
        }

        IEnumerator ResetPassableThreshold()
        {
            // Wait for the specified duration
            yield return new WaitForSeconds(temporaryThresholdDuration);

            // Reset passable threshold to default value
            passableThreshold = defaultPassableThreshold;
        }
    }
}
