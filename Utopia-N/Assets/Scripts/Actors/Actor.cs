using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : MonoBehaviour
{
	public float healthMax;
	private float health;
	public float impactDamage;
	public string information;

	private float damageFlashDuration = 0.0f;
	private float damageFlashTimer = 0.0f;
	private Dictionary<Renderer, Material> materialBuffer = new Dictionary<Renderer, Material>();	// Original materials of each renderer, stored so they can be returned to after flashing.
	public Material damageMaterial;

	//public Explosion deathExplosion;

	protected virtual void Awake()
	{
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			materialBuffer.Add (r, r.material);
		}
	}

	protected virtual void Update()
	{
		if (damageFlashTimer > 0)
		{
			damageFlashTimer -= Time.deltaTime;
			if (damageFlashTimer <= 0)
			{
				foreach (Renderer r in GetComponentsInChildren<Renderer>())
				{
					r.material = materialBuffer[r];
				}
			}
		}
	}

	public void TakeDamage(DamageInfo damageInfo)
	{
		health -= Mathf.Abs (damageInfo.damage);
		damageFlashTimer += damageFlashDuration;

		//RespondToDamage(damageInfo);

		if (health <= 0)
		{
			//RespondToDeath (damageInfo);
			Destroy (gameObject);
		}
	}
	
	//protected virtual void RespondToDamage(DamageInfo damageInfo) = 0;
	//protected virtual void RespondToDeath(DamageInfo damageInfo) = 0;
}

public struct DamageInfo
{
	public Actor source;
	public float damage;
	public int[] flags;

	public DamageInfo(Actor source, float damage, params int[] flags)
	{
		this.source = source;
		this.damage = damage;
		this.flags = flags;
	}
}