using UnityEngine;
using System.Collections;

public class CameraEffect : MonoBehaviour
{
	// A function pointer to accumulate rendering methods.
	public delegate void RenderFunction();
	public static RenderFunction ScreenRender;
	public static RenderFunction WorldRender;

	private void OnPostRender()
	{
		if (ScreenRender != null)
		{
			GL.LoadIdentity();
			ScreenRender();
		}

		if (WorldRender != null)
		{
			GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
			WorldRender();
		}
	}
}
