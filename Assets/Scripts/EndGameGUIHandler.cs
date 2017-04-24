using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameGUIHandler : MonoBehaviour {

    public GameObject retryButton;
    public GameObject menuButton;

    public string winMessage;
    public string winComment;
    public string loseMessage;
    public string loseComment;

	// Use this for initialization
	void Start () {
        retryButton.GetComponent<Button>().onClick.AddListener(OnRetryPressed);
        menuButton.GetComponent<Button>().onClick.AddListener(OnMenuPressed);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void WonTheGame(int peopleSaved)
    {
        Show(winMessage, winComment.Replace("#", peopleSaved.ToString()));
    }

    public void LostTheGame()
    {
        Show(loseMessage, loseComment);
    }

    private void Show(string message, string comment)
    {
        for (int i=0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "Message":
                    child.GetComponent<Text>().text = message;
                    break;
                case "Comment":
                    child.GetComponent<Text>().text = comment;
                    break;
                default:
                    break;
            }
        }
        gameObject.SetActive(true);
    }

    public void OnRetryPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMenuPressed()
    {
        SceneManager.LoadScene("Title Scene");
    }
}
