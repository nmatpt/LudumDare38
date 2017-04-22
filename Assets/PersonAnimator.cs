using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAnimator : MonoBehaviour {
	//private SpriteRenderer spriteRenderer;
	private bool isSelected = false;
	private Animator animator;					//Used to store a reference to the Player's animator component.

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		//spriteRenderer.sprite = defaultSprite;

	}

	// Update is called once per frame
	void Update () {
		
	}

    public void Select()
    {
        animator.SetBool("personHappy", true);
        isSelected = true;
    }

    public void Unselect()
    {
        animator.SetBool("personHappy", false);
        isSelected = false;
    }
}
