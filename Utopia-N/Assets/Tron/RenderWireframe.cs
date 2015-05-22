using UnityEngine;
using System.Collections;

public class RenderWireframe : MonoBehaviour
{	
	public Color colour;
	public Material material;

	private void Awake()
	{
		CameraEffect.WorldRender += Wireframe;
	}

	private void DrawShitLine(Vector3 a, Vector3 b, Color colour)
	{
		GL.Color (colour);
		GL.Vertex(a);
		GL.Vertex(b);
	}

	private void DrawLine(Vector3 a, Vector3 b, Color colour, int width = 0)
	{
		if (width == 0)
		{
			DrawShitLine(a, b, colour);
		}
		else
		{
			// Something involving projecting points onto screen and rendering as screen oriented quads?
		}
	}

	private void Wireframe()
	{
		if (material != null)
		{
			foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
			{
				Mesh mesh = mf.mesh;

				material.SetPass(0);
				GL.Begin (GL.LINES);

				GL.modelview = Camera.main.worldToCameraMatrix;
				GL.modelview *= transform.localToWorldMatrix;

				// Loop through every triangle and draw its lines.
				for (int i = 0; i < mesh.triangles.Length; i += 3)
				{
					DrawLine(mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]], colour, 0);
					DrawLine(mesh.vertices[mesh.triangles[i + 1]], mesh.vertices[mesh.triangles[i + 2]], colour, 0);
					DrawLine(mesh.vertices[mesh.triangles[i + 2]], mesh.vertices[mesh.triangles[i]], colour, 0);
				}

				GL.End ();
			}
		}
	}


	//---Possibly more kinds of wireframe?


}
