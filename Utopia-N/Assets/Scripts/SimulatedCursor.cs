using UnityEngine;
using System.Collections;

public class SimulatedCursor : MonoBehaviour
{
	public static Vector2 cursorPosition = new Vector2(0.5f, 0.5f);
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
		if (cursorPosition.x > 1)
			cursorPosition.x = 1;
		else if (cursorPosition.x < -0)
			cursorPosition.x = -0;

		cursorPosition.y += Input.GetAxis("Mouse Y") * sensitivity * Screen.width / Screen.height;
		if (cursorPosition.y > 1)
			cursorPosition.y = 1;
		else if (cursorPosition.y < 0)
			cursorPosition.y = 0;
	}

	void LateUpdate()
	{
		// Apply after so things can modify the cursor position.
		rect.localPosition = Camera.main.ViewportToScreenPoint(new Vector3(cursorPosition.x - 0.5f, cursorPosition.y - 0.5f));
	}
}
