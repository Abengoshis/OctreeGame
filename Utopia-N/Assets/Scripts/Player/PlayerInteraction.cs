using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
	public float aimRadius = 1.0f;	// Outer radius as a factor of the screen height.

	private Transform aimTarget = null;
	private Transform lockTarget = null;	// todo:

	private void Update ()
	{
		// Keep the cursor in an ellipse.
		Vector2 ellipseCoord = new Vector2(SimulatedCursor.cursorPosition.x * Screen.height / Screen.width, SimulatedCursor.cursorPosition.y);
		if (ellipseCoord.magnitude > aimRadius * Screen.height)
		{
			ellipseCoord = ellipseCoord.normalized * aimRadius * Screen.height;
			SimulatedCursor.cursorPosition = new Vector2(ellipseCoord.x * Screen.width / Screen.height, ellipseCoord.y);
		}

		HandleAiming();
	}

	private void HandleAiming()
	{
		// Cast a ray from the camera through the simulated cursor to see what the player has the cursor over.
		RaycastHit hit;
		if (Physics.Raycast (Camera.main.ScreenPointToRay(SimulatedCursor.cursorPosition), out hit, 1000, ~(1 << gameObject.layer)))	// May want to push the ray forwards to not detect things behind the player.
		{
			aimTarget = hit.transform;
		}
		else
		{
			aimTarget = null;
		}
	}

	private void HandleShooting()
	{
		// Shoot all guns towards the aim target, so long as the aim target is within 60 degrees of the gun.
	}
}
