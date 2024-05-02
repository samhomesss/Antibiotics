using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    public static ScenesManager instance;
    public GameObject helpObj;
    public void MainScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Help()
    {
        helpObj.SetActive(true);
    }
    public void HelpExit()
    {
        helpObj.SetActive(false);
    }

    public void Exit()
    {


        Application.Quit();
    }

}
