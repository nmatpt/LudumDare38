﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketManager : MonoBehaviour {

	public Sprite defaultSprite;
	public Sprite launchSprite;
	public float launchSpeed = 1;

	private SpriteRenderer spriteRenderer;
	private Rigidbody2D rigidBody;

	private TextMesh progressText;

	public float progressPerPersonPerSecond = 1;
	private float nrPeopleIn = 0;
	private float buildProgress = 0;

	private bool launched = false;

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		rigidBody = GetComponent<Rigidbody2D> ();
		progressText = GetComponentInChildren<TextMesh>();
		spriteRenderer.sprite = defaultSprite;
		BuildDaemon ();
	}

	void Update()
	{
		if(!launched){
			string text = (int)buildProgress + "%";
			if (buildProgress >= 100) {
				text = "Ready! Press space to launch.";
			}
			progressText.text = text;

			if (Input.GetKeyDown(KeyCode.Space) && buildProgress >= 100) {
					launched = true;
				Launch ();
					progressText.text = "";
			}	
		}
	}
	
	public void Launch () 
	{
		spriteRenderer.sprite = launchSprite;
		AddForce ();
		Invoke("AddForce", 2);
		Invoke("AddForce", 4);
        print("SCORE = " + (int)nrPeopleIn);
	}

	private void AddForce()
	{
		Vector2 launch = new Vector2 (0, 1) * launchSpeed;
		rigidBody.AddForce (launch);
	}
		

	public void AddPeople(float nrPeople) 
	{
        if (launched == false)
        {
            nrPeopleIn += nrPeople;
        }
	}

    public bool IsReadyToLaunch()
    {
        return !launched && buildProgress >= 100;
    }

    public float GetPeopleInside()
    {
        return nrPeopleIn;
    }

    private void BuildDaemon() {
		InvokeRepeating	("BuildRocket", 1.0f, 1.0f);
	}

	private void BuildRocket()
	{
		buildProgress += nrPeopleIn * progressPerPersonPerSecond;
		buildProgress = Mathf.Min (buildProgress, 100);
	}
}
	
