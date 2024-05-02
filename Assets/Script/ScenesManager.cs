using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    public GameObject helpObj;
    public GameObject questWindow;
    public void MainScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void Update()
    {
        
        if (PlayerController.instance.Hp == 0)
        {
            SceneManager.LoadScene("GameOver");
        }
        if (PlayerController.instance.isGameClear)
        {
            SceneManager.LoadScene("GameClear");
        }
       
    }

    public void Help()
    {
        helpObj.SetActive(true);
    }
    public void HelpExit()
    {
        helpObj.SetActive(false);
    }

    public void isQuestExit()
    {
        helpObj.SetActive(false);
        questWindow.SetActive(true);
        PlayerController.instance.isQuestOff = false;
    }

    public void  Exit()
    {
    
        Application.Quit();
    }

   
}
