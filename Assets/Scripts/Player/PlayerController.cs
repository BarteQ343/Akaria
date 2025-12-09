using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class PlayerController : MonoBehaviour
{
	public float maxSpeed = 3.4f; //default 3.4f
	public float jumpHeight = 6.5f; //default 6.5f
	public float gravityScale = 1.5f; //default 1.5f
	public GameObject mainCamera;
	Camera cameraComponent; 
	private Vector3 camOffset = new Vector3(0f, 0.6f, -10f);
	private float camSmoothTime = 0.25f;
	private Vector3 camVelocity = Vector3.zero;
	[Header("Materia³y do poprawnej kolizji ze œcianami")]
	[SerializeField]
	private PhysicsMaterial2D Slidey;
	[SerializeField]
	private PhysicsMaterial2D NotSlidey;
	private bool facingRight = true;
	float moveDirection = 0;
	bool isGrounded = false;
	bool isHittingWall = false;
	Rigidbody2D r2d;
	CapsuleCollider2D mainCollider;
	Animator anim;
	Transform t;
	private bool hasDJumped = false;
	private float airControlSpeed = 0.4f;
	private float raycastDistance = 0.1f;
	[Header("Warstwa dla schodów")]
	[SerializeField]
	private LayerMask stairMask;
	[Header("Warstwa pod³o¿a (Ground)")]
	public LayerMask layerMask;
	private Attack _attack;
	private DustController dustEffect;
	private float dashRate = 2f;
	private float nextDashTime = 0f;

	// Start is called before the first frame update
	void Start()
	{
		dustEffect = GetComponent<DustController>();
		_attack = GetComponent<Attack>();
		t = transform;
		r2d = GetComponent<Rigidbody2D>();
		mainCollider = GetComponent<CapsuleCollider2D>();
		anim = GetComponent<Animator>();
		r2d.freezeRotation = true;
		r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		r2d.gravityScale = gravityScale;
		facingRight = t.localScale.x > 0;
		cameraComponent = mainCamera.GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update()
	{
		// Movement controls
		if (((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))))
		{
			moveDirection = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? -1 : 1;
			
		} else
		{
			if (isGrounded || r2d.velocity.magnitude < 0.01f)
			{
				moveDirection = 0;
			} 
		}
		// Change facing direction
		if (moveDirection != 0)
		{
			if (moveDirection > 0 && !facingRight)
			{
				facingRight = true;
				t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
			}
			if (moveDirection < 0 && facingRight)
			{
				facingRight = false;
				t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
			}
			// Apply movement velocity
			if (!isGrounded)
			{
				// If not grounded, apply air control only if the desired velocity is different
				float targetVelocityX = moveDirection * maxSpeed;
				if (Mathf.Abs(targetVelocityX - r2d.velocity.x) > airControlSpeed)
				{
					r2d.velocity = new Vector2(Mathf.MoveTowards(r2d.velocity.x, targetVelocityX*0.6f, airControlSpeed), r2d.velocity.y);
				}
			}
			else
			{
				r2d.velocity = new Vector2(moveDirection * maxSpeed, r2d.velocity.y);
			}
		}
		// Jumping

		if (Time.time >= nextDashTime)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                r2d.constraints = RigidbodyConstraints2D.FreezeAll;
                t.position += new Vector3(facingRight ? 1.5f : -1.5f, 0, 0);
                if (gameObject.name == "Player")
				{
                    anim.SetTrigger("Dash");
                } else
                {
					anim.ResetTrigger("run");
                    anim.SetTrigger("dash");
                }
				nextDashTime = Time.time + 1f / dashRate;
			}

		}

		if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && (!hasDJumped || isGrounded))
		{
			r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
			if (!isGrounded)
			{
				hasDJumped = true;
			}
		}
		if (isGrounded)
		{
			hasDJumped = false;
		}
		if (isHittingWall)
		{
			r2d.sharedMaterial = Slidey;
		}
		if (!isHittingWall)
		{
			r2d.sharedMaterial = NotSlidey;
		}

	}

	void FixedUpdate()
	{

		// Camera follow
		if (mainCamera)
		{
			camOffset.x = (cameraComponent.ScreenToWorldPoint(Input.mousePosition).x - t.position.x) * 0.1f;
			camOffset.y = ((cameraComponent.ScreenToWorldPoint(Input.mousePosition).y - t.position.y) * 0.1f) + 0.7f;
			Vector3 targetPosition = t.position + camOffset;
			mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref camVelocity, camSmoothTime);
		}

		bool wasGrounded = isGrounded;
		mainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		isGrounded = false;
		RaycastHit2D hitDown = Physics2D.Raycast(transform.position + new Vector3(0.2f,0,0), Vector2.down, raycastDistance, layerMask);
		RaycastHit2D hitDown2 = Physics2D.Raycast(transform.position + new Vector3(-0.2f, 0, 0), Vector2.down, raycastDistance, layerMask);
		RaycastHit2D hitStairs = Physics2D.Raycast(transform.position + new Vector3(0.2f, 0, 0), Vector2.down, raycastDistance, stairMask);
		RaycastHit2D hitStairs2 = Physics2D.Raycast(transform.position + new Vector3(-0.2f, 0, 0), Vector2.down, raycastDistance, stairMask);
		RaycastHit2D hitSide = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+0.15f, layerMask);
		RaycastHit2D hitSide1 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.15f, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+ 0.15f, layerMask);
		RaycastHit2D hitSide2 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.3f, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+ 0.15f, layerMask);
		RaycastHit2D hitSide3 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.45f, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+ 0.15f, layerMask);
		RaycastHit2D hitSide4 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+ 0.15f, layerMask);
		RaycastHit2D hitSide5 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.75f, 0), facingRight ? Vector2.right : Vector2.left, raycastDistance+ 0.15f, layerMask);
		mainCollider.gameObject.layer = LayerMask.NameToLayer("Player");
		if (hitDown.collider != null
			|| (hitDown2.collider != null 
				)
			)
		{
			isGrounded = true;   
		}
		if (wasGrounded != isGrounded)
		{
			if (isGrounded)
			{
				// Landed !!!WIP/NEED FEEDBACK!!!
				if (gameObject.name == "Tavor")
                {
                    anim.ResetTrigger("fall");
					anim.SetTrigger("land");
                }
			}
			else
			{
				// Jumped
				dustEffect.CreateDust("Jump");
				
			}
		}
		if ((hitSide.collider != null) || 
			(hitSide1.collider != null) || 
			(hitSide2.collider != null) || 
			(hitSide3.collider != null) || 
			(hitSide4.collider != null) || 
			(hitSide5.collider != null))
		{
			isHittingWall = true;
		} else
		{
			isHittingWall = false;
		}
		if (isGrounded)
		{
			if (hitStairs.collider != null || hitStairs2.collider != null)
			{
				r2d.velocity -= new Vector2(0,0);
			} else
			{
				r2d.velocity += new Vector2(0, -0.2f);
			}
		}
		// Handle animations for MC
		if (gameObject.name == "Player")
		{
            if ((r2d.velocity.x > 0.01f || r2d.velocity.x < -0.01f) && isGrounded && !_attack.attackIsHappening)
            {
                anim.ResetTrigger("Jump");
                anim.ResetTrigger("Fall");
                anim.ResetTrigger("Stop");
                //anim.ResetTrigger("Run");
                anim.SetTrigger("Walk");
            }
            if ((r2d.velocity.x > -0.001f && r2d.velocity.x < 0.001f) && isGrounded && (anim.GetBool("Hurt") == false || anim.GetBool("Die") == false))
            {
                /*if ((anim.GetBool("Jump") || anim.GetBool("Fall") || anim.GetBool("Walk"))
                    || (!anim.GetBool("Run") && !anim.GetBool("Stop") && !anim.GetBool("Jump") && !anim.GetBool("Fall") && !anim.GetBool("Attack") && !anim.GetBool("Die") && !anim.GetBool("Hurt") && !anim.GetBool("Attack2") && !anim.GetBool("Attack3") && !anim.GetBool("AirAttack") && !anim.GetBool("Walk")))
                */
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump") || anim.GetCurrentAnimatorStateInfo(0).IsName("Fall") || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                {
                    anim.ResetTrigger("Jump");
                    anim.ResetTrigger("Run");
                    anim.ResetTrigger("Fall");
                    anim.ResetTrigger("Walk");
                    anim.SetTrigger("Stop");
                }
            }
            if (r2d.velocity.y > 0 && !isGrounded && !anim.GetBool("Attack") && (hitStairs.collider == null && hitStairs2.collider == null))
            {
                anim.ResetTrigger("Fall");
                anim.ResetTrigger("Run");
                anim.ResetTrigger("Stop");
                anim.SetTrigger("Jump");
            }
            if (r2d.velocity.y < 0 && !isGrounded && !anim.GetBool("Attack") && (hitStairs.collider == null && hitStairs2.collider == null))
            {
                anim.ResetTrigger("Fall");
                anim.ResetTrigger("Run");
                anim.ResetTrigger("Stop");
                anim.SetTrigger("Fall");
            }
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("AirAttack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
            {
                r2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            }
        } 
		if (gameObject.name == "Tavor")
		{
            if ((r2d.velocity.x > 0.01f || r2d.velocity.x < -0.01f) && isGrounded && !_attack.attackIsHappening)
            {
                anim.ResetTrigger("jump");
                anim.ResetTrigger("fall");
                anim.ResetTrigger("idle");
                //anim.ResetTrigger("Run");
                anim.SetTrigger("run");
            }
            if ((r2d.velocity.x > -0.001f && r2d.velocity.x < 0.001f) && isGrounded && (anim.GetBool("take_damage") == false || anim.GetBool("Die") == false))
            {
                /*if ((anim.GetBool("Jump") || anim.GetBool("Fall") || anim.GetBool("Walk"))
                    || (!anim.GetBool("Run") && !anim.GetBool("Stop") && !anim.GetBool("Jump") && !anim.GetBool("Fall") && !anim.GetBool("Attack") && !anim.GetBool("Die") && !anim.GetBool("Hurt") && !anim.GetBool("Attack2") && !anim.GetBool("Attack3") && !anim.GetBool("AirAttack") && !anim.GetBool("Walk")))
                */
                if ((anim.GetCurrentAnimatorStateInfo(0).IsName("jump") || anim.GetCurrentAnimatorStateInfo(0).IsName("run")) && !anim.GetCurrentAnimatorStateInfo(0).IsName("land"))
                {
                    anim.ResetTrigger("jump");
                    anim.ResetTrigger("run");
                    anim.ResetTrigger("fall");
                    anim.SetTrigger("idle");
                }
            }
            if (r2d.velocity.y > 0 && !isGrounded && !anim.GetBool("attack1") && (hitStairs.collider == null && hitStairs2.collider == null))
            {
                anim.ResetTrigger("fall");
                anim.ResetTrigger("run");
                anim.ResetTrigger("idle");
                anim.SetTrigger("jump");
            }
            if (r2d.velocity.y < 0 && !isGrounded && !anim.GetBool("attack1") && (hitStairs.collider == null && hitStairs2.collider == null))
            {
                anim.ResetTrigger("jump");
                anim.ResetTrigger("run");
                anim.ResetTrigger("idle");
                anim.SetTrigger("fall");
            }
            /*if (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("attack2") && !anim.GetCurrentAnimatorStateInfo(0).IsName("attack3") && !anim.GetCurrentAnimatorStateInfo(0).IsName("dash"))
            {
				ResetLock();
            }*/
        }
	}
	void ResetHurt()
	{
		if (gameObject.name == "Player")
		{
            if (r2d.velocity.x == 0 && isGrounded && !anim.GetBool("Die"))
            {
                anim.ResetTrigger("Hurt");
                anim.SetTrigger("Stop");
            }
        }
		if (gameObject.name == "Tavor")
		{
            if (r2d.velocity.x == 0 && isGrounded && !anim.GetBool("Die"))
            {
                anim.ResetTrigger("take_damage");
                anim.SetTrigger("idle");
            }
        }
	}
	void ResetLock()
	{
		r2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
	}
	public bool checkIsGrounded()
	{
		return isGrounded;
	}

	public bool facingDirection()
	{
		return facingRight;
	}
	void resetLand()
	{
		anim.ResetTrigger("dash");
		anim.ResetTrigger("land");
		anim.SetTrigger("idle");
	}
}