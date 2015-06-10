using UnityEngine;
using System.Collections;

// NOTE: Projectile (Actor sender, Color colour, float size, float speed, float spin, Vector3 direction, int layerMask)

public class ProjectileGun : Gun
{
	[System.Serializable]
	public struct Shot
	{
		public int repetitions;
		public Color colour;
		public float size;
		public float speed;
		public float spin;
		public Vector2 directionOffset;	// 0,0 is straight forwards, 1, 0 is 90 degrees left, 0, 1 is 90 degrees up.
	}

	public Shot[] shots;
	public float shotDelay;
	public float reloadDuration;	// Duration between the end of the last shot and the start of the next series of shots if constantly shooting.
	
	public override void Shoot (Vector3 direction)
	{
		if (!shooting)
		{
			StartCoroutine(ReleaseShots(direction));
		}
	}

	private IEnumerator ReleaseShots (Vector3 direction)
	{
		shooting = true;

		Pool pool = GetComponent<Pool>();

		for (int i = 0; i < shots.Length; ++i)
		{
			for (int j = 0; j < shots[i].repetitions + 1; ++j)
			{
				Poolable p = pool.Create(GetComponentInParent<Actor>(), shots[i].colour, shots[i].size, shots[i].speed, shots[i].spin, GetOffsetDirection(direction, shots[i].directionOffset), ~(1 << gameObject.layer));
				if (p != null)
					p.transform.position = transform.position;

				if (shotDelay > 0 && (shots.Length > 1 || shots[i].repetitions != 0))
					yield return new WaitForSeconds(shotDelay);
			}
		}

		if (reloadDuration > 0)
			yield return new WaitForSeconds(reloadDuration);

		shooting = false;
	}

	private Vector3 GetOffsetDirection(Vector3 direction, Vector2 offset)
	{
		if (offset.x > 0)
			direction = Vector3.Lerp (direction, transform.right, offset.x);
		else if (offset.x < 0)
			direction = Vector3.Lerp (direction, -transform.right, -offset.x);
		
		if (offset.y > 0)
			direction += Vector3.Lerp (direction, transform.up, offset.y);
		else if (offset.y < 0)
			direction += Vector3.Lerp (direction, -transform.up, -offset.y);

		direction.Normalize();
		return direction;
	}
}
