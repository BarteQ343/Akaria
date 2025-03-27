using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WalkingEnemyController : MonoBehaviour
{
	public float maxSpeed = 1.4f;
	public float jumpHeight = 4.5f;
	public float gravityScale = 1.5f;

	[SerializeField]
	private PhysicsMaterial2D Slidey;
	[SerializeField]
	private PhysicsMaterial2D NotSlidey;
	public bool facingRight = true;
	Collider2D mainCollider;
	Rigidbody2D r2d;
	Transform t;
	Transform playerTransform;
	Animator anim;
	public bool isGrounded = false;
	bool isJumping = false;
	float obstacleCheckDistance = 0.5f;
	float raycastDistance = 1f;
	public LayerMask playerLayer;
	Vector3 originalPosition;
	[SerializeField]
	public RaycastHit2D hitFront;
	[SerializeField]
	public RaycastHit2D hitPlayer;
	CapsuleCollider2D player;
	PlayerHealthController playerHealthController;
	private int goBackCounter = 0;
	float playerAwerness;
	[SerializeField] float playerAwernessIdle = 1;
	[SerializeField] float playerAwernessHunting = 2;
	[SerializeField] float detectionDistance = 2.5f;
	HealthController healthController;
	enum EnemyState
	{
		Idle,
		FollowPlayer,
		ReturnToOriginalPosition,
		Attack,
		Dead
	}
	EnemyState currentState;

	void Start()
	{
		healthController = GetComponent<HealthController>();
		playerHealthController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealthController>();
		mainCollider = GetComponent<Collider2D>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<CapsuleCollider2D>();
		t = transform;
		r2d = GetComponent<Rigidbody2D>();
		r2d.freezeRotation = true;
		r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		r2d.gravityScale = gravityScale;
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		originalPosition = t.position;
		anim = GetComponent<Animator>();
		playerAwerness = playerAwernessIdle;
	}
	public static void DrawDebugCircle(Vector3 center, float radius, int segments, Color color, float duration = 0, bool depthTest = true)
	{
		float angleStep = 360.0f / segments;
		Vector3 prevPoint = center + new Vector3(radius, 0, 0);
		Vector3 newPoint = Vector3.zero;

		for (int i = 1; i <= segments; i++)
		{
			float angle = angleStep * i * Mathf.Deg2Rad;
			newPoint.x = center.x + Mathf.Cos(angle) * radius;
			newPoint.y = center.y + Mathf.Sin(angle) * radius;
			newPoint.z = center.z;

			Debug.DrawLine(prevPoint, newPoint, color, duration, depthTest);
			prevPoint = newPoint;
		}
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.L)) {
			print(currentState);
		}

		mainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		Vector3 RaycastPos = new Vector3(t.position.x, t.position.y + 0.7f, t.position.z);
		Vector3 RaycastPosLow = new Vector3(t.position.x, t.position.y + 0.05f, t.position.z);
		CheckGrounded();
		// Check for obstacles and player
		RaycastHit2D hitSide = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0, 0), facingRight ? Vector2.right : Vector2.left, 0.2f);
		RaycastHit2D hitFront = Physics2D.Raycast(RaycastPosLow, facingRight ? Vector2.right : Vector2.left, obstacleCheckDistance);
		mainCollider.gameObject.layer = LayerMask.NameToLayer("Enemies");
		Debug.DrawLine(RaycastPos + new Vector3(facingRight ? -0.7f : 0.7f, 0, 0), (RaycastPos + new Vector3(facingRight ? -0.7f : 0.7f, 0, 0)) + (facingRight ? Vector3.right : Vector3.left) * 1.5f, Color.blue);
		Debug.DrawLine(RaycastPosLow, RaycastPosLow + (facingRight ? Vector3.right : Vector3.left) * obstacleCheckDistance, Color.blue);
		Collider2D[] checkPlayer = Physics2D.OverlapCircleAll(t.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), detectionDistance * playerAwerness, playerLayer);
		Collider2D[] checkPlayerClose = Physics2D.OverlapCircleAll(t.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), 0.4f, playerLayer);
		DrawDebugCircle(t.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), detectionDistance * playerAwerness, 32, Color.red);
		DrawDebugCircle(t.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), 0.4f, 32, Color.red);
		

		if (hitSide.collider != null)
		{
			r2d.sharedMaterial = Slidey;
		} else
		{
			r2d.sharedMaterial = NotSlidey;
		}
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
		{
			currentState = EnemyState.Dead;
		}
		switch (currentState)
		{
			case EnemyState.Idle:
				//print("I'm just krillin'");
					if (checkPlayer.Length > 0)
					{
						currentState = EnemyState.FollowPlayer;
						//print("Following after idling");
					}
					else
					{
						playerAwerness = playerAwernessIdle;
						Idle();
					}
				StopCoroutine(WaitBeforeReturning());
				break;

			case EnemyState.FollowPlayer:
				//print("Get that mf");
				foreach (Collider2D collider in checkPlayer)
				{
					// Check if the collider belongs to the player
					if (collider.CompareTag("Player"))
					{
						playerAwerness = playerAwernessHunting;
						goBackCounter = 0;
						StopCoroutine(WaitBeforeReturning());
						if ((!facingRight && playerTransform.position.x > t.position.x) || (facingRight && playerTransform.position.x < t.position.x))
						{
							TurnAround();
						}
						if (hitFront.collider == null || hitFront.collider == player)
						{
							Move(facingRight ? 1 : -1);
						}
						else if (hitFront.collider != player && !isJumping && isGrounded)
						{
							Jump();
						}
					}
					if (checkPlayerClose.Length > 0)
					{
						currentState = EnemyState.Attack;
					}
				}
				if (checkPlayer.Length == 0 && goBackCounter == 0)
				{
					//print("Where'd he go?");
					StartCoroutine(WaitBeforeReturning());
					goBackCounter++;
				}
				break;

			case EnemyState.ReturnToOriginalPosition:
				//print("Fuck go back");
				if (checkPlayer.Length > 0 && checkPlayerClose.Length <= 0) 
				{ 
					currentState = EnemyState.FollowPlayer;
					//print("Following instead of returning");
				} else
				{
					ComeBack(hitFront.collider);
					if (Mathf.Abs(t.position.x - originalPosition.x) < 0.1f)
					{
						currentState = EnemyState.Idle;
					}
				}
				break;
			case EnemyState.Attack:
				//print("Rattle'em boys!");
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
				{
					anim.SetBool("Idle", false);
					anim.SetBool("Walk", false);
					anim.SetTrigger("Attack1");
				}
				StopCoroutine(WaitBeforeReturning());
				break;
			case EnemyState.Dead:
				StopAllCoroutines();
				StartCoroutine(healthController.Despawn());
				break;
		}
	}
	void CheckGrounded()
	{
		RaycastHit2D hitDown = Physics2D.Raycast(transform.position + new Vector3(0.2f, 0, 0), Vector2.down, raycastDistance);
		RaycastHit2D hitDown2 = Physics2D.Raycast(transform.position + new Vector3(-0.2f, 0, 0), Vector2.down, raycastDistance);
		//print(hitDown.collider);
		isGrounded = hitDown.collider != null || hitDown2.collider != null;
	} 

	void Move(int direction)
	{
		if (r2d.velocity.x > 0.01f || r2d.velocity.x < -0.01f)
		{
			anim.ResetTrigger("Attack1");
			anim.ResetTrigger("Attack2");
			anim.ResetTrigger("Idle");
			anim.SetTrigger("Walk");
		}
		r2d.velocity = new Vector2(direction * maxSpeed, r2d.velocity.y);
	}

	void TurnAround()
	{
		facingRight = !facingRight;
		t.localScale = new Vector3(facingRight ? (-1 * t.localScale.x) : (-1 * t.localScale.x), t.localScale.y, t.localScale.z);
	}

	void Idle()
	{
		if (r2d.velocity.x < 0.01f || r2d.velocity.x > -0.01f)
		{
			anim.ResetTrigger("Attack1");
			anim.ResetTrigger("Attack2");
			anim.ResetTrigger("Walk");
			anim.SetTrigger("Idle");
		}
	}
	
	void ComeBack(Collider2D frontCast)
	{
		if ((facingRight && t.position.x > originalPosition.x) || (!facingRight && t.position.x < originalPosition.x))
		{
			TurnAround();
		}
		if (frontCast == null || frontCast == player && ((!facingRight && t.position.x > originalPosition.x) || (facingRight && t.position.x < originalPosition.x)))
		{
			Move(facingRight ? 1 : -1);
		} else if (frontCast != player && !isJumping && isGrounded)
		{
			Jump();
		}
	}

	void Jump()
	{
		r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
		Move(facingRight ? 1 : -1);
		isJumping = true;
		StartCoroutine(ResetJump());
	}
	IEnumerator ResetJump()
	{
		yield return new WaitForSeconds(1f); // Adjust jump cooldown as needed
		isJumping = false;
	}
	IEnumerator WaitBeforeReturning()
	{
		Idle();
		yield return new WaitForSeconds(3f); // Adjust waiting time as needed
		currentState = EnemyState.ReturnToOriginalPosition;
	}
	void CheckForHit()
	{
		Vector3 RaycastPos = new Vector3(t.position.x, t.position.y + 0.7f, t.position.z);
		RaycastHit2D attackPlayer = Physics2D.Raycast(RaycastPos, facingRight ? Vector2.right : Vector2.left, 1, playerLayer);
		Debug.DrawLine(RaycastPos, RaycastPos + (facingRight ? Vector3.right : Vector3.left) * 1, Color.red);
		if (attackPlayer.collider == player)
		{
			playerHealthController.TakeDamage(10);
			anim.ResetTrigger("Attack1");
			anim.SetTrigger("Attack2");
		} else if (attackPlayer.collider != player)
		{
			anim.ResetTrigger("Attack1");
			anim.ResetTrigger("Attack2");
			if (!anim.GetBool("Walk"))
			{
				anim.SetTrigger("Idle");
			}
			if (Physics2D.OverlapCircleAll(transform.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), 0.4f, playerLayer).Length <= 0)
			{
				currentState = EnemyState.FollowPlayer;
				//print("Following after missing");
			}
		}
	}
	void CheckForHit2()
	{
		Vector3 RaycastPos = new Vector3(t.position.x, t.position.y + 0.7f, t.position.z);
		RaycastHit2D attackPlayer = Physics2D.Raycast(RaycastPos, facingRight ? Vector2.right : Vector2.left, 1, playerLayer);
		Debug.DrawLine(RaycastPos, RaycastPos + (facingRight ? Vector3.right : Vector3.left) * 1, Color.red);
		if (attackPlayer.collider == player)
		{
			playerHealthController.TakeDamage(10);
			anim.ResetTrigger("Attack2");
			anim.SetTrigger("Attack1");
		}
		else if (attackPlayer.collider != player)
		{
			anim.ResetTrigger("Attack1");
			anim.ResetTrigger("Attack2");
			if (!anim.GetBool("Walk"))
			{
				anim.SetTrigger("Idle");
			}
			if (Physics2D.OverlapCircleAll(transform.position + new Vector3(facingRight ? 0.3f : -0.3f, 0.5f, 0), 0.4f, playerLayer).Length <= 0)
			{
				currentState = EnemyState.FollowPlayer;
			}
		}
	}
}