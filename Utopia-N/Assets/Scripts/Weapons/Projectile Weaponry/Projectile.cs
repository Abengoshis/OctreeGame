using UnityEngine;
using System.Collections;

public sealed class Projectile : Poolable
{
	private const float HORIZON_DISTANCE = 1000.0f;
	private const float HORIZON_DISTANCE_SQUARED = HORIZON_DISTANCE * HORIZON_DISTANCE;

	public Transform model;

	private Actor sender;
	private Color colour;
	private float size;
	private float speed;
	private float spin;
	private Vector3 direction;
	private int layerMask;

	private float damage;

	/// <param name="initParams">Actor sender, Color colour, float size, float speed, float spin, Vector3 direction, int layerMask</param>
	public override void Init (params object[] initParams)
	{
		sender = (Actor)initParams[0];
		colour = (Color)initParams[1];
		size = (float)initParams[2];
		speed = (float)initParams[3];
		spin = (float)initParams[4];
		direction = (Vector3)initParams[5];
		layerMask = (int)initParams[6];

		foreach (Renderer r in GetComponentsInChildren<Renderer>())
			r.material.color = colour;

		transform.localScale = size * Vector3.one;
		transform.rotation = Quaternion.LookRotation(direction, sender.transform.up);

		damage = size * size * speed;
	}

	private void Update()
	{
		if (!expired)
		{
			if ((model.position - GameManager.instance.player.transform.position).sqrMagnitude > HORIZON_DISTANCE_SQUARED)
			{
				expired = true;
				return;
			}

			Vector3 nextPosition = model.position + direction * speed * Time.deltaTime;

			RaycastHit hit;
			if (Physics.Linecast(model.position, nextPosition, out hit, layerMask))
			{
				Actor actor = hit.transform.root.GetComponentInChildren<Actor>();
				if (actor != null)
				{
					DamageInfo dmg = new DamageInfo();
					dmg.source = sender;
					dmg.damage = damage;
					actor.TakeDamage(dmg);
					expired = true;
					return;
				}
			}

			model.position = nextPosition;
			transform.Rotate(0, 0, spin * Time.deltaTime, Space.Self);	// Rotate the transform, not the model. This way, the offset model can be offset to appear to rotate around the centre.
		}
	}
}

//public class Missile : Projectile
//{
//	public Transform model;
//	
//	private Vector3 velocity;
//	private Vector3 force;
//	
//	/// <param name="initParams"></param>
//	public override void Init (params object[] initParams)
//	{
//
//	}
//
//	private void Update()
//	{
//		// Euler integrate force->velocity->position.
//		velocity += force * Time.fixedDeltaTime;
//		model.position += velocity * Time.fixedDeltaTime;
//
//		// Aim towards the direction of travel and spin.
//		model.rotation = Quaternion.LookRotation(velocity, model.up);
//		model.Rotate (0, 0, spin * Time.deltaTime, Space.Self);
//			
//		// Clear forces.
//		force = Vector3.zero;
//	}
//}
