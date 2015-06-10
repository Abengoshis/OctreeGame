using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
	public Gun[] guns;

	public void Shoot(Vector3 direction)
	{
		direction.Normalize();

		for (int i = 0; i < guns.Length; ++i)
			guns[i].Shoot(direction);
	}
}
