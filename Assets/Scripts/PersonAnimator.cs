using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAnimator : MonoBehaviour {
    
    enum MovingStates { CanMove, Moving, StoppedMoving, IsReceiving }

    public float movementPerSecond;
    private GameObject arrow;

    private float quantity = 0;     // tiny hack :p
    private MovingStates movingState = MovingStates.CanMove;

	private Animator animator;
    private TextMesh playerCountText;

    private GameObject destinationPerson;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
        playerCountText = GetComponentInChildren<TextMesh>();

        SetQuantity(quantity);    // tiny hack :p

        for (int i=0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "Arrow")
            {
                arrow = transform.GetChild(i).gameObject;
                break;
            }
        }

        if (movingState == MovingStates.IsReceiving) // hack
        {
            animator.SetBool("personHappy", true);
        }

    }

	// Update is called once per frame
	void Update () {
        if (movingState == MovingStates.Moving) {
            if (destinationPerson != null && destinationPerson.activeSelf == true)
            {
                float quantityInTick = movementPerSecond * Time.deltaTime;
                quantityInTick = Mathf.Min(quantityInTick, quantity);

                destinationPerson.GetComponent<PersonAnimator>().AddQuantity(quantityInTick);
                AddQuantity(-quantityInTick);

                if (quantity <= 0)
                {
                    StopMoving();
                }
            }
            else // something happened to the destination person :'(
            {
                print("DESTINATION PERSON DEAD. OR WORSE");
                StopMoving();
            }
        }
	}

    private void OnDisable()
    {
        if(destinationPerson != null && destinationPerson.activeSelf == true)
        {
            destinationPerson.GetComponent<PersonAnimator>().ReadyToMove();
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
        this.quantity = quantity;
        if (playerCountText != null)
        {
            // to account fo when this method is called before Start()
            playerCountText.text = Mathf.Round(quantity).ToString();
        }
    }

    public float GetQuantity()
    {
        return quantity;
    }

    public void AddQuantity(float quantity)
    {
        SetQuantity(this.quantity + quantity);
    }

    public void StartMoving(Vector3 direction, GameObject destinationPerson)
    {
        movingState = MovingStates.Moving;
        arrow.SetActive(true);
        arrow.transform.Rotate(Vector3.forward * AngleBetweenVectors(Vector3.right, direction));
        this.destinationPerson = destinationPerson;
    }

    public void StopMoving()
    {
        movingState = MovingStates.StoppedMoving;
        arrow.SetActive(false);
        arrow.transform.rotation = Quaternion.identity;
        animator.SetBool("personHappy", false);
    }

    public void SetReceivingPeople()
    {
        movingState = MovingStates.IsReceiving;
        if (animator != null)
        {
            animator.SetBool("personHappy", true);
        }
    }

    public void ReadyToMove()
    {
        movingState = MovingStates.CanMove;
        //animator.SetBool("personHappy", true);
    }

    public bool CanMove()
    {
        return movingState == MovingStates.CanMove;
    }

    public bool HasStoppedMoving()
    {
        return movingState == MovingStates.StoppedMoving;
    }

    private float AngleBetweenVectors(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle( vec1, vec2) * sign;
    }
}
