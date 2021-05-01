using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour
{

    public string Client;
    public string Server;

    public void LoadClient()
    {
        SceneManager.LoadScene(Client);
    }

    public void LoadServer()
    {
        SceneManager.LoadScene(Server);
    }
}
