using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
	const float SCALE_MIN = 20;
	const float SCALE_MAX = 50;
	const float DISTANCE_BETWEEN_LEVELS = 30;

	public Transform model { get; private set; }
	private int connectionCount;
	public int[] connectionOrder { get; private set; }
	public Room[] connectedRooms { get; private set; }
	public Wall[] connectionWalls { get; private set; }
	public Tube[] connectionTubes { get; private set; }
	public int level { get; private set; }

	public bool connectionsActive { get; private set; }

	public void ActivateConnections()
	{
		if (connectionsActive)
			return;

		// Loop through all connections and activate and position them if they are not already activated.
		foreach (int i in connectionOrder)
		{
			if (connectedRooms[i] == null || connectedRooms[i].isActiveAndEnabled)
				continue;
		
			Room child = connectedRooms[i];
			Wall wallToChild = connectionWalls[i];

			child.gameObject.SetActive(true);

			// Get the index of this room in the child.
			foreach (int j in child.connectionOrder)
			{
				if (child.connectedRooms[j] == this)
				{
					// Get the wall in the child that leads to this room.
					Wall wallToThis = child.connectionWalls[j];

					// Rotate the child so that the wallToThis is facing away from the wallToChild.
					Vector3 currentDirectionToThis = wallToThis.transform.forward;
					Vector3 desiredDirectionToThis = -wallToChild.transform.forward;
					if (currentDirectionToThis != desiredDirectionToThis)
					{
						Quaternion rotation = Quaternion.FromToRotation(currentDirectionToThis, desiredDirectionToThis);
						child.transform.rotation = rotation;
					}

					// Position the wallToThis of the child in front of the wallToChild so that the holes align.
					Vector3 offsetFromWallToThis = child.transform.position - wallToThis.holePosition;
					float distanceBetweenRooms = (Mathf.Abs(child.level - level) + 1) * DISTANCE_BETWEEN_LEVELS;
					child.transform.position = wallToChild.holePosition + offsetFromWallToThis - desiredDirectionToThis.normalized * distanceBetweenRooms;

					// Create a tube between the rooms.
					if (connectionTubes[i] == null)
					{
						connectionTubes[i] = Instantiate<GameObject>(LevelManager.instance.tubePrefab).GetComponent<Tube>();
						connectionTubes[i].transform.SetParent(transform.parent);
					}
					connectionTubes[i].SetRooms(this, child);
					//break;
				}
				else if (child.connectedRooms[j] != null)
				{
					// Create connection tubes to currently unpositioned rooms from the child.
					if (child.connectionTubes[j] == null)
					{
						child.connectionTubes[j] = Instantiate<GameObject>(LevelManager.instance.tubePrefab).GetComponent<Tube>();
						child.connectionTubes[j].transform.SetParent(child.transform.parent);
						child.connectionTubes[j].transform.localScale += new Vector3(0.0f, 0.0f, DISTANCE_BETWEEN_LEVELS * 2);
						child.connectionTubes[j].transform.forward = child.connectionWalls[j].transform.position - child.transform.position;
						child.connectionTubes[j].transform.position = child.connectionWalls[j].holePosition + child.connectionTubes[j].transform.forward * child.connectionTubes[j].transform.localScale.z * 0.5f;
					}
				}
			}
		}

		connectionsActive = true;
	}

	public void DeactivateConnections(Room exclusion = null)
	{
		if (!connectionsActive)
			return;

		// Loop through all connections and deactivate them if the player is not inside them.
		foreach (int i in connectionOrder)
		{
			if (connectedRooms[i] == null || !connectedRooms[i].isActiveAndEnabled)
				continue;
			
			Room child = connectedRooms[i];

			// If the child room is the room the player is in, reassign the tube connecting this room to the child room.
			if (child == exclusion)
			{
				// Get the index of this room in the player room.
				foreach (int j in child.connectionOrder)
				{
					if (child.connectedRooms[j] == this)
					{
						// Reassign the tube.
						child.connectionTubes[j] = child.connectionTubes[i];
						child.connectionTubes[i] = null;
						break;
					}
				}
			}
			else
			{
				// If the child room is not the room the player is in, deactivate it and remove its tubes.
				foreach (int j in child.connectionOrder)
				{
					if (child.connectionTubes[j] != null)
					{
						Destroy(child.connectionTubes[j].gameObject);
						child.connectionTubes[j] = null;
					}
				}
				child.gameObject.SetActive(false);
			}
		}

		connectionsActive = false;
	}

	private void Awake ()
	{
		model = transform.Find ("Model");

		connectionOrder = new int[6];
		connectedRooms = new Room[6];
		connectionWalls = new Wall[6];
		connectionTubes = new Tube[6];
		connectionsActive = false;

		// Populate connector walls.
		for (int i = 0; i < connectionWalls.Length; ++i)
			connectionWalls[i] = model.Find ("ConnectorWall " + i.ToString()).GetComponent<Wall>();

		// Populate connection order with numbers 0 to 5.
		for (int i = 0; i < connectionOrder.Length; ++i)
			connectionOrder[i] = i;

		// Shuffle connection order.
		for (int i = 0; i < connectionOrder.Length - 1; ++i)
		{
			int j = Random.Range(i + 1, connectionOrder.Length);
			int temp = connectionOrder[i];
			connectionOrder[i] = connectionOrder[j];
			connectionOrder[j] = temp;
		}
	}

	private void Connect(Room room)
	{
		// Connect if there are any connections free.
		if (connectionCount < connectionOrder.Length)
		{
			// Set the free connection to the room.
			connectedRooms[connectionOrder[connectionCount]] = room;

			// Create a hole in the corresponding connector wall.
			connectionWalls[connectionOrder[connectionCount]].SetHoleRandomly(Tube.RADIUS);

			++connectionCount;
		}
	}

	public void Branch(Room parent, int recursions)
	{
		if (connectionCount != 0)
		{
			Debug.Log ("Room already branched.");
			return;
		}

		if (parent == null)
		{
			level = 0;
		}
		else
		{
			level = parent.level + 1;
			Connect(parent);
		}

		if (recursions > 0)
		{
			// Create at least 1, and at most 4, child rooms.
			int numChildren = Random.Range (1, 4);
			for (int i = 0; i < numChildren; ++i)
			{
				// Create the child with a random shape and size.
				GameObject prefab = LevelManager.GetRandomRoomPrefab();
				Room child = Instantiate<GameObject>(prefab).GetComponent<Room>();
				child.name = prefab.name;
				child.transform.SetParent(transform.parent);
				child.model.transform.localScale = new Vector3((int)(Random.Range (SCALE_MIN, SCALE_MAX + 1) * 100) / 100.0f,
				                                               (int)(Random.Range (SCALE_MIN, SCALE_MAX + 1) * 100) / 100.0f,
				                                               (int)(Random.Range (SCALE_MIN, SCALE_MAX + 1) * 100) / 100.0f);

				// Connect to the child.
				Connect (child);

				// Branch the child into more rooms.
				child.Branch(this, recursions - 1);
			}
		}

		gameObject.SetActive(false);
	}
}
