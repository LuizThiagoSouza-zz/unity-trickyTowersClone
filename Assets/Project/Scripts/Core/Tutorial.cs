using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject tutorialUI;
    public TMP_Text tutorialLabel;
    public List<string> tutorialMessages;

    private int step = -1;

    public bool AllowTutorial { get; private set; }
    public int TutorialStep { get; private set; }

    void Start()
    {
        tutorialUI.SetActive(false);

        if (PlayerPrefs.HasKey("runTutorial"))
            AllowTutorial = PlayerPrefs.GetInt("runTutorial") >= 1 ? true : false;
        else
            PlayerPrefs.SetInt("runTutorial", 1);
    }

    public void RunTutorial()
    {
        if (!AllowTutorial) return;

        Invoke("RunNextTutorialStep", 2);
    }

    public void RunNextTutorialStep()
    {
        if (step + 1 >= tutorialMessages.Count) return;

        GameManager.SetState(GameState.Tutorial);

        step++;

        tutorialLabel.SetText(tutorialMessages[step]);
        tutorialUI.gameObject.SetActive(true);
    }

    public void CompleteTutorial(int stepCheck)
    {

        if (!AllowTutorial || stepCheck != step) return;
        GameManager.SetState(GameState.Playing);

        tutorialUI.gameObject.SetActive(false);

        if (step + 1 >= tutorialMessages.Count)
        {
            PlayerPrefs.SetInt("runTutorial", 0);
            return;
        }

        Invoke("RunNextTutorialStep", 2);
    }
}
