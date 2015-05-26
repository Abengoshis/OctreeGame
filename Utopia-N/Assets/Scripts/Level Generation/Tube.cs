using UnityEngine;
using System.Collections;

public class Tube : MonoBehaviour
{
	public const float RADIUS = 0.5f;
	public float length { get { return transform.localScale.z; } }

	public Room a { get; private set; }
	public Room b { get; private set; }

	public Vector3 aHole { get; private set; }
	public Vector3 bHole { get; private set; }

	/// <summary>
	/// Enters the tube, activating the rooms.
	/// </summary>
	public void Enter()
	{
		Debug.Log ("Entering tube.");
		a.ActivateConnections();
		b.ActivateConnections();
	}

	/// <summary>
	/// Exits the tube, deactivating room the player came from.
	/// </summary>
	private void Exit(Vector3 position)
	{
		Debug.Log ("Exiting tube.");

		if ((position - bHole).sqrMagnitude < (position - aHole).sqrMagnitude)
		{
			a.DeactivateConnections(b);
			GameManager.instance.player.room = b;
		}
		else
		{
			b.DeactivateConnections(a);
			GameManager.instance.player.room = a;
		}
	}

	public void SetRooms(Room a, Room b)
	{
		this.a = a;
		this.b = b;

		foreach (int index in a.connectionOrder)
		{
			if (a.connectedRooms[index] == b)
			{
				aHole = a.connectionWalls[index].holePosition;
				break;
			}
		}

		foreach (int index in b.connectionOrder)
		{
			if (b.connectedRooms[index] == a)
			{
				bHole = b.connectionWalls[index].holePosition;
				break;
			}
		}

		// Place the tube between the ends.
		transform.position = Vector3.Lerp (aHole, bHole, 0.5f);

		// Rotate the tube to the direction between the ends.
		transform.rotation = Quaternion.LookRotation(bHole - aHole);

		// Set the length of the tube to the distance between the ends.
		transform.localScale = new Vector3(RADIUS * 2, RADIUS * 2, (bHole - aHole).magnitude);
	}

	private void OnTriggerEnter(Collider other)
	{
		// On entry of a tube, activate the next room.
		if (other.transform.root == GameManager.instance.player.transform)
		{
			Enter();
		}
	}
	
	private void OnTriggerExit(Collider other)
	{
		// On exit of a tube, deactivate the previous room.
		if (other.transform.root == GameManager.instance.player.transform)
		{
			Exit(GameManager.instance.player.transform.position);
		}
	}
}
