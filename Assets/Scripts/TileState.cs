using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState : MonoBehaviour {

	public Sprite defaultSprite;
	public Sprite selectedSprite;
	private SpriteRenderer spriteRenderer;
	private bool isSelected = false;

	void Awake ()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
		spriteRenderer.sprite = defaultSprite;
	}

	// Use this for initialization
	void Start () {
		
	}

	//void OnMouseDown(){
	//	if (!isSelected) {
	//		spriteRenderer.sprite = selectedSprite;
	//		isSelected = true;
	//	} else {
	//		spriteRenderer.sprite = defaultSprite;
	//		isSelected = false;
	//	}
	//}

	// Update is called once per frame
	void Update () {
	}

    public void Select()
    {
        spriteRenderer.sprite = selectedSprite;
        isSelected = true;
    }

    public void Unselect()
    {
        spriteRenderer.sprite = defaultSprite;
        isSelected = false;
    }

}

