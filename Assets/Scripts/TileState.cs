using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState : MonoBehaviour {

	public Sprite defaultSprite;
	public Sprite selectedSprite;
	public Sprite destroyedSprite;
	public Sprite walkableSprite;
	private SpriteRenderer spriteRenderer;

	private bool isDestroyed = false;

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
		if (!isDestroyed) {
			spriteRenderer.sprite = selectedSprite;
		}
    }

    public void Unselect()
    {
		if (!isDestroyed) {
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

