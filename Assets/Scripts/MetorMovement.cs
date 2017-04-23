using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetorMovement : MonoBehaviour {
	private Rigidbody2D rb2d;
	private float distanceToTarget = float.MaxValue;

	public float speed = 3.0f;

	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();
	}
	
	void Update () {
		transform.Rotate (new Vector3 (0, 0, 45) * Time.deltaTime);
	}

	public void MoveTo(Vector2 location){
		GetComponent<Rigidbody2D>().AddForce (location.normalized * speed);
	}
}
