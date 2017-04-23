using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAnimator : MonoBehaviour {

    enum MovingStates { CanMove, Moving, StoppedMoving }

    public float timeToMove;
    public GameObject arrow;

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
        for (int i=0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "Arrow")
            {
                arrow = transform.GetChild(i).gameObject;
                break;
            }
        }
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
	}

    public void Select()
    {
        animator.SetBool("personHappy", true);
    }

    public void Unselect()
    {
        if (movingState != MovingStates.Moving)
        {
            animator.SetBool("personHappy", false);
        }
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

    public void StartMoving(Vector3 direction)
    {
        movingState = MovingStates.Moving;
        arrow.SetActive(true);
        print(direction);
        arrow.transform.Rotate(Vector3.forward * angleBetweenVectors(Vector3.right, direction));
    }

    public void StopMoving()
    {
        movingState = MovingStates.StoppedMoving;
        timeMoving = 0;
        arrow.SetActive(false);
        arrow.transform.rotation = Quaternion.identity;
        animator.SetBool("personHappy", false);
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

    private float angleBetweenVectors(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle( vec1, vec2) * sign;
    }
}
