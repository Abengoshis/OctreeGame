using UnityEngine;
using System.Collections;

public class RayGun : Gun
{
	[System.Serializable]
	public struct Wave
	{
		public Color colour;
		public float amplitude;
		public float frequency;
		public float cycle;		// Speed the whole wave cycles.
		public float thickness;	// Thickness of the wave beam.
		public float rotation;	// Speed the whole wave rotates around.
		public float angle;		// Initial angle of the wave.
		public float twist;		// Angle the beam is twisted every unit.
		public float instability;	// Random offset to each of the vertices causing a kind of electric effect.
	}

	public Color colour;	// Colour of the straight main ray.
	public float thickness;	// Thickness of the straight main ray.
	public Wave[] waves;

	public float energyMax;		// Maximum energy.
	private float energy;		// Energy remaining.
	public float energyConsumptionRate;	// Energy consumed per second while firing.
	public float energyRechargeRate;	// Energy recharged per second while not firing.

	private void Update()
	{
		if (shooting)
		{
			energy -= energyConsumptionRate * Time.deltaTime;
			if (energy < 0)
				energy = 0;

			shooting = false;
		}
		else
		{
			energy += energyRechargeRate * Time.deltaTime;		// Maybe make this exponential.
			if (energy > energyMax)
				energy += energyMax;
		}
	}

	public override void Shoot (Vector3 direction)
	{
		if (energy > 0)
		{
			shooting = true;

			// Cast a ray straight forwards.
			RaycastHit hit;



		}
	}

	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			for (int i = 0; i < waves.Length; ++i)
				RenderWave(waves[i], transform.forward, 0.5f, 100);
		}
	}

	private void RenderWave(Wave wave, Vector3 direction, float vertexDistance, float totalDistance)
	{
		// Generate all the points of the wave.
		Vector3[] points = new Vector3[Mathf.CeilToInt (totalDistance / vertexDistance)];
		float distance = 0;
		for (int i = 0; i < points.Length; ++i)
		{
			Vector3 point = transform.position + direction * distance;

			// Get the perpendicular direction to offset through, rotated by the total rotation and the twist.
			float rot = Mathf.Deg2Rad * wave.angle + wave.rotation * Time.time + wave.twist * distance;
			Vector3 offsetDirection = transform.TransformVector(new Vector3(Mathf.Sin (rot), Mathf.Cos (rot), 0));	// Could even have a sub wave using the cross product of this!

			// Get the distance to offset through.
			float offset = Mathf.Lerp(0, wave.amplitude * Mathf.Sin (wave.frequency * distance - wave.cycle * Time.time), distance);

			// Offset the point.
			point += offsetDirection * offset;

			// Add instability vector.
			float rnd = Mathf.Lerp (0, wave.instability, distance);
			point += offsetDirection * Random.Range (-rnd, rnd);
			point += Vector3.Cross (direction, offsetDirection) * Random.Range (-rnd, rnd);

			points[i] = point;

			distance += vertexDistance;
		}

		Gizmos.color = wave.colour;
		for (int i = 1; i < points.Length; ++i)
		{
			Gizmos.DrawLine (points[i - 1], points[i]); 
			//Gizmos.DrawSphere(points[i], 0.1f);
		}
	}
}
