using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GristShell : MonoBehaviour
{
	public enum DirectionCalculationMode
	{
		NORMALS,
		OFFSET,
		OFFSET_NORMALISED
	}

	public DirectionCalculationMode directionCalculationMode = DirectionCalculationMode.OFFSET;	// The method used to calculate the direction of each bit.

	public void Start()
	{
		Explode ();
	}

	private void GetPointsOnSurface(float interval, out List<Vector3> surfacePoints, out List<Vector3> surfaceNormals)
	{
		surfacePoints = new List<Vector3>();
		surfaceNormals = new List<Vector3>();
	
		// Generate the list of points by interpolating across the mesh's triangles.
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			// Get the vertices and normals of the current triangle.
			Vector3[] vertices = new Vector3[3];
			Vector3[] normals = new Vector3[3];
			for (int j = 0; j < 3; ++j)
			{
				int index = mesh.triangles[i + j];
				vertices[j] = transform.TransformPoint(mesh.vertices[index]);
				normals[j] = transform.TransformDirection(mesh.normals[index]);
			}
			
			// Starting at the base (a random side, picked as side 0 here) and moving up the two adjacent sides, add equally spaced points.
			for (float tLength = 0.0f; tLength <= 1.0f; tLength += interval / Vector3.Distance((vertices[0] + vertices[1]) / 2, vertices[2]))
			{
				Vector3 left = Vector3.Lerp (vertices[0], vertices[2], tLength);
				Vector3 leftNormal = Vector3.Lerp (normals[0], normals[2], tLength);

				Vector3 right = Vector3.Lerp (vertices[1], vertices[2], tLength);
				Vector3 rightNormal = Vector3.Lerp (normals[1], normals[2], tLength);
				
				for (float tWidth = 0.0f; tWidth <= 1.0f; tWidth += interval / Vector3.Distance(left, right))
				{
					Vector3 betwixt = Vector3.Lerp (left, right, tWidth);
					Vector3 betwixtNormal = Vector3.Lerp (leftNormal, rightNormal, tWidth);
					
					surfacePoints.Add(betwixt);

					switch (directionCalculationMode)
					{
					// Interpolate between the normals of the left and right part of the triangle.
					case DirectionCalculationMode.NORMALS:
						surfaceNormals.Add(betwixtNormal);
						break;
					// Get the true offset from the centre.
					case DirectionCalculationMode.OFFSET:
						surfaceNormals.Add((betwixt - GetMeshCentre(mesh)));
						break;
					// Get the normalised offset from the centre (i.e. the offset mapped to a unit sphere).
					case DirectionCalculationMode.OFFSET_NORMALISED:
						surfaceNormals.Add((betwixt - GetMeshCentre(mesh)).normalized);
						break;
					}
				}
			}
		}

//		// Remove points that are very close to each other.
//		for (int i = surfacePoints.Count - 1; i >= 0; --i)
//		{
//			// Loop until reaching i, since any after i will have already checked for nearby points.
//			for (int j = 0; j < i; ++j)
//			{
//				// If the points are close, remove i.
//				if ((surfacePoints[i] - surfacePoints[j]).sqrMagnitude < CLOSEST_DISTANCE_SQUARED)
//				{
//					surfacePoints.RemoveAt (i);
//					break;
//				}
//			}
//		}
	}

	private Vector3 GetMeshCentre(Mesh mesh)
	{
		Vector3 centre = Vector3.zero;

		foreach (Vector3 vertex in mesh.vertices)
		{
			centre += vertex;
		}

		centre = transform.TransformPoint(centre / mesh.vertices.Length);

		return centre;
	}

	public void Explode()
	{
		// There will be a lot of surface points so write directly to the list rather than returning its value.
		List<Vector3> surfacePoints, surfaceNormals;
		GetPointsOnSurface(0.3f, out surfacePoints, out surfaceNormals);

		for (int i = 0; i < surfacePoints.Count; ++i)
		{
			GameManager.main.gristManager.AddGrist(surfacePoints[i], surfaceNormals[i] * 0.5f);
		}

	}
}
