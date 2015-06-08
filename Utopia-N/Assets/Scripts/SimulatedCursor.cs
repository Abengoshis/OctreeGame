using UnityEngine;
using System.Collections;

public class SimulatedCursor : MonoBehaviour
{
	public static Vector2 cursorPosition = new Vector2(0.0f, 0.0f);
	public static float sensitivity = 0.01f;

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
		if (cursorPosition.x > 0.5f)
			cursorPosition.x = 0.5f;
		else if (cursorPosition.x < -0.5f)
			cursorPosition.x = -0.5f;

		cursorPosition.y += Input.GetAxis("Mouse Y") * sensitivity * Screen.width / Screen.height;
		if (cursorPosition.y > 0.5f)
			cursorPosition.y = 0.5f;
		else if (cursorPosition.y < -0.5f)
			cursorPosition.y = -0.5f;
	}

	void LateUpdate()
	{
		// Apply after so things can modify the cursor position.
		rect.localPosition = Camera.main.ViewportToScreenPoint(cursorPosition);
	}
}
