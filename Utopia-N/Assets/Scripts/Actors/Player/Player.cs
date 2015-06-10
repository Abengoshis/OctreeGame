using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(Rigidbody))]
public class Player : Actor
{
	public Room room;

	// Components
	private PlayerMovement movement;
	private PlayerInteraction interaction;

	protected override void Awake()
	{
		base.Awake();


	}
}
