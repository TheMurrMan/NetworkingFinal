using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
	public void Win()
	{
		SceneManager.LoadScene("Title Scene");
	}

	public void Lose()
	{
		SceneManager.LoadScene("Title Scene");
	}
}
