using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
	public Transform model;
	public Camera camera;
	private Vector3 cameraOffset;
	
	public Rigidbody rigidbody;
	public float xAcceleration, yAcceleration, zAcceleration;
	public float pitchAcceleration, yawAcceleration, rollAcceleration;
	private float throttle;
	private Vector3 angularVelocity = Vector3.zero;	// Fake angular velocity since rotation is constrained to avoid jarring collisions.
	
	private float xInput, yInput, zInput, xAngInput, yAngInput, zAngInput;

	private bool oob = false;	// Whether the player is out of bounds and will automatically turn back.
	private Vector3 oobDirection, oobUp, oobCamPos;
	private float oobSpeed;
	private float oobReturnDuration = 4.0f;
	private float oobReturnTimer = 0.0f;

	private void Awake()
	{
		cameraOffset = camera.transform.position - transform.position;
		rigidbody.velocity = Vector3.zero;
	}
	
	private void Update()
	{
		if (!oob)
		{
			// Get the cursor in normalised ellipsoid space.
			Vector2 ellipseCoord = new Vector2((SimulatedCursor.cursorPosition.x * Screen.height / Screen.width), SimulatedCursor.cursorPosition.y);

			xInput = Input.GetAxis("Skew");
			yInput = -Input.GetAxis("Vertical");
			zInput = Input.GetAxis("Depth");

			xAngInput = -(ellipseCoord.y);
			yAngInput = ellipseCoord.x * Screen.width / Screen.height;
			zAngInput = Input.GetAxis("Horizontal");

			// Manage the fake angular velocity.
			angularVelocity /= (1 + rigidbody.angularDrag * Time.deltaTime);
			angularVelocity += new Vector3(pitchAcceleration * xAngInput, yawAcceleration * yAngInput, rollAcceleration * -zAngInput) * Time.deltaTime;

		}
		else
		{
			oobReturnTimer += Time.deltaTime;
			if (oobReturnTimer > oobReturnDuration)
			{
				oobReturnTimer = 0;
				oob = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!oob)
		{
			rigidbody.AddRelativeForce(xAcceleration * xInput, yAcceleration * yInput, zAcceleration * zInput, ForceMode.Acceleration);
			transform.rotation *= Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);

			model.transform.rotation = Quaternion.LookRotation(Camera.main.ViewportToWorldPoint(new Vector3(SimulatedCursor.cursorPosition.x + 0.5f, SimulatedCursor.cursorPosition.y + 0.5f, 1000.0f)) - transform.position, transform.up);

			camera.transform.position = transform.TransformPoint(cameraOffset);
			camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation(transform.forward, Vector3.Lerp (camera.transform.up, transform.up, Vector3.Angle (camera.transform.up, transform.up) * 0.005f)), 4 * Time.fixedDeltaTime);
		}
		else
		{
			float portionOOB = 0.5f;
			float portionWait = portionOOB + 0.1f;

			float lerp = Mathf.SmoothStep(0, 1, oobReturnTimer / (oobReturnDuration * portionOOB));
			Vector3 dForward = Vector3.Slerp (oobDirection + oobUp, -oobDirection + oobUp, lerp) - oobUp;	// Slerp the direction around up.
			Vector3 dUp = Vector3.Slerp (oobUp - oobDirection, -oobUp - oobDirection, lerp) + oobDirection;	// Slerp up around the direction.
			rigidbody.velocity = dForward * oobSpeed;
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dForward, dUp), 180 * Time.deltaTime); 

			if (oobReturnTimer <= oobReturnDuration * portionWait)
			{
				//camera.transform.position = Vector3.Lerp (oobCamPos, transform.TransformPoint(cameraOffset), lerp);		
				camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation(transform.position - camera.transform.position, dForward), 4 * Time.fixedDeltaTime);
			}
			else
			{
				lerp = Mathf.SmoothStep(0, 1, (oobReturnTimer - oobReturnDuration * portionWait) / (oobReturnDuration * (1 - portionWait)));
				camera.transform.position = Vector3.Lerp (camera.transform.position, transform.TransformPoint(cameraOffset), lerp);		
				camera.transform.rotation = Quaternion.Lerp(Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation(transform.position - camera.transform.position, dForward), 4 * Time.fixedDeltaTime),
				                                            Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation(transform.forward, Vector3.Lerp (camera.transform.up, transform.up, Vector3.Angle (camera.transform.up, transform.up) * 0.005f)), 4 * Time.fixedDeltaTime),
				                                            lerp);	// WOW this needs to be cleaned up!!!
			}

		}
	
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!oob)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Skybox"))
			{
				oob = true;
				oobDirection = rigidbody.velocity.normalized;
				oobUp = transform.up;
				oobCamPos = camera.transform.position;
				oobSpeed = rigidbody.velocity.magnitude;
				if (oobSpeed == 0)
					oobSpeed = zAcceleration;

				xInput = yInput = zInput = xAngInput = yAngInput = zAngInput = 0;
				angularVelocity = Vector3.zero;
			}
		}
	}
}
