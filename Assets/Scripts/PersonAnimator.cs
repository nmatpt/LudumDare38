using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAnimator : MonoBehaviour {
	//private SpriteRenderer spriteRenderer;
	public bool isSelected = false;
	private Animator animator;					//Used to store a reference to the Player's animator component.

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		//spriteRenderer.sprite = defaultSprite;

	}

	void OnMouseDown(){
		if (!isSelected) {
			animator.SetBool("personHappy", true);
			isSelected = true;
		} else {
			animator.SetBool("personHappy", false);
			isSelected = false;
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
