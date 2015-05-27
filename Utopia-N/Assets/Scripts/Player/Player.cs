using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	public Room room;

	// Components
	private PlayerMovement movement;
	private PlayerInteraction interaction;
}
