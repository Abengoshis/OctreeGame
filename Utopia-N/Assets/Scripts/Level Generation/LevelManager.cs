using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
	public static LevelManager instance { get; private set; }

	public static GameObject GetRandomRoomPrefab()
	{
		return instance.roomPrefabs[Random.Range (0, instance.roomPrefabs.Length)];
	}

	// ---- Instance ----
	
	public GameObject[] roomPrefabs;		// Move these into Room.
	public GameObject tubePrefab;			// Move these into Room.

	public int graphRecursions;
	public Room graphRoot;

	private void Start()
	{
		instance = this;

		Build ();
	}

	private void Build()
	{
		// Branch from the root room.
		graphRoot.Branch(null, graphRecursions);
		graphRoot.gameObject.SetActive(true);
		graphRoot.ActivateConnections();
	}
}
