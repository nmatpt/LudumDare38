﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleHandler : MonoBehaviour {

    public GameObject playButton;
    public GameObject howToPlayButton;
    public GameObject quitButton;

	// Use this for initialization
	void Start () {
        playButton.GetComponent<Button>().onClick.AddListener(OnPlayPressed);
        howToPlayButton.GetComponent<Button>().onClick.AddListener(OnHowToPlayPressed);
        quitButton.GetComponent<Button>().onClick.AddListener(OnQuitPressed);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPlayPressed()
    {
        print("Pressed Play");
    }

    public void OnHowToPlayPressed()
    {
        print("Pressed How To Play");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

}
