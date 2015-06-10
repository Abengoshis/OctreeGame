using UnityEngine;
using System.Collections;

public abstract class Poolable : MonoBehaviour
{
	public bool expired;

	public abstract void Init(params object[] initParams);
}
