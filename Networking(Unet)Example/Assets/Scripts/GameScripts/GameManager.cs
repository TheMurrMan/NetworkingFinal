using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Text p1ScoreText;
    public Text p2ScoreText;

    int player1Score;

	private void Awake()
	{
		
	}

	// Start is called before the first frame update
	void Start()
    {
        if(instance == null)
		{
            instance = this;
		}
    }

    // Update is called once per frame
    void Update()
    {
        p1ScoreText.text = "Score: " + player1Score;
    }

    public void IncreaseScore(int score)
	{
        player1Score += score;
	}
}
