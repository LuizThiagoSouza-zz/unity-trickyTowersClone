using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject blanckScreen;
    public Animator menusAnimator;
    public Animator loadingScreenAnimator;
    public Animator logoAnimator;

    private MainMenuState state;

    private void Start()
    {
        state = MainMenuState.MainMenu;
        logoAnimator.SetTrigger("Run");
        logoAnimator.SetBool("Showed", true);
    }

    #region <--- PUBLIC UI METHODS --->

    public void SetModeMenuUIActive(bool isActive)
    {
        if (state == MainMenuState.MainMenu && isActive)
        {
            menusAnimator.SetBool("ShowModeMenu", true);
            state = MainMenuState.ModeMenu;
        }
        else if (state == MainMenuState.ModeMenu && !isActive)
        {
            menusAnimator.SetBool("ShowModeMenu", false);
            state = MainMenuState.MainMenu;
        }
    }

    public void SetAboutWindowActive(bool isActive)
    {
        if (state == MainMenuState.MainMenu && isActive)
        {
            menusAnimator.SetBool("ShowAboutWindow", true);
            state = MainMenuState.AboutWindow;
        }
        else if (state == MainMenuState.AboutWindow && !isActive)
        {
            menusAnimator.SetBool("ShowAboutWindow", false);
            state = MainMenuState.MainMenu;
        }
    }

    public void SetLoadingWindowActive(bool isActive)
    {
        if (state == MainMenuState.ModeMenu && isActive)
        {
            loadingScreenAnimator.SetBool("IsActive", true);
            state = MainMenuState.LoadingWindow;
        }
        else if (state == MainMenuState.LoadingWindow && !isActive)
        {
            loadingScreenAnimator.SetBool("IsActive", false);
            state = MainMenuState.ModeMenu;
        }
    }

    public void OnBackButton()
    {
        Debug.Log(state);
        if (state == MainMenuState.ModeMenu)
            SetModeMenuUIActive(false);
        else if (state == MainMenuState.AboutWindow)
            SetAboutWindowActive(false);
    }

    #endregion <--- PUBLIC UI METHODS --->

    public void LoadSinglePlayerMode()
    {
        StartCoroutine(LoadAsyncScene("SinglePlayerMode"));
    }

    public void LoadAIMode()
    {
        StartCoroutine(LoadAsyncScene("AiMode"));
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        SetLoadingWindowActive(true);

        yield return new WaitForSeconds(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        bool isDone = false;

        while (!isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                isDone = true;

            yield return null;
        }

        blanckScreen.SetActive(true);
        SetLoadingWindowActive(false);
        yield return new WaitForSeconds(1.5f);
        asyncLoad.allowSceneActivation = true;
        yield break;
    }
}
