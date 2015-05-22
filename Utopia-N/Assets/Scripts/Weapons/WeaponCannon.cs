using UnityEngine;
using System.Collections;

public class WeaponCannon : Weapon
{
	protected override void Shoot (RaycastHit target)
	{
		Vector3 direction = GetShootDirection(target);

	}
}