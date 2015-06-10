using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
	public Camera camera;

	private Transform aimTarget = null;
	private Vector3 aimPosition;

	private void Update ()
	{
		// Cast a ray from the camera through the simulated cursor to see what the player has the cursor over.
		Ray ray = camera.ViewportPointToRay(SimulatedCursor.cursorPosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 1000, ~(1 << gameObject.layer)))	// May want to push the ray forwards to not detect things behind the player.
		{
			aimPosition = hit.point;
			aimTarget = hit.transform;
		}
		else
		{
			aimPosition = ray.GetPoint(1000);
			aimTarget = null;
		}

		if (Input.GetAxis("Fire1") > 0.0f)
		{
			GetComponent<GunController>().Shoot (aimPosition - transform.position);
		}


	}
}
