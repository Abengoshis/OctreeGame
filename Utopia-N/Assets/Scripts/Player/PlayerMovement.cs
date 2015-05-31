using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
	public Vector3 moveAcceleration;
	public float moveSpeed;
	private Vector3 force;

	public Vector3 turnAcceleration;
	public float turnSpeed;
	public float turnDeadzone;	// Deadzone radius as a factor of the screen height.
	private Vector3 torque;
	private Vector3 angularVelocity;

	public Vector3 cameraOffset;

	private Transform model;
	private float modelAngleMax = 90.0f;	// The model will be pitched/yawed this amount as a factor of the screen height.

	// Components
	Rigidbody rigidbody;

	private void Awake()
	{
		model = transform.Find ("Model");
		rigidbody = GetComponent<Rigidbody>();
	}

	private void TurnModel(float pitchFactor, float yawFactor, float rollFactor)
	{
		float modelPitchDesired = pitchFactor * modelAngleMax * Screen.height / Screen.width;
		float modelYawDesired = yawFactor * modelAngleMax;
		float modelRollDesired = Mathf.Clamp (force.x / moveSpeed * modelAngleMax * 0.5f + rollFactor * modelAngleMax * 0.25f, -modelAngleMax * 0.4f, modelAngleMax * 0.4f);

		Vector3 temp = model.transform.localEulerAngles;
		temp.x += Mathf.DeltaAngle (temp.x, modelPitchDesired) * Time.deltaTime * 10;
		temp.y += Mathf.DeltaAngle (temp.y, modelYawDesired) * Time.deltaTime * 10;
		temp.z += Mathf.DeltaAngle (temp.z, modelRollDesired) * Time.deltaTime * 5;
		model.transform.localEulerAngles = temp;
	}

	private void CalculateTorque(float pitchFactor, float yawFactor, float rollFactor)
	{
		torque = Vector3.zero;
		torque = new Vector3(pitchFactor * turnAcceleration.x,
		                     yawFactor * turnAcceleration.y);

		torque.z = rollFactor * turnAcceleration.z;
		angularVelocity += torque * 100 * Time.deltaTime;
		angularVelocity -= angularVelocity * rigidbody.angularDrag * Time.deltaTime;
		if (angularVelocity.sqrMagnitude > turnSpeed * turnSpeed * 10000)
			angularVelocity = angularVelocity.normalized * turnSpeed;
	}

	private void CalculateForce()
	{
		force = Vector3.zero;

		force.x = Input.GetAxis("Horizontal") * moveAcceleration.x;
		force.y = (Input.GetKey (KeyCode.Space) ? moveAcceleration.y : 0) - (Input.GetKey(KeyCode.LeftControl) ? moveAcceleration.y : 0);
		force.z = Input.GetAxis("Vertical") * moveAcceleration.z;

	}

	private void LateUpdate()
	{
		// Get the cursor in normalised ellipsoid space.
		Vector2 ellipseCoord = new Vector2((SimulatedCursor.cursorPosition.x * Screen.height / Screen.width) / Screen.width, SimulatedCursor.cursorPosition.y / Screen.height);

		// DO SOMETHING WITH THE DEADZONE TO MAKE THE SPEED SCALE BETWEEN THE DEADZONE ELLIPSE AND THE OUTER ELLIPSE.

		float pitchFactor = -(ellipseCoord.y + 0.15f);
		float yawFactor = ellipseCoord.x * Screen.width / Screen.height;
		float rollFactor = (Input.GetKey (KeyCode.Q) ? 1 : 0) - (Input.GetKey(KeyCode.E) ? 1 : 0);

		CalculateForce();
		CalculateTorque(pitchFactor, yawFactor, rollFactor);
		TurnModel(pitchFactor, yawFactor, rollFactor);
	}

	private void FixedUpdate()
	{
		rigidbody.AddRelativeForce(force, ForceMode.Acceleration);
		transform.rotation *= Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);

		if (rigidbody.velocity.sqrMagnitude > moveSpeed * moveSpeed)
			rigidbody.velocity = rigidbody.velocity.normalized * moveSpeed;

		Vector3 nextCameraPosition = transform.position + transform.right * cameraOffset.x + transform.up * cameraOffset.y + transform.forward * cameraOffset.z;
		RaycastHit hit;
		if (Physics.SphereCast(transform.position, Camera.main.nearClipPlane, nextCameraPosition - transform.position, out hit, Vector3.Distance(transform.position, nextCameraPosition), ~(1 << gameObject.layer)))
		{
			nextCameraPosition = hit.point + hit.normal * Camera.main.nearClipPlane;
		}
		Camera.main.transform.position = nextCameraPosition;
		Camera.main.transform.LookAt(transform.position + transform.up * cameraOffset.y, transform.up);
	}
}
