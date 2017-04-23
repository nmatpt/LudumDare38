using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState : MonoBehaviour {

	public Sprite defaultSprite;
	public Sprite selectedSprite;
	public Sprite destroyedSprite;
	public Sprite walkableSprite;
	public Sprite obstacleSprite;
	private SpriteRenderer spriteRenderer;

	private bool isDestroyed = false;
	private bool isObstacle = false;

	void Awake ()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
		spriteRenderer.sprite = defaultSprite;
	}

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
	}

    public void Select()
    {
		if (!isDestroyed && !isObstacle) {
			spriteRenderer.sprite = selectedSprite;
		}
    }

	public void SetObstacle()
	{
		isObstacle = true;
		spriteRenderer.sprite = obstacleSprite;
	}

	public void Unselect()
    {
		if (!isDestroyed && !isObstacle) {
			spriteRenderer.sprite = defaultSprite;
		}
    }

	public void Destroy()
	{
		spriteRenderer.sprite = destroyedSprite;
		isDestroyed = true;
	}

    public void SetWalkable()
    {
        spriteRenderer.sprite = walkableSprite;
    }

}

