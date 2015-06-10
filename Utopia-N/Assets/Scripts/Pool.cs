using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class Pool : MonoBehaviour
{
	public Transform folder;

	public int capacity;
	public int remaining { get; private set; }
	public GameObject prefab;

	private Poolable[] pool;
	private int[] links;
	private int index;

	// Use this for initialization
	void Start ()
	{
		pool = new Poolable[capacity];
		links = new int[capacity];
		index = 0;

		for (int i = 0; i < capacity; ++i)
		{
			pool[i] = Instantiate<GameObject>(prefab).GetComponent<Poolable>();
			links[i] = i + 1;
		
			pool[i].transform.SetParent(folder);
			pool[i].gameObject.SetActive(false);
		}

		remaining = capacity;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Loop through the pool and, if any of the objects have expired, make them available by pointing their link towards the available index then making the index point to them.
		for (int i = 0; i < capacity; ++i)
		{
			if (pool[i].gameObject.activeSelf && pool[i].expired)
			{
				// Deactivate the pool item.
				pool[i].gameObject.SetActive(false);

				int temp = links[index];
				links[index] = i;
				links[i] = temp;

				++remaining;
			}
		}
	}

	public List<Poolable> GetAllActive()
	{
		List<Poolable> active = new List<Poolable>();
		for (int i = 0; i < capacity; ++i)
		{
			if (pool[i].gameObject.activeSelf)
				active.Add(pool[i]);
		}
		return active;
	}

	public Poolable Create(params object[] initParams)
	{
		if (index != capacity)
		{
			// Activate and initialise the item.
			Poolable item = pool[index];
			item.gameObject.SetActive(true);
			item.Init(initParams);
			item.expired = false;

			// Shift the index to the next available item.
			index = links[index];

			--remaining;

			return item;
		}

		return null;
	}
}
