using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	public Room room;

	// Components
	Rigidbody rigidbody;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Keypad8))
			rigidbody.AddForce(Vector3.up);
		if (Input.GetKey(KeyCode.Keypad2))
			rigidbody.AddForce(Vector3.down);
		if (Input.GetKey(KeyCode.Keypad4))
			rigidbody.AddForce(Vector3.left);
		if (Input.GetKey(KeyCode.Keypad6))
			rigidbody.AddForce(Vector3.right);
		if (Input.GetKey(KeyCode.KeypadPlus))
			rigidbody.AddForce(Vector3.forward);
		if (Input.GetKey(KeyCode.KeypadEnter))
			rigidbody.AddForce(Vector3.back);
	}
}
