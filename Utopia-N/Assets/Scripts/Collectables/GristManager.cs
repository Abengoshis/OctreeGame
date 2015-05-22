// todo: clean this up so ones and zeros code isn't duplicate (function or pointer loop)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GristManager : MonoBehaviour
{
	public ParticleSystem zeros;
	public ParticleSystem ones;
	private bool lastEmittedWasZero = false;

	public int gristCount;
	public float gristLife;
	public float gristSize;
	public Color gristColour;
	public float gristShrinkDistanceSquared;
	private ParticleSystem.Particle[] gristZeros;
	private ParticleSystem.Particle[] gristOnes;
	
	public float attractFalloff;
	public float attractSpeed;
	public float velocityOverride;	// How much of the grist's velocity is overridden when steering.
	
	private void Awake()
	{
		zeros.maxParticles = gristCount / 2;
		ones.maxParticles = gristCount / 2;

		// Put the grist manager directly in front of the camera so all grists are always rendered.
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * Camera.main.nearClipPlane * 1.5f;

		gristZeros = new ParticleSystem.Particle[zeros.maxParticles];
		gristOnes = new ParticleSystem.Particle[ones.maxParticles];

		for (int l = 0; l < 50; ++l)
		{
			Vector3 pos = Random.onUnitSphere * 200;
			for (int i = 0; i < gristCount / 50; ++i)
			{
				AddGrist(pos + Random.insideUnitSphere * 20, Vector3.zero);
			}
		}
	}

	private void Update()
	{
		#region Zeros
		// Get the particles in a temporary array.
		ParticleSystem.Particle[] tempZeros = new ParticleSystem.Particle[zeros.maxParticles];
		zeros.GetParticles(tempZeros);

		// Manage how the grists are added and removed from the octree.
		for (int i = 0; i < zeros.maxParticles; ++i)
		{
			if (tempZeros[i].lifetime != 0 && tempZeros[i].lifetime - Time.deltaTime <= 0)
			{
				// If the grist will expire, deallocate it from the octree.
				GameManager.main.worldGenerator.Deallocate(WorldGenerator.Node.Data.Kind.GRIST, i, gristZeros[i].position);
			}
			else
			{
				// Potentially reallocate grist in the octree if it has moved from its previous position (or the index has been reallocated by the annoying particle system).
				if (tempZeros[i].position != gristZeros[i].position)
				{
					// Make sure the grist is in bounds.
					if (GameManager.main.worldGenerator.IsPositionValid(tempZeros[i].position) && GameManager.main.worldGenerator.IsPositionValid(gristZeros[i].position))
					{
						GameManager.main.worldGenerator.Reallocate(WorldGenerator.Node.Data.Kind.GRIST, i, gristZeros[i].position, tempZeros[i].position);
					}
					else
					{
						tempZeros[i].position = gristZeros[i].position;
						tempZeros[i].velocity = -tempZeros[i].velocity;
					}
				}
			}
		}
		#endregion
		#region Ones
		/* Indexes for ones are artificially placed after all the indexes of zeros. */

		// Get the particles in a temporary array.
		ParticleSystem.Particle[] tempOnes = new ParticleSystem.Particle[ones.maxParticles];
		ones.GetParticles(tempOnes);
		;
		// Manage how the grists are added and removed from the octree.
		for (int i = 0; i < ones.maxParticles; ++i)
		{
			if (tempOnes[i].lifetime != 0 && tempOnes[i].lifetime - Time.deltaTime <= 0)
			{
				// If the grist will expire, deallocate it from the octree.
				GameManager.main.worldGenerator.Deallocate(WorldGenerator.Node.Data.Kind.GRIST, zeros.maxParticles + i, gristOnes[i].position);
			}
			else
			{
				// Potentially reallocate grist in the octree if it has moved from its previous position (or the index has been reallocated by the annoying particle system).
				if (tempOnes[i].position != gristOnes[i].position)
				{
					// Make sure the grist is in bounds.
					if (GameManager.main.worldGenerator.IsPositionValid(tempOnes[i].position) && GameManager.main.worldGenerator.IsPositionValid(gristOnes[i].position))
					{
						GameManager.main.worldGenerator.Reallocate(WorldGenerator.Node.Data.Kind.GRIST, zeros.maxParticles + i, gristOnes[i].position, tempOnes[i].position);
					}
					else
					{
						tempOnes[i].position = gristOnes[i].position;
						tempOnes[i].velocity = -tempOnes[i].velocity;
					}
				}
			}
		}
		#endregion

		// Find all the grists in range of the player.
		Vector3 playerPosition = GameManager.main.player.transform.position;
		Vector3 playerLeafCoord = GameManager.main.worldGenerator.GetLeafCoord(playerPosition);
		Vector3 coord = new Vector3();
		for (coord.x = playerLeafCoord.x - 1; coord.x <= playerLeafCoord.x + 1; ++coord.x)
		{
			for (coord.y = playerLeafCoord.y - 1; coord.y <= playerLeafCoord.y + 1; ++coord.y)
			{
				for (coord.z = playerLeafCoord.z - 1; coord.z <= playerLeafCoord.z + 1; ++coord.z)
				{
					// Check if the coordinate is valid.
					if (GameManager.main.worldGenerator.IsCoordinateValid(coord))
					{
						// Loop through all grists in this octree cell.
						foreach (int gristIndex in GameManager.main.worldGenerator.GetLeafDataByLeafCoord(coord).gristIndices)
						{
							if (gristIndex < zeros.maxParticles)
							{
								#region Zeros
								// Check if it has passed through the player.
								RaycastHit hit;
								if (Physics.Linecast(gristZeros[gristIndex].position, tempZeros[gristIndex].position, out hit, 1 << (GameManager.main.player.layer)))
								{
									// Grist has passed through the player and must be destroyed.
									tempZeros[gristIndex].lifetime = 0;

									// Add to the grist counter.
								}
								else
								{
									// Get the vector to the player.
									Vector3 delta = playerPosition - tempZeros[gristIndex].position;

									if (delta.sqrMagnitude > 0)
									{
										// Get the steering vector towards the player.
										Vector3 steer = (delta / (delta.sqrMagnitude * attractFalloff) * attractSpeed) - tempZeros[gristIndex].velocity * velocityOverride;
										tempZeros[gristIndex].velocity += steer;

										if (delta.sqrMagnitude < gristShrinkDistanceSquared)
											tempZeros[gristIndex].size = gristSize * (delta.sqrMagnitude / gristShrinkDistanceSquared);
									}
								}
								#endregion
							}
							else
							{
								#region Ones
								int newIndex = gristIndex - zeros.maxParticles;

								// Check if it has passed through the player.
								RaycastHit hit;
								if (Physics.Linecast(gristOnes[newIndex].position, tempOnes[newIndex].position, out hit, 1 << (GameManager.main.player.layer)))
								{
									// Grist has passed through the player and must be destroyed.
									tempOnes[newIndex].lifetime = 0;
									
									// Add to the grist counter.
								}
								else
								{
									// Get the vector to the player.
									Vector3 delta = playerPosition - tempOnes[newIndex].position;
									
									if (delta.sqrMagnitude > 0)
									{
										// Get the steering vector towards the player.
										Vector3 steer = (delta / (delta.sqrMagnitude * attractFalloff) * attractSpeed) - tempOnes[newIndex].velocity * velocityOverride;
										tempOnes[newIndex].velocity += steer;
										
										if (delta.sqrMagnitude < gristShrinkDistanceSquared)
											tempOnes[newIndex].size = gristSize * (delta.sqrMagnitude / gristShrinkDistanceSquared);
									}
								}
								#endregion
							}
						}
					}
				}
			}
		}

		// Replace the grists with the new grists.
		gristZeros = tempZeros;
		gristOnes = tempOnes;

		// Replace the particles with the new grists.
		zeros.SetParticles(gristZeros, gristZeros.Length);
		ones.SetParticles(gristOnes, gristOnes.Length);
	}

	public void AddGrist(Vector3 position, Vector3 velocity)
	{
		// Emit a new grist particle as either a 0 or a 1.
		if (lastEmittedWasZero)
		{
			ones.Emit(position, velocity, gristSize, gristLife, gristColour);
		}
		else
		{
			zeros.Emit(position, velocity, gristSize, gristLife, gristColour);
		}

		// Set the next grist to the opposite bit.
		lastEmittedWasZero = !lastEmittedWasZero;
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
			
			// Find all the grists in range of the player.
			Vector3 playerPosition = GameManager.main.player.transform.position;
			Vector3 playerLeafCoord = GameManager.main.worldGenerator.GetLeafCoord(playerPosition);
			Vector3 coord = new Vector3();
			for (coord.x = playerLeafCoord.x - 1; coord.x <= playerLeafCoord.x + 1; ++coord.x)
			{
				for (coord.y = playerLeafCoord.y - 1; coord.y <= playerLeafCoord.y + 1; ++coord.y)
				{
					for (coord.z = playerLeafCoord.z - 1; coord.z <= playerLeafCoord.z + 1; ++coord.z)
					{
						// Check if the coordinate is valid.
						if (GameManager.main.worldGenerator.IsCoordinateValid(coord))
						{
							Bounds bounds = GameManager.main.worldGenerator.GetLeafBoundsByLeafCoord(coord);
							Gizmos.DrawWireCube(bounds.center, bounds.size);
						}
					}
				}
			}

		}
	}
}
