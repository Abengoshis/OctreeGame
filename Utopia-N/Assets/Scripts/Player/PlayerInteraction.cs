using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
	public Transform weaponHolder;
	public Weapon leftWeapon, rightWeapon;

	void FixedUpdate()
	{
		// Make the weapon holder rotate with the player.
		weaponHolder.transform.rotation = Quaternion.Lerp(weaponHolder.transform.rotation, transform.rotation, 20 * Time.fixedDeltaTime);
	}

	void LateUpdate()
	{
		// Move the weapon holder to the player after physics has been applied.
		weaponHolder.transform.position = transform.position;
	}

	// Update is called once per frame
	void Update ()
	{
		// ---- Find things in front of the player. ----
		Debug.DrawLine (transform.position, transform.position + transform.forward * 1000);

		// Cast a ray from the centre of the player outwards. This will get the point guns shoot at etc.
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit, 1000.0f, ~(1 << gameObject.layer));

		// ---- Shoot each weapon. ----

		if (Input.GetMouseButton(0))
		{
			leftWeapon.TryShoot(hit);
			rightWeapon.TryShoot(hit);
		}
	}
}
