using UnityEngine;
using System.Collections;

public class SimulatedCursor : MonoBehaviour
{
	public static Vector2 cursorPosition = new Vector2(0.5f, 0.5f);
	public static float sensitivity = 30.0f;

	// Components
	RectTransform rect;

	// Use this for initialization
	void Start ()
	{
		rect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		cursorPosition.x += Input.GetAxis("Mouse X") * sensitivity;
		cursorPosition.y += Input.GetAxis("Mouse Y") * sensitivity;
	}

	void LateUpdate()
	{
		rect.localPosition = cursorPosition;
	}
}
