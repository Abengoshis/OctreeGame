using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wall : MonoBehaviour
{
	private const float BUILDING_SIZE_THRESHOLD_MAX = 0.2f;
	private const float BUILDING_SIZE_THRESHOLD_MIN = 0.05f;

	public Vector3 localHolePosition { get; private set; }
	public Vector3 holePosition { get { return transform.TransformPoint(localHolePosition); } private set { localHolePosition = transform.InverseTransformPoint(value); } }
	public float holeRadius { get; private set; }
	private Transform[] pieces = new Transform[4];
	private List<Transform> buildings = new List<Transform>();

	public GameObject[] buildingPrefabs;
	public Material cityMaterial;
	
	private void Awake()
	{
		for (int i = 0; i < pieces.Length; ++i)
		{
			pieces[i] = transform.Find ("Piece " + i.ToString());
		}
	}

	public void SetHoleRandomly(float radius)
	{
		float width = radius / (transform.localScale.x * transform.parent.localScale.x);
		float height = radius / (transform.localScale.y * transform.parent.localScale.y);
		float lx = Random.Range (-0.4f + width, 0.4f - width);
		float ly = Random.Range (-0.4f + height, 0.4f - height);
		SetHole(transform.TransformPoint(lx, ly, 0), radius);
	}

	public void SetHole(Vector3 position, float radius)
	{
		holePosition = GetComponent<Collider>().ClosestPointOnBounds(position);
		holeRadius = radius;

		float sx = (transform.localRotation.y == 0 || transform.localRotation.y == 180) ? transform.localScale.x * transform.parent.localScale.x : transform.localScale.z * transform.parent.localScale.z;
		float sy = (transform.localRotation.x == 0 || transform.localRotation.x == 180) ? transform.localScale.y * transform.parent.localScale.y : transform.localScale.z * transform.parent.localScale.z;

		Vector3 point = transform.InverseTransformPoint(holePosition) + new Vector3(0.5f, 0.5f);
		float width = holeRadius / sx;
		float height = holeRadius / sy;

		float l, r, t, b;
		l = (point.x - width); // transform.localScale.x + 0.5f;
		r = (point.x + width); // transform.localScale.x + 0.5f;
		t = (point.y + height); // transform.localScale.y + 0.5f;
		b = (point.y - height); // transform.localScale.y + 0.5f;

		pieces[0].localScale = new Vector3(l, 1, 1);
		pieces[1].localScale = new Vector3(r - l, 1 - t, 1);
		pieces[2].localScale = new Vector3(r - l, b, 1);
		pieces[3].localScale = new Vector3(1 - r, 1, 1);

		pieces[0].localPosition = new Vector3(pieces[0].localScale.x * 0.5f - 0.5f, 0, 0);
		pieces[3].localPosition = new Vector3(0.5f - pieces[3].localScale.x * 0.5f, 0, 0);
		pieces[1].localPosition = new Vector3(pieces[0].localPosition.x + pieces[0].localScale.x * 0.5f + pieces[1].localScale.x * 0.5f, 0.5f - pieces[1].localScale.y * 0.5f, 0);
		pieces[2].localPosition = new Vector3(pieces[0].localPosition.x + pieces[0].localScale.x * 0.5f + pieces[1].localScale.x * 0.5f, pieces[2].localScale.y * 0.5f - 0.5f, 0);

		GenerateBuildings();
	}

	private void Split(Rect source)
	{
		// Split into two across smallest axis.
		Rect[] children = new Rect[2];
		children[0].xMin = source.xMin;
		children[0].yMin = source.yMin;
		children[1].xMax = source.xMax;
		children[1].yMax = source.yMax;
		if (source.width < source.height)
		{
			children[0].xMax = source.xMax;
			children[1].xMin = source.xMin;

			children[0].yMax = source.center.y;
			children[1].yMin = source.center.y;
		}
		else
		{
			children[0].yMax = source.yMax;
			children[1].yMin = source.yMin;

			children[0].xMax = source.center.x;
			children[1].xMin = source.center.x;
		}

		for (int i = 0; i < children.Length; ++i)
		{
			float buildingSize = (int)(Mathf.Max (children[i].size.x, children[i].size.y) * 100) / 100.0f;
			if (buildingSize > BUILDING_SIZE_THRESHOLD_MAX || buildingSize > BUILDING_SIZE_THRESHOLD_MIN && Random.Range (0, 4) != 0)
			{
				Split (children[i]);
			}
			else
			{
				if (Vector3.Distance(children[i].center, localHolePosition) < 0.3f || Random.Range (0, 3) != 0)
				{
					// Create a building 
					Transform building = Instantiate<GameObject>(buildingPrefabs[Random.Range (0, buildingPrefabs.Length)]).transform;
					building.transform.parent = transform;
					building.transform.up = -transform.forward;	// Make the building point away from the wall.
					building.transform.localPosition = children[i].center;
					building.transform.localScale = new Vector3(buildingSize * 0.6f, Mathf.Pow ((1.0f - Vector2.Distance(children[i].center, localHolePosition) + 0.2f) * 0.7f, 4), buildingSize * 0.6f);
					buildings.Add(building);
				}
			}
		}



	}

	private void GenerateBuildings()
	{
		// Clear current buildings.
		foreach (Transform building in buildings)
			Destroy (building.gameObject);
		buildings.Clear();

		// Recursively split each wall piece.
		foreach (Transform piece in pieces)
		{
			// Split the piece into buildings.
			Split (new Rect(piece.localPosition.x - piece.localScale.x / 2, piece.localPosition.y - piece.localScale.y / 2, piece.localScale.x, piece.localScale.y)); 
      	
			// Set the piece's material to the ground material as if it is the ground of the city.
			piece.GetComponent<Renderer>().material = cityMaterial;
		}
	}
}
