using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class Starfield : MonoBehaviour
{
	public Color[] colours = new Color[1] { Color.white };
	public int coloursToMix = 0;

	public float minSize = 0.1f;
	public float maxSize = 0.5f;

	public float minDistance = 1.0f;
	public float maxDistance = 10.0f;
	public float nearClip = 2.0f;

	new private ParticleSystem particleSystem;
	private ParticleSystem.Particle[] spawnBuffer;
	private ParticleSystem.Particle[] dynamicBuffer;


	private void Awake()
	{
		// Put the star field directly in front of the camera so it is rendered.
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * Camera.main.nearClipPlane * 1.5f;

		// Assign the particle system component since apparently useful legacy members are deprecated.
		particleSystem = GetComponent<ParticleSystem>();

		// Create stars.
		spawnBuffer = new ParticleSystem.Particle[particleSystem.maxParticles];
		dynamicBuffer = new ParticleSystem.Particle[spawnBuffer.Length];
		for (int i = 0; i < spawnBuffer.Length; ++i)
		{
			RespawnParticle(i);
		}
	}

	private void RespawnParticle(int index)
	{
		// Position the star in a sphere.
		Vector3 position = Random.insideUnitSphere;
		spawnBuffer[index].position = Camera.main.transform.position + position.normalized * minDistance + position * (maxDistance - minDistance);

		// Randomise the size of the star.
		spawnBuffer[index].size = Random.Range (minSize, maxSize);

		// Randomise the colour of the star.
		if (coloursToMix <= 0)
		{
			spawnBuffer[index].color = colours[Random.Range (0, colours.Length)];
		}
		else
		{
			// Mix the first two colours.
			int firstColourIndex = Random.Range (0, colours.Length);
			Color colour = Color.Lerp (colours[firstColourIndex], colours[(firstColourIndex + Random.Range (0, colours.Length - 1)) % colours.Length], 0.5f);
			
          	// Mix further colours.
			for (int i = 1; i < coloursToMix; ++i)
			{
				colour = Color.Lerp (colour, colours[Random.Range (0, colours.Length)], 0.5f);
			}

			spawnBuffer[index].color = colour;
		}

		// Copy to the dynamic buffer.
		dynamicBuffer[index] = spawnBuffer[index];
	}

	private void Update()
	{
		for (int i = 0; i < dynamicBuffer.Length; ++i)
		{
			// Respawn the star if it goes out of bounds.
			float distance = Vector3.Distance (dynamicBuffer[i].position, Camera.main.transform.position);
			if (distance > maxDistance)
			{
				RespawnParticle(i);
			}

			float fadeFactor;
			if (distance < nearClip)
				fadeFactor = Mathf.Pow (distance / nearClip, 2);	// Prevent the star from getting too big on the screen.
			else
				fadeFactor = (1.0f - ((distance - nearClip) / (maxDistance - nearClip))) * 0.5f + 0.5f;	// Fade the star in when it is far away.

			dynamicBuffer[i].size = spawnBuffer[i].size * fadeFactor;
			dynamicBuffer[i].color = new Color32(spawnBuffer[i].color.r, spawnBuffer[i].color.g, spawnBuffer[i].color.b, (byte)(spawnBuffer[i].color.a * fadeFactor));
		}

		particleSystem.SetParticles(dynamicBuffer, dynamicBuffer.Length);
	}

}
