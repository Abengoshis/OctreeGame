using UnityEngine;
using System.Collections;


public abstract class Weapon : MonoBehaviour
{
	public Transform shootPoint;
	public float shootDelay;
	private float shootTimer = 0.0f;

	private void Update()
	{
		Debug.DrawLine (transform.position, transform.position + transform.forward * 1000);

		if (shootTimer < shootDelay)
		{
			shootTimer += Time.deltaTime;
			if (shootTimer > shootDelay)
				shootTimer = shootDelay;
		}
	}

	public bool TryShoot(RaycastHit target)
	{
		if (shootTimer >= shootDelay)
		{
			Shoot(target);
			return true;
		}

		return false;
	}
	
	protected abstract void Shoot(RaycastHit target);
	
	protected Vector3 GetShootDirection(RaycastHit target)
	{
		Vector3 direction;
		if (target.transform != null)
			return (target.point - shootPoint.position).normalized;
		
		return shootPoint.forward;
	}
}
