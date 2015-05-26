using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
	public Vector3 localHolePosition { get; private set; }
	public Vector3 holePosition { get { return transform.TransformPoint(localHolePosition); } private set { localHolePosition = transform.InverseTransformPoint(value); } }
	public float holeRadius { get; private set; }
	private Transform[] pieces = new Transform[4];
	
	private void Awake()
	{
		for (int i = 0; i < pieces.Length; ++i)
		{
			pieces[i] = transform.Find ("Piece " + i.ToString());
		}
	}

	public void SetHoleRandomly(float radius)
	{
		float width = radius / (transform.localScale.x * transform.parent.localScale.x);
		float height = radius / (transform.localScale.y * transform.parent.localScale.y);
		float lx = Random.Range (-0.4f + width, 0.4f - width);
		float ly = Random.Range (-0.4f + height, 0.4f - height);
		SetHole(transform.TransformPoint(lx, ly, 0), radius);
	}

	public void SetHole(Vector3 position, float radius)
	{
		holePosition = GetComponent<Collider>().ClosestPointOnBounds(position);
		holeRadius = radius;

		float sx = transform.localRotation.y == 0 ? transform.localScale.x * transform.parent.localScale.x : transform.localScale.z * transform.parent.localScale.z;
		float sy = transform.localRotation.x == 0 ? transform.localScale.y * transform.parent.localScale.y : transform.localScale.z * transform.parent.localScale.z;

		Vector3 point = transform.InverseTransformPoint(holePosition) + new Vector3(0.5f, 0.5f);
		float width = holeRadius / sx;
		float height = holeRadius / sy;

		float l, r, t, b;
		l = (point.x - width); // transform.localScale.x + 0.5f;
		r = (point.x + width); // transform.localScale.x + 0.5f;
		t = (point.y + height); // transform.localScale.y + 0.5f;
		b = (point.y - height); // transform.localScale.y + 0.5f;

		pieces[0].localScale = new Vector3(l, 1, 1);
		pieces[1].localScale = new Vector3(r - l, 1 - t, 1);
		pieces[2].localScale = new Vector3(r - l, b, 1);
		pieces[3].localScale = new Vector3(1 - r, 1, 1);

		pieces[0].localPosition = new Vector3(pieces[0].localScale.x * 0.5f - 0.5f, 0, 0);
		pieces[3].localPosition = new Vector3(0.5f - pieces[3].localScale.x * 0.5f, 0, 0);
		pieces[1].localPosition = new Vector3(pieces[0].localPosition.x + pieces[0].localScale.x * 0.5f + pieces[1].localScale.x * 0.5f, 0.5f - pieces[1].localScale.y * 0.5f, 0);
		pieces[2].localPosition = new Vector3(pieces[0].localPosition.x + pieces[0].localScale.x * 0.5f + pieces[1].localScale.x * 0.5f, pieces[2].localScale.y * 0.5f - 0.5f, 0);		
	}
}
