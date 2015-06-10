using UnityEngine;
using System.Collections;

public class ByteEnemy : Actor
{
	public enum Size { KILOBYTE, MEGABYTE, GIGABYTE, TERABYTE };
	public Size size;

	private Actor[] segments;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update ()
	{
		base.Update ();


	}
}
