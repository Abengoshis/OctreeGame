using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static GameManager main { get; private set; }

	public GameObject player;
	public WorldGenerator worldGenerator;
	public BitManager bitManager;

	private void Awake ()
	{
		main = this;
	}
}
