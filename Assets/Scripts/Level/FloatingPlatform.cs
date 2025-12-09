using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingSinkingPlatform : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float frequency = 1f;

    public float sinkDelay = 3f;
    public float sinkWhileOnPlatform = 5f;
    public float sinkSpeed = 0.5f;
    public float sinkDepth = 3f;
    public float waitAtBottom = 2f;
    public float riseDuration = 0.5f;

    private Vector3 startPos;
    private float currentX;
    private float currentY;

    private bool playerOnPlatform = false;
    private bool isSinking = false;
    private bool isRising = false;
    private bool allowHorizontalMovement = true;

    private float onPlatformTimer = 0f;
    private Coroutine sinkCoroutine;

    private float riseTimer = 0f;
    private float sinkEndY;

    private float timeOffset;

    void Start()
    {
        startPos = transform.position;
        currentX = startPos.x;
        currentY = startPos.y;
        timeOffset = Time.time;
    }

    void Update()
    {
        if (allowHorizontalMovement)
        {
            float t = Time.time - timeOffset;
            currentX = startPos.x + amplitude * Mathf.Sin(t * frequency);
        }

        if (!isSinking && !isRising)
        {
            if (playerOnPlatform)
            {
                onPlatformTimer += Time.deltaTime;
                if (onPlatformTimer >= sinkWhileOnPlatform)
                {
                    StartSinking();
                }
            }
            else
            {
                onPlatformTimer = 0f;
            }
        }
        else if (isSinking)
        {
            float targetY = startPos.y - sinkDepth;
            currentY = Mathf.MoveTowards(currentY, targetY, sinkSpeed * Time.deltaTime);

            if (Mathf.Abs(currentY - targetY) < 0.01f)
            {
                isSinking = false;
                StartCoroutine(RiseAfterDelay());
            }
        }
        else if (isRising)
        {
            riseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(riseTimer / riseDuration);
            currentY = Mathf.Lerp(sinkEndY, startPos.y, t);

            if (t >= 1f)
            {
                isRising = false;
                onPlatformTimer = 0f;
                allowHorizontalMovement = true;

                timeOffset = Time.time - Mathf.Asin((currentX - startPos.x) / amplitude) / frequency;
            }
        }

        transform.position = new Vector3(currentX, currentY, startPos.z);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;
            collision.transform.SetParent(transform);

            if (sinkCoroutine != null)
            {
                StopCoroutine(sinkCoroutine);
                sinkCoroutine = null;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
            collision.transform.SetParent(null);

            if (!isSinking && !isRising)
            {
                sinkCoroutine = StartCoroutine(SinkAfterDelay());
            }

            onPlatformTimer = 0f;
        }
    }

    IEnumerator SinkAfterDelay()
    {
        yield return new WaitForSeconds(sinkDelay);

        if (!playerOnPlatform && !isSinking && !isRising)
        {
            StartSinking();
        }
    }

    IEnumerator RiseAfterDelay()
    {
        yield return new WaitForSeconds(waitAtBottom);

        isRising = true;
        riseTimer = 0f;
        sinkEndY = currentY;
    }

    void StartSinking()
    {
        isSinking = true;
        onPlatformTimer = 0f;
        sinkCoroutine = null;
        allowHorizontalMovement = false;

        timeOffset = Time.time - Mathf.Asin((currentX - startPos.x) / amplitude) / frequency;
    }
}
