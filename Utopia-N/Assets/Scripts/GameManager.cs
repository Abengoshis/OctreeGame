using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }
	

	
	// ---- Instance ----
	
	public Player player;
	
	private void Awake()
	{
		instance = this;
	}

}
