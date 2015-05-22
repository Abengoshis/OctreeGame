using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
	public float acceleration = 1.0f;
	public float maxSpeed = 10.0f;

	public float pitchSpeed = 1.0f;
	public float yawSpeed = 1.0f;
	public float rollSpeed = 1.0f;

	#region Components
	new private Rigidbody rigidbody;
	#endregion

	void Awake()
	{
		// Assign components.
		rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		// Rotation.
		float pitch = -Input.GetAxis("Mouse Y");
		float yaw = Input.GetAxis("Mouse X");
		float roll = (Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? -1 : 0);
		rigidbody.MoveRotation (transform.rotation * Quaternion.Euler(pitch * pitchSpeed, yaw * yawSpeed, roll * rollSpeed));

		// Movement.
		Vector3 move = new Vector3(Input.GetAxis("Horizontal"),
		                           (Input.GetKey(KeyCode.LeftShift) ? 1 : 0) + (Input.GetKey(KeyCode.LeftControl) ? -1 : 0),
		                           Input.GetAxis("Vertical")).normalized;

		// Do not allow acceleration over the maximum speed, but do allow the player to be moving faster than the maximum speed.
		if (rigidbody.velocity.magnitude + acceleration * Time.fixedDeltaTime >= maxSpeed)
			rigidbody.AddRelativeForce(move * (maxSpeed - rigidbody.velocity.magnitude), ForceMode.Acceleration);
		else
			rigidbody.AddRelativeForce (move * acceleration, ForceMode.Acceleration);
	}
}
