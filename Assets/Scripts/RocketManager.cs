using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketManager : MonoBehaviour {

	public Sprite defaultSprite;
	public Sprite launchSprite;
	public float launchSpeed = 1;

	private SpriteRenderer spriteRenderer;
	private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		rigidBody = GetComponent<Rigidbody2D> ();
		spriteRenderer.sprite = defaultSprite;
	}
	
	// Update is called once per frame
	public void Launch () {
		spriteRenderer.sprite = launchSprite;
		AddForce ();
		Invoke("AddForce", 2);
		Invoke("AddForce", 4);
	}

	private void AddForce()
	{
		Vector2 launch = new Vector2 (0, 1) * launchSpeed;
		rigidBody.AddForce (launch);
	}
		


}
	
