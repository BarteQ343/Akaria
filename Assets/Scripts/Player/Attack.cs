using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Attack : MonoBehaviour
{
	private Rigidbody2D r2d;
	private PlayerController _controller;
	Animator anim;
	[SerializeField]
	public LayerMask layerMask;
	CapsuleCollider2D mainCollider;
	HealthController healthController;
	[SerializeField]
	private int playerDamage;
	[HideInInspector]
	public bool attackCooldown;
	private int lastAttack = 0;
	[HideInInspector]
	public bool attackIsHappening = false;
	public float attackRate = 2f;
	float nextAttackTime = 0f;
	// Start is called before the first frame update
	void Start()
	{
		r2d = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		_controller = GetComponent<PlayerController>();
		mainCollider = GetComponent<CapsuleCollider2D>();
		attackCooldown = false;
	}

	// Update is called once per frame
	void Update()
	{
		/*if (Input.GetKey(KeyCode.R))
		{
			attackCooldown = false;
		}*/
		bool isGrounded = _controller.checkIsGrounded();
		if (Time.time >= nextAttackTime)
		{
			if (Input.GetKeyDown(KeyCode.Mouse0) && Time.timeScale > 0)
			{
				attackIsHappening = true;
				//attackCooldown = true;
				//StopCoroutine(ResetCooldown());
				nextAttackTime = Time.time + 1f / attackRate;
				if (!isGrounded)
				{
					r2d.constraints = RigidbodyConstraints2D.FreezeAll;
					anim.ResetTrigger("Fall");
					anim.ResetTrigger("Walk");
					anim.ResetTrigger("Run");
					anim.ResetTrigger("Jump");
					anim.ResetTrigger("Stop");
					anim.SetTrigger("AirAttack");
					lastAttack = 0;
					//StopCoroutine(ResetCooldown());
				}
				else if (isGrounded && (lastAttack == 0 || lastAttack == 3))
				{
					r2d.constraints = RigidbodyConstraints2D.FreezeAll;
					anim.ResetTrigger("Walk");
					anim.ResetTrigger("Run");
					anim.ResetTrigger("Stop");
					anim.SetTrigger("Attack");
					lastAttack = 1;
					//StopCoroutine(ResetCooldown());
				}
				else if (isGrounded && lastAttack == 1)
				{
					r2d.constraints = RigidbodyConstraints2D.FreezeAll;
					anim.ResetTrigger("Walk");
					anim.ResetTrigger("Run");
					anim.ResetTrigger("Stop");
					anim.ResetTrigger("Attack");
					anim.SetTrigger("Attack2");
					lastAttack = 2;
					//StopCoroutine(ResetCooldown());
				}
				else if (isGrounded && lastAttack == 2)
				{
					r2d.constraints = RigidbodyConstraints2D.FreezeAll;
					anim.ResetTrigger("Walk");
					anim.ResetTrigger("Run");
					anim.ResetTrigger("Stop");
					anim.ResetTrigger("Attack3");
					anim.SetTrigger("Attack3");
					lastAttack = 3;
					//StopCoroutine(ResetCooldown());
				}
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
				{
					lastAttack = 0;
				}
			}
		
		}
	}
	void ResetLastAttack()
	{
		lastAttack = 0;
	}
	void ResetAnims()
	{
		bool isGrounded = _controller.checkIsGrounded();
		if (isGrounded)
		{
			anim.ResetTrigger("AirAttack");
			anim.ResetTrigger("Fall");
			anim.ResetTrigger("Attack");
			anim.ResetTrigger("Attack2");
			anim.ResetTrigger("Attack3");
			anim.ResetTrigger("Jump");
			anim.ResetTrigger("Stop");
			anim.SetTrigger("Stop");
			attackIsHappening = false;
		}
		else
		{
			anim.ResetTrigger("AirAttack");
			anim.ResetTrigger("Attack");
			anim.ResetTrigger("Attack2");
			anim.ResetTrigger("Attack3");
			anim.ResetTrigger("Jump");
			anim.ResetTrigger("Stop");
			anim.SetTrigger("Fall");
			attackIsHappening = false;
		}
	}

	void ResetLock()
	{
		r2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
		//StartCoroutine(ResetCooldown());
	}
	public IEnumerator ResetCooldown()
	{
		yield return new WaitForSeconds(0.3f);
		attackCooldown = false;
	}
	void CheckHit()
	{
		attackCooldown = true;
		bool facingRight = _controller.facingDirection();
		mainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		RaycastHit2D hit1 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), facingRight ? new(1, 1) : new(-1, 1), 0.5f, layerMask);
		RaycastHit2D hit2 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), facingRight ? Vector2.right : Vector2.left, 0.6f, layerMask);
		RaycastHit2D hit3 = Physics2D.Raycast(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), facingRight ? new(1, -1) : new(-1, -1), 0.5f, layerMask);
		Debug.DrawLine(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0) + (facingRight ? new Vector3(1, 1, 0) : new Vector3(-1, 1, 0)) * 0.5f, Color.red);
		Debug.DrawLine(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0) + (facingRight ? Vector3.right : Vector3.left) * 0.6f, Color.red);
		Debug.DrawLine(transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0), transform.position + new Vector3(facingRight ? 0.2f : -0.2f, 0.6f, 0) + (facingRight ? new Vector3(1, -1, 0) : new Vector3(-1, -1, 0)) * 0.5f, Color.red);
		mainCollider.gameObject.layer = LayerMask.NameToLayer("Player");
		if ((hit1.collider != null && hit1.collider.CompareTag("Enemy")) || (hit2.collider != null && hit2.collider.CompareTag("Enemy")) || (hit3.collider != null && hit3.collider.CompareTag("Enemy")))
		{
			if (hit1.collider != null && hit1.collider.CompareTag("Enemy"))
			{
				healthController = hit1.collider.transform.gameObject.GetComponent<HealthController>();
				print(healthController);
				healthController.TakeDamage(playerDamage);
			}
			else if (hit2.collider != null && hit2.collider.CompareTag("Enemy"))
			{
				print(hit2.collider.transform.gameObject);
				healthController = hit2.collider.transform.gameObject.GetComponent<HealthController>();
				print(healthController);
				healthController.TakeDamage(playerDamage);
			}
			else if (hit3.collider != null && hit3.collider.CompareTag("Enemy"))
			{
				healthController = hit3.collider.transform.gameObject.GetComponent<HealthController>();
				print(healthController);
				healthController.TakeDamage(playerDamage);
			}
		}
		healthController = null;
	}
}
