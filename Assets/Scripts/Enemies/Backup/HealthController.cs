using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class HealthController : MonoBehaviour
{
	// �eby nie wpisywa� ogromnych liczb do skalowania przeciwnik�w mo�emy u�y� tych dw�ch zmiennych do mno�enia ich hp
	[SerializeField]
	private float startingHealth = 100;
	[SerializeField]
	private float healthMult = 1;
	// T� zmienn� mo�na zmniejszy�, �eby zwi�kszy� odporno��, albo mo�na zwi�kszy� warto�� zmiennej, co zmniejszy odporno�� na dmg
	[SerializeField]
	private float defenceMult = 1;
	private float health;
	[SerializeField]
	private BoxCollider2D DeathTrigger;
	Animator anim;
	Rigidbody2D r2d;
	WalkingEnemyController enemyController;
	// Start is called before the first frame update
	void Start()
	{
		enemyController = GetComponent<WalkingEnemyController>();
		health = startingHealth * healthMult;
		anim = GetComponent<Animator>();
		r2d = GetComponent<Rigidbody2D>();
	}

	public void TakeDamage(int damage)
	{
		health -= (damage * defenceMult);
		anim.SetBool("Walk", false);
		anim.SetBool("Idle", false);
		anim.SetBool("Attack1", false);
		anim.SetBool("Attack2", false);
		anim.SetBool("Take damage", true);
		int pushDirection;
		if (enemyController.facingRight == true)
		{
			pushDirection = -3;
		}
		else
		{
			pushDirection = 3;
		}
		r2d.linearVelocity += new Vector2(pushDirection, 0);
		Debug.Log("Ouch, I've only " + health + " left");
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<BoxCollider2D>() == DeathTrigger)
		{
			health = 0;
			anim.SetBool("Walk", false);
			anim.SetBool("Idle", false);
			anim.SetBool("Attack1", false);
			anim.SetBool("Attack2", false);
			anim.SetBool("Take damage", true);
			CheckDeath();
			Debug.Log("Ded");
		}
	}
	void CheckDeath()
	{
		if (health <= 0)
		{
			anim.SetBool("Take damage", false);
			anim.SetBool("Die", true);
		}
	}
	public IEnumerator Despawn()
	{
		yield return new WaitForSeconds(5f);
		Object.Destroy(gameObject);
	}

	void ResetAnims()
	{
		if (health > 0)
		{
			anim.SetBool("Take damage", false);
			anim.SetBool("Idle", true);
		}
	}
}
