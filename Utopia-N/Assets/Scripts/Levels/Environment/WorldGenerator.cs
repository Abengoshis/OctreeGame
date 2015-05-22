using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
	// An octree node.
	public class Node
	{
		// Info contained by the node.
		public class Data
		{
			public enum Kind
			{
				GRIST,
			}

			public HashSet<int> gristIndices;

			public Data()
			{
				gristIndices = new HashSet<int>();
			}
		}

		public Data data;

		public int level { get; private set; }
		public Node parent { get; private set; }
		public Node[] children { get; private set; }
		public Bounds bounds { get; private set; }

		public Node(int level, Node parent, Bounds bounds, OctreeInfo octreeInfo)
		{
			data = new Data();

			this.level = level;
			this.parent = parent;
			this.bounds = bounds;
			
			// Leaf nodes do not have children.
			if (level == octreeInfo.recursions)
			{
				this.children = null;
			}
			else
			{
				float xSplit = 0, ySplit = 0, zSplit = 0;
				if (octreeInfo.randomSplit)
				{
					xSplit = Random.Range (0.0f, 1.0f) * bounds.size.x;	// BUT MAKE IT CLAMPED TO octreeInfo.leafSize;
				}
				else
				{
					xSplit = bounds.size.x / 2;
					ySplit = bounds.size.y / 2;
					zSplit = bounds.size.z / 2;
				}

				// Increment the level for the children.
				int childLevel = level + 1;

				// Create eight new children.
				this.children = new Node[8];
				for (int x = 0; x < 2; ++x)
				{
					for (int y = 0; y < 2; ++y)
					{
						for (int z = 0; z < 2; ++z)
						{
							Bounds childBounds = new Bounds();
							childBounds.min = bounds.min + new Vector3(x * xSplit, y * ySplit, z * zSplit);
							childBounds.max = childBounds.min + new Vector3(
								x == 0 ? xSplit : bounds.size.x - xSplit,
								y == 0 ? ySplit : bounds.size.y - ySplit,
								z == 0 ? zSplit : bounds.size.z - zSplit);

							children[z + (y + x * 2) * 2] = new Node(childLevel, this, childBounds, octreeInfo);
						}
					}				
				}




			}
		}
	}

	[System.Serializable]
	public struct OctreeInfo
	{
		public float worldSize;
		public float leafSize { get; private set; }

		public int recursions;
		public int minRecursionsUntilRoom;
		public int maxRecursionsUntilRoom;
		public bool randomSplit;

		public void UpdateLeafSize()
		{
			leafSize = worldSize / Mathf.Pow (2, recursions);
		}
	}

	public OctreeInfo octreeInfo;	
	private Node octreeRoot;
	//private List<Node[,,]> octreeLevels;
	private Node[,,] octreeLeaves;

	public GameObject DEBUG_WORLD_CUBE_PREFAB;
	public void DEBUG_WORLD_GENERATE()
	{
		GameObject[,,] cubes = new GameObject[octreeLeaves.GetLength(0),octreeLeaves.GetLength(1),octreeLeaves.GetLength(2)];
		for (int i = 0; i < octreeLeaves.GetLength(0); ++i)
		{
			for (int j = 0; j < octreeLeaves.GetLength(1); ++j)
			{
				for (int k = 0; k < octreeLeaves.GetLength(2); ++k)
				{
					cubes[i,j,k] = Instantiate<GameObject>(DEBUG_WORLD_CUBE_PREFAB);
					Bounds bounds = GetLeafBoundsByLeafCoord(new Vector3(i,j,k));
					cubes[i,j,k].transform.position = bounds.center;
					cubes[i,j,k].transform.localScale = bounds.size;
				}
			}
		}

		Stack<Node> nodes = new Stack<Node>();
		nodes.Push(octreeRoot);
		List<Node> rooms = new List<Node>();
		while (nodes.Count != 0)
		{
			Node current = nodes.Pop();
			if (current.children != null)
			{
				if (current.level > 0 && Random.Range (0, 3) == 0)
				{
					Vector3 firstLeaf = GetLeafCoord (current.bounds.min);
					Vector3 lastLeaf = GetLeafCoord (current.bounds.max);
					int xLeaves = (int)(lastLeaf.x - firstLeaf.x) - 1;
					int yLeaves = (int)(lastLeaf.y - firstLeaf.y) - 1;
					int zLeaves = (int)(lastLeaf.z - firstLeaf.z) - 1;
					
					for (int i = 1; i < xLeaves - 1; ++i)
					{
						for (int j = 1; j < yLeaves - 1; ++j)
						{
							for (int k = 1; k < zLeaves - 1; ++k)
							{
								Destroy(cubes[(int)(firstLeaf.x + i), (int)(firstLeaf.y + j), (int)(firstLeaf.z + k)]);
							}
						}
					}

					rooms.Add(current);
				}
				else
				{
					foreach (Node child in current.children)
					{
						nodes.Push(child);
					}
				}
			}
		}

		foreach (Node room in rooms)
		{
			Node[] parentChildren = room.parent.children;

			for (int i = 0; i < parentChildren.Length; ++i)
			{
				for (int j = 0; j < parentChildren.Length; ++j)
				{
					if (i != j)
					{
						Bounds ib = parentChildren[i].bounds;
						Bounds jb = parentChildren[j].bounds;

						if (ib.center.x - jb.center.x != 0 ^ ib.center.y - jb.center.y != 0 ^ ib.center.z - jb.center.z != 0)
						{
							Vector3 ic = GetLeafCoord(ib.center);
							Vector3 jc = GetLeafCoord(jb.center);
							Vector3 dir = (jc - ic).normalized;

							for (int ii = 0; ii <= Vector3.Distance(ic, jc); ++ii)
							{
								int x = (int)(ic.x + dir.x * ii);
								int y = (int)(ic.y + dir.y * ii);
								int z = (int)(ic.z + dir.z * ii);
								Destroy(cubes[x, y, z]);
							}
						}						 
					}
				}
			}
		}
	}

	private void Awake()
	{
		// Build the octree recursively.
		octreeRoot = new Node(0, null, new Bounds(Vector3.zero, octreeInfo.worldSize * Vector3.one), octreeInfo);
		PopulateLeavesArray();
	
		//DEBUG_WORLD_GENERATE();
	}

	public bool IsCoordinateValid(Vector3 coordinate)
	{
		return !(coordinate.x < 0 || coordinate.x >= octreeLeaves.GetLength(0) ||
		         coordinate.y < 0 || coordinate.y >= octreeLeaves.GetLength(1) ||
		         coordinate.z < 0 || coordinate.z >= octreeLeaves.GetLength(2));
	}

	public bool IsPositionValid(Vector3 position)
	{
		return octreeRoot.bounds.Contains(position);
	}

	public bool IsLeafDifferent(Vector3 oldPosition, Vector3 newPosition)
	{
		return GetLeafCoord(oldPosition) != GetLeafCoord(newPosition);
	}

	#region Allocation

	// Call Allocate function on instantiation.
	// Call Reallocate function on update.
	// Call Deallocate function on destruction.
	
	public void Allocate(Node.Data.Kind kind, object datum, Vector3 position)
	{
		Node node = GetLeaf (position);

		for (int i = octreeInfo.recursions; i >= 0; --i)
		{
			switch (kind)
			{
			case Node.Data.Kind.GRIST:
				node.data.gristIndices.Add((int)datum);
				break;
			}
			node = node.parent;
		}
	}
	
	public void Deallocate(Node.Data.Kind kind, object datum, Vector3 position)
	{
		Node node = GetLeaf (position);
		
		for (int i = octreeInfo.recursions; i >= 0; --i)
		{
			switch (kind)
			{
			case Node.Data.Kind.GRIST:
				node.data.gristIndices.Remove((int)datum);
				break;
			}

			node = node.parent;
		}
	}
	
	public void Reallocate(Node.Data.Kind kind, object datum, Vector3 oldPosition, Vector3 newPosition)
	{
		// Check whether nodes need reassigning.
		if (IsLeafDifferent(oldPosition, newPosition))
		{
			Node oldNode = GetLeaf(oldPosition);
			Node newNode = GetLeaf(newPosition);

			for (int i = octreeInfo.recursions; i >= 0; --i)
			{
				// Remove data from the old node and add data to the new node.
				switch (kind)
				{
				case Node.Data.Kind.GRIST:
					oldNode.data.gristIndices.Remove((int)datum);
					newNode.data.gristIndices.Add((int)datum);
					break;
				}

				// If the parents of the nodes are the same, don't bother with any more reassignment, otherwise continue.
				if (oldNode.parent == newNode.parent) break;

				// Move up to the parents of the nodes.
				oldNode = oldNode.parent;
				newNode = newNode.parent;
			}
		}
	}

	#endregion

	public Vector3 GetLeafCoord(Vector3 position)
	{
		return new Vector3((int)((position.x - octreeRoot.bounds.min.x) / octreeLeaves[0,0,0].bounds.size.x),
		                   (int)((position.y - octreeRoot.bounds.min.y) / octreeLeaves[0,0,0].bounds.size.y),
		                   (int)((position.z - octreeRoot.bounds.min.z) / octreeLeaves[0,0,0].bounds.size.z));
	}

	private Node GetLeafByLeafCoord(Vector3 coordinate)
	{
		return octreeLeaves[(int)coordinate.x, (int)coordinate.y, (int)coordinate.z];
	}
	
	private Node GetNodeByLeafCoord(Vector3 coordinate, int level)
	{
		Node node = GetLeafByLeafCoord (coordinate);
		
		for (int i = octreeInfo.recursions - level; i > 0; --i)
		{
			node = node.parent;
		}
		
		return node;
	}
	
	public Node.Data GetLeafDataByLeafCoord(Vector3 coordinate)
	{
		return GetLeafByLeafCoord(coordinate).data;
	}
	
	public Node.Data GetNodeDataByLeafCoord(Vector3 coordinate, int level)
	{
		return GetNodeByLeafCoord (coordinate, level).data;
	}

	public Bounds GetLeafBoundsByLeafCoord(Vector3 coordinate)
	{
		return GetLeafByLeafCoord (coordinate).bounds;
	}

	public Bounds GetNodeBoundsByLeafCoord(Vector3 coordinate, int level)
	{
		return GetNodeByLeafCoord (coordinate, level).bounds;
	}

	private Node GetLeaf(Vector3 position)
	{
		return GetLeafByLeafCoord(GetLeafCoord(position));
	}

	private Node GetNode(Vector3 position, int level)
	{
		return GetNodeByLeafCoord(GetLeafCoord(position), level);
	}

	public Node.Data GetLeafData(Vector3 position)
	{
		return GetLeafDataByLeafCoord(GetLeafCoord(position));
	}

	public Node.Data GetNodeData(Vector3 position, int level)
	{
		return GetNodeDataByLeafCoord (GetLeafCoord(position), level);
	}

	public Bounds GetLeafBounds(Vector3 position)
	{
		return GetLeafBoundsByLeafCoord(GetLeafCoord (position));
	}

	public Bounds GetNodeBounds(Vector3 position, int level)
	{
		return GetNodeBoundsByLeafCoord(GetLeafCoord (position), level);
	}

	private void PopulateLeavesArray()
	{
		// Make lists of nodes for each level.
		List<List<Node>> levels = new List<List<Node>>();
		for (int i = 0; i <= octreeInfo.recursions; ++i)
			levels.Add (new List<Node>());

		// Add each node to their level list.
		Stack<Node> nodes = new Stack<Node>();
		nodes.Push(octreeRoot);
		while (nodes.Count != 0)
		{
			// Get the newest node.
			Node current = nodes.Pop();

			// Add the node to its level list.
			levels[current.level].Add(current);

			// Push the node's children to the stack.
			if (current.children != null)
			{
				foreach (Node child in current.children)
				{
					nodes.Push(child);
				}
			}
		}

		int leavesPerDimension = (int)Mathf.Pow (levels[levels.Count - 1].Count, 1.0f / 3.0f);
		octreeLeaves = new Node[leavesPerDimension, leavesPerDimension, leavesPerDimension];
		foreach (Node node in levels[levels.Count - 1])
		{
			int nodeX = (int)((node.bounds.min.x - octreeRoot.bounds.min.x) / node.bounds.size.x);
			int nodeY = (int)((node.bounds.min.y - octreeRoot.bounds.min.y) / node.bounds.size.y);
			int nodeZ = (int)((node.bounds.min.z - octreeRoot.bounds.min.z) / node.bounds.size.z);
			octreeLeaves[nodeX, nodeY, nodeZ] = node;
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			if (Input.GetKey(KeyCode.F1))
			{
				Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);

				for (int i = 0; i < octreeLeaves.GetLength(0); ++i)
				{
					for (int j = 0; j < octreeLeaves.GetLength(1); ++j)
					{
						for (int k = 0; k < octreeLeaves.GetLength(2); ++k)
						{
							Gizmos.DrawWireCube(octreeLeaves[i,j,k].bounds.center, octreeLeaves[i,j,k].bounds.size);
						}
					}
				}
			}
		}
	}
}
