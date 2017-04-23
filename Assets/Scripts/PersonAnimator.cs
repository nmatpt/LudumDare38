using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAnimator : MonoBehaviour {

    enum MovingStates { CanMove, Moving, StoppedMoving }

    public float timeToMove;

    private float quantity = 0;     // tiny hack :p
    private float actualQuantity = 0;
    private MovingStates movingState;

	private Animator animator;
    private TextMesh playerCountText;
    private float timeMoving;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
        playerCountText = GetComponentInChildren<TextMesh>();

        SetQuantity(actualQuantity);    // tiny hack :p

        timeMoving = 0;
        movingState = MovingStates.CanMove;
	}

	// Update is called once per frame
	void Update () {
		if (movingState == MovingStates.Moving)
        {
            timeMoving += Time.deltaTime;
            if (timeMoving > timeToMove)
            {
                StopMoving();
            }
        }
        print(movingState);
	}

    public void Select()
    {
        animator.SetBool("personHappy", true);
    }

    public void Unselect()
    {
        animator.SetBool("personHappy", false);
    }

    public void SetQuantity(float quantity)
    {
        actualQuantity = this.quantity = quantity;
        if (playerCountText != null)
        {
            // to account fo when this method is called before Start()
            playerCountText.text = actualQuantity.ToString();
        }
    }

    public void StartMoving()
    {
        movingState = MovingStates.Moving;
    }

    public void StopMoving()
    {
        movingState = MovingStates.StoppedMoving;
        timeMoving = 0;
    }

    public void ReadyToMove()
    {
        movingState = MovingStates.CanMove;
    }

    public bool CanMove()
    {
        return movingState == MovingStates.CanMove;
    }

    public bool HasStoppedMoving()
    {
        return movingState == MovingStates.StoppedMoving;
    }
}
