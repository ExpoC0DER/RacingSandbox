using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    void Start()
    {
        Time.timeScale = 1;
    }

    public void PlayGame()
   {
      SceneManager.LoadScene("GameScene");
   }
    public void QuitGame()
   {
    Application.Quit();
   }
    
}
