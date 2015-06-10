using UnityEngine;
using System.Collections;

public abstract class Gun : MonoBehaviour
{
	protected bool shooting = false;

	public abstract void Shoot(Vector3 direction);
}
