using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
	[SerializeField]
	private int StartingPlayerHealth = 100;
	[SerializeField]
	private int PlayerLevel = 1;
	[SerializeField]
	private BoxCollider2D DeathTrigger;
	[SerializeField]
	private GameObject LevelStartPoint;
	Vector3 LevelStartPointPos;
	Animator anim;
	private float PlayerHealth;
	private Attack _attack;
	Rigidbody2D r2d;
	public HealthBar healthBar;
	private float maxHealth;
	public Animator DeathBackground;
	public GameObject DeathScreen;
	PlayerController playerController;
	// Start is called before the first frame update
	void Start()
	{
		playerController = GetComponent<PlayerController>();
		r2d = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		_attack = GetComponent<Attack>();
		PlayerHealth = StartingPlayerHealth * PlayerLevel;
		maxHealth = PlayerHealth;
		healthBar.SetHealthValue((int)PlayerHealth, (int)maxHealth);
		if (LevelStartPoint)
		{
			LevelStartPointPos = LevelStartPoint.transform.position;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (PlayerHealth <= 0)
		{
			anim.ResetTrigger("Hurt");
			anim.SetTrigger("Die");
		}
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			r2d.constraints = RigidbodyConstraints2D.FreezeAll;
		} else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			r2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
		}
	}
	public float GetHealth()
	{
		return PlayerHealth;
	}
	public void Respawn()
	{
		PlayerHealth = StartingPlayerHealth * PlayerLevel;
		transform.position = LevelStartPointPos;
		healthBar.SetHealth(100);
		healthBar.SetHealthValue((int)PlayerHealth, (int)maxHealth);
		anim.ResetTrigger("Die");
		anim.SetTrigger("Stop");
		DeathBackground.ResetTrigger("Start");
		DeathBackground.SetTrigger("End");
		Cursor.visible = false;
	}
	public void TakeDamage(int damage) {
		if (!_attack.attackIsHappening && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt") && gameObject.name == "Player")
		{
			anim.ResetTrigger("Stop");
			anim.ResetTrigger("Run");
			anim.ResetTrigger("Fall");
			anim.ResetTrigger("Jump");
			anim.ResetTrigger("Attack");
			anim.ResetTrigger("Attack2");
			anim.ResetTrigger("Attack3");
			anim.ResetTrigger("AirAttack");
			anim.ResetTrigger("Walk");
			anim.SetTrigger("Hurt");
		}
		_attack.attackCooldown = false;
		_attack.StopCoroutine(_attack.ResetCooldown());
		PlayerHealth -= damage;
		// Knockback WIP
		int pushDirection;
		if (playerController.checkIsGrounded() == true)
		{
			if (playerController.facingDirection() == true)
			{
				pushDirection = -3;
			}
			else
			{
				pushDirection = 3;
			}
		} else
		{
			if (playerController.facingDirection() == true)
			{
				pushDirection = -1;
			}
			else
			{
				pushDirection = 1;
			}
		}
		r2d.linearVelocity += new Vector2(pushDirection, 0);
		healthBar.SetHealthValue((int)PlayerHealth, (int)maxHealth);
		healthBar.SetHealth((int)Mathf.Abs((PlayerHealth / maxHealth) * 100));
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<BoxCollider2D>() == DeathTrigger)
		{
			PlayerHealth = 0;
			healthBar.SetHealth(0);
			healthBar.SetHealthValue((int)PlayerHealth, (int)maxHealth);
			Debug.Log("Ded");
		}
		if (other.CompareTag("Food"))
		{
			PlayerHealth = maxHealth;
			healthBar.SetHealthValue((int)PlayerHealth, (int)maxHealth);
			healthBar.SetHealth((int)Mathf.Abs((PlayerHealth / maxHealth) * 100));
			Destroy(other.transform.gameObject);
		}
	}
	public void ShowDeathMessage()
	{
		DeathScreen.SetActive(true);
		DeathBackground.ResetTrigger("End");
		DeathBackground.SetTrigger("Start");
		Cursor.visible = true;
	}
}
