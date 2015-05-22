using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BitManager : MonoBehaviour
{
	public float bitLife;
	public float bitSize;
	public Color bitColour;
	private ParticleSystem.Particle[] bits;

	public float attractRange;
	public float attractSpeed;

	new private ParticleSystem particleSystem;	
	
	private void Awake()
	{
		// Put the bit manager directly in front of the camera so all bits are always rendered.
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * Camera.main.nearClipPlane * 1.5f;
		
		// Assign the particle system component.
		particleSystem = GetComponent<ParticleSystem>();

		bits = new ParticleSystem.Particle[particleSystem.maxParticles];

		for (int i = 0; i < bits.Length; ++i)
		{
			AddBit(Random.insideUnitSphere * 500, Vector3.zero);
		}
	}

	private void Update()
	{
		// Get the particles in a temporary array.
		ParticleSystem.Particle[] temp = new ParticleSystem.Particle[particleSystem.maxParticles];
		particleSystem.GetParticles(temp);

		// Manage how the bits are added and removed from the octree.
		for (int i = 0; i < particleSystem.maxParticles; ++i)
		{
			if (temp[i].lifetime <= 0 && bits[i].lifetime > 0)
			{
				// If the bit has expired, deallocate it from the octree.
				GameManager.main.worldGenerator.Deallocate(WorldGenerator.Node.Data.Kind.BIT, i, bits[i].position);
			}
			else if (temp[i].lifetime > 0 && bits[i].lifetime <= 0)
			{
				// If a new bit exists, allocate it to the octree.
				GameManager.main.worldGenerator.Allocate(WorldGenerator.Node.Data.Kind.BIT, i, temp[i].position);
			}
			else
			{
				// Potentially reallocate an existing bit index in the octree if it has moved from its previous position.
				if (temp[i].position != bits[i].position)
				{
					// Make sure the bit is in bounds.
					if (GameManager.main.worldGenerator.IsPositionValid(temp[i].position))
					{
						GameManager.main.worldGenerator.Reallocate(WorldGenerator.Node.Data.Kind.BIT, i, bits[i].position, temp[i].position);
					}
					else
					{
						temp[i].position = bits[i].position;
						temp[i].velocity = Vector3.zero;
					}
				}
			}
		}

		// Find all the bits in range of the player.
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
						// Loop through all bits in this octree cell.
						foreach (int bitIndex in GameManager.main.worldGenerator.GetLeafDataByLeafCoord(coord).bitIndices)
						{
							// Do stuff to the new bits.

							// Get the vector to the player.
							Vector3 delta = playerPosition - temp[bitIndex].position;

							// Steer the particle towards the player.
							temp[bitIndex].velocity = delta / delta.sqrMagnitude * attractSpeed;

						}
					}
				}
			}
		}

		// Replace the bits with the new bits.
		bits = temp;

		// Replace the particles with the new bits.
		particleSystem.SetParticles(bits, bits.Length);
	}

	public void AddBit(Vector3 position, Vector3 velocity)
	{
		// Emit a new bit particle.
		particleSystem.Emit(position, velocity, bitSize, bitLife, bitColour);
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
			
			// Find all the bits in range of the player.
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
