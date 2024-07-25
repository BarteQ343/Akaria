using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformWithSlope : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private Collider2D solidCollider0;    // The solid collider of the platform
    [SerializeField]
    private Collider2D solidCollider1;
    [SerializeField]
    private float defaultImpassableThreshold = 2f; // Default passable threshold
    
    private float impassableThreshold; // Current passable threshold
    void Start()
    {
        impassableThreshold = defaultImpassableThreshold;
    }
    // Update is called once per frame
    void Update()
    {
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Set temporary passable threshold
                impassableThreshold = -2f;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Start a coroutine to reset the passable threshold after the duration
                StartCoroutine(ResetPassableThreshold());
            }
            // Check if the player is below the platform's passable threshold
            if (IsPlayerBelowThreshold())
            {
                // Disable the solid collider
                solidCollider0.enabled = false;
                solidCollider1.enabled = false;
            }
            else if (!IsPlayerBelowThreshold())
            {
                // Enable the solid collider
                solidCollider0.enabled = true;
                solidCollider1.enabled = true;
            }
        }

        bool IsPlayerBelowThreshold()
        {
            // Calculate the position of the player relative to the platform
            CapsuleCollider2D playerCollider = Player.GetComponent<CapsuleCollider2D>();
            Bounds playerBounds = playerCollider.bounds;
            float playerCenterY = playerBounds.center.y;

            Vector3 platformPosition = solidCollider0.transform.position;
            float platformCenterY = platformPosition.y;

            float playerRelativeY = playerCenterY - platformCenterY;

            // Check if the player is below the passable threshold
            return playerRelativeY > impassableThreshold;
        }

        IEnumerator ResetPassableThreshold()
        {
            // Wait for the specified duration
            yield return new WaitForSeconds(0.05f);

            // Reset passable threshold to default value
            impassableThreshold = defaultImpassableThreshold;
        }
    }
}
