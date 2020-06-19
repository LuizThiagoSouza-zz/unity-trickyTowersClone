using Borodar.FarlandSkies.LowPoly;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int heightToWin = 40;
    public int initialLifes = 3;
    public float fallingSpeed = 1.0f;
    public float maxAccelerationDuration = 2f;
    public float maxAccelerationModifier = 5.0f;
    public float cloudsSpeed = 2.0f;
    public AudioSource postGameWindowAudio;
    public List<AudioClip> postGameAudioClips;
    public GameObject flagSprite;
    public GameObject winnerLettering;
    public GameObject youLoseLettering;
    public TMP_Text lifesLabel;
    public TMP_Text postGameTitleLabel;
    public TMP_Text postGameMessageLabel;
    public UnityEvent onPrepareGame;
    public UnityEvent onGameStart;
    public UnityEvent onGameOver;
    public UnityEvent onGameWin;
    public Animator goAnimator;
    public Animator postGameWindowAnimator;
    public Animator fadeScreenAnimator;
    public Animator finalLetteringAnimator;

    private GameState state;
    private int currentLifes;
    private List<ResettableComponent> resettableComponents;

    public static float FallingSpeed { get { return Instance.fallingSpeed; } }
    public static float MaxAccelerationDuration { get { return Instance.maxAccelerationDuration; } }
    public static float MaxAccelerationModifier { get { return Instance.maxAccelerationModifier; } }
    public static GameState State {  get { return Instance.state; } }
    public static int CurrentLifes { get { return Instance.currentLifes; } }

    #region <--- MONOBEHAVIOURS --->

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;
    }

    private void Start()
    {
        resettableComponents = new List<ResettableComponent>(FindObjectsOfType<ResettableComponent>());

        if (onPrepareGame != null)
            onPrepareGame.Invoke();

        fallingSpeed = fallingSpeed < 1 ? 1 : fallingSpeed;
        maxAccelerationDuration = maxAccelerationDuration < 0.01f ? 0.01f : maxAccelerationDuration;
        maxAccelerationModifier = maxAccelerationModifier < 1 ? 1 : maxAccelerationModifier;
        winnerLettering.SetActive(false);
        youLoseLettering.SetActive(false);
        StartGame();
    }

    private void Update()
    {
        SkyboxController.Instance.CloudsRotation += cloudsSpeed * Time.deltaTime;
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region <--- START GAME METHODS --->

    public static void StartGame()
    {
        Instance.StopAllCoroutines();
        Instance.state = GameState.Starting;
        Instance.StartCoroutine(Instance.ProcessStartGame());
    }

    private IEnumerator ProcessStartGame()
    {
        winnerLettering.SetActive(false);
        youLoseLettering.SetActive(false);

        var flagPosition = flagSprite.transform.position;
        flagPosition.y = heightToWin + 0.25f;
        flagSprite.transform.position = flagPosition;

        fadeScreenAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        goAnimator.SetTrigger("Run");
        yield return new WaitForSeconds(3f);
        state = GameState.Playing;
        if (onGameStart != null)
            onGameStart.Invoke();
    }

    #endregion <--- START GAME METHODS --->
   
    #region <--- RESTART GAME METHODS --->

    public void RestartGame()
    {
        StopAllCoroutines();
        Instance.StartCoroutine(Instance.HidePostGameWindow(() =>
        {
            if (onPrepareGame != null)
                onPrepareGame.Invoke();
            StartGame();
        }));
    }

    private IEnumerator HidePostGameWindow(UnityAction onHide)
    {
        postGameWindowAnimator.SetBool("Show", false);
        yield return new WaitForSeconds(0.5f);
        fadeScreenAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(2f);

        if (onHide != null)
            onHide.Invoke();
    }

    public void ResetComponents()
    {
        ResetLifes();

        foreach (var resettableComponent in resettableComponents)
            resettableComponent.ResetComponent();
    }

    private void ResetLifes()
    {
        currentLifes = initialLifes;
        lifesLabel.SetText("x" + CurrentLifes);
    }

    #endregion <--- RESTART GAME METHODS --->

    #region <--- GAME WIN METHODS --->

    public static void GameWin()
    {
        if (Instance.onGameWin != null)
            Instance.onGameWin.Invoke();

        PieceSpawner.Instance.FreezeAllPieces();

        Instance.postGameWindowAudio.PlayOneShot(Instance.postGameAudioClips[0]);
        Instance.finalLetteringAnimator.SetBool("Show", true);
        Instance.winnerLettering.SetActive(true);
        Instance.youLoseLettering.SetActive(false);

        Instance.state = GameState.Win;
        Instance.StartCoroutine(Instance.ShowGameWinWindow());
    }

    private IEnumerator ShowGameWinWindow()
    {
        yield return new WaitForSeconds(3f);

        Instance.finalLetteringAnimator.SetBool("Show", false);
        Instance.winnerLettering.SetActive(false);
        Instance.winnerLettering.SetActive(false);

        postGameTitleLabel.SetText("You Win");
        postGameMessageLabel.SetText("You reached the height of " + heightToWin + "m! Congratulations!");
        postGameWindowAnimator.SetBool("Show", true);
    }

    #endregion <--- GAME WIN METHODS --->

    #region <--- GAME OVER METHODS --->

    public static void GameOver()
    {
        if (Instance.onGameOver != null)
            Instance.onGameOver.Invoke();

        Instance.state = GameState.GameOver;
        Instance.StartCoroutine(Instance.ShowGameOverWindow());

        Instance.postGameWindowAudio.PlayOneShot(Instance.postGameAudioClips[1]);
        Instance.finalLetteringAnimator.SetBool("Show", true);
        Instance.youLoseLettering.SetActive(true);
        Instance.winnerLettering.SetActive(false);

        PieceSpawner.Instance.FreezeAllPieces();
    }

    private IEnumerator ShowGameOverWindow()
    {
        yield return new WaitForSeconds(3f);

        Instance.finalLetteringAnimator.SetBool("Show", false);
        Instance.youLoseLettering.SetActive(false);
        Instance.winnerLettering.SetActive(false);

        postGameTitleLabel.SetText("Game Over");
        postGameMessageLabel.SetText("You reached the height of " + PlayerController.Instance.cameraController.heightMeter.Height + "m!");
        postGameWindowAnimator.SetBool("Show", true);
    }

    #endregion <--- GAME OVER METHODS --->

    #region <--- PUBLIC METHODS --->

    public static void AddLife(int amount)
    {
        if (State != GameState.Playing) return;

        Instance.currentLifes += amount;

        if (Instance.currentLifes <= 0)
        {
            Instance.currentLifes = 0;
            GameOver();
        }

        Instance.lifesLabel.SetText("x" + CurrentLifes);
    }

    public static void SetState(GameState gameState)
    {
        Instance.state = gameState;
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine("GoToMainMenu");
    }

    private IEnumerator GoToMainMenu()
    {
        fadeScreenAnimator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        bool isDone = false;

        while (!isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                isDone = true;

            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        asyncLoad.allowSceneActivation = true;
        yield break;
    }

    public void PauseGame()
    {
        state = GameState.Paused;
    }

    public void ResumeGame()
    {
        state = GameState.Playing;
    }

    #endregion <--- PUBLIC METHODS --->
}

#region <--- EDITOR --->

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private SerializedObject me;

    private SerializedProperty heightToWin;
    private SerializedProperty initialLifes;
    private SerializedProperty fallingSpeed;
    private SerializedProperty maxAccelerationDuration;
    private SerializedProperty maxAccelerationModifier;
    private SerializedProperty cloudsSpeed;
    private SerializedProperty goAnimator;
    private SerializedProperty postGameWindowAnimator;
    private SerializedProperty lifesLabel;
    private SerializedProperty postGameTitleLabel;
    private SerializedProperty postGameMessageLabel;
    private SerializedProperty onPrepareGame;
    private SerializedProperty onGameStart;
    private SerializedProperty onGameOver;
    private SerializedProperty onGameWin;
    private SerializedProperty fadeScreenAnimator;
    private SerializedProperty flagSprite;
    private SerializedProperty postGameWindowAudio;
    private SerializedProperty postGameAudioClips;
    private SerializedProperty finalLetteringAnimator;
    private SerializedProperty winnerLettering;
    private SerializedProperty youLoseLettering;


    private int currentTab;

    private void OnEnable()
    {
        me = new SerializedObject(target);

        heightToWin = me.FindProperty("heightToWin");
        initialLifes = me.FindProperty("initialLifes");
        fallingSpeed = me.FindProperty("fallingSpeed");
        maxAccelerationDuration = me.FindProperty("maxAccelerationDuration");
        maxAccelerationModifier = me.FindProperty("maxAccelerationModifier");
        cloudsSpeed = me.FindProperty("cloudsSpeed");
        goAnimator = me.FindProperty("goAnimator");
        postGameWindowAnimator = me.FindProperty("postGameWindowAnimator");
        lifesLabel = me.FindProperty("lifesLabel");
        postGameTitleLabel = me.FindProperty("postGameTitleLabel");
        postGameMessageLabel = me.FindProperty("postGameMessageLabel");
        onPrepareGame = me.FindProperty("onPrepareGame");
        onGameStart = me.FindProperty("onGameStart");
        onGameOver = me.FindProperty("onGameOver");
        onGameWin = me.FindProperty("onGameWin");
        fadeScreenAnimator = me.FindProperty("fadeScreenAnimator");
        flagSprite = me.FindProperty("flagSprite");
        postGameWindowAudio = me.FindProperty("postGameWindowAudio");
        postGameAudioClips = me.FindProperty("postGameAudioClips");
        finalLetteringAnimator = me.FindProperty("finalLetteringAnimator");
        winnerLettering = me.FindProperty("winnerLettering");
        youLoseLettering = me.FindProperty("youLoseLettering");
    }

    public override void OnInspectorGUI()
    {
        me.Update();

        EditorGUILayout.Space();

        currentTab = GUILayout.Toolbar(currentTab, new[] { "Properties", "References", "Callbacks" });
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(0.25f));

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        switch (currentTab)
        {
            case 1:
                DrawReferencesTab();
                break;
            case 2:
                DrawCallbacksTab();
                break;
            default:
                DrawPropertiesTab();
                break;
        }

        EditorGUILayout.Space();

        me.ApplyModifiedProperties();
    }

    private void DrawPropertiesTab()
    {
        EditorGUILayout.PropertyField(heightToWin);
        if (heightToWin.intValue <= 0)
            heightToWin.intValue = 1;

        EditorGUILayout.PropertyField(initialLifes);
        if (initialLifes.intValue <= 0)
            initialLifes.intValue = 1;

        EditorGUILayout.PropertyField(fallingSpeed);
        if (fallingSpeed.floatValue <= 0)
            fallingSpeed.floatValue = 1;

        EditorGUILayout.PropertyField(cloudsSpeed);
        if (cloudsSpeed.floatValue <= 0)
            cloudsSpeed.floatValue = 1;

        EditorGUILayout.PropertyField(maxAccelerationDuration);
        if (maxAccelerationDuration.floatValue <= 0)
            maxAccelerationDuration.floatValue = 1;

        EditorGUILayout.PropertyField(maxAccelerationModifier);
        if (maxAccelerationModifier.floatValue <= 0)
            maxAccelerationModifier.floatValue = 1;
    }

    private void DrawReferencesTab()
    {
        EditorGUILayout.PropertyField(flagSprite);
        EditorGUILayout.PropertyField(goAnimator);
        EditorGUILayout.PropertyField(postGameWindowAnimator);
        EditorGUILayout.PropertyField(fadeScreenAnimator);
        EditorGUILayout.PropertyField(lifesLabel);
        EditorGUILayout.PropertyField(postGameTitleLabel);
        EditorGUILayout.PropertyField(postGameMessageLabel);
        EditorGUILayout.PropertyField(finalLetteringAnimator);
        EditorGUILayout.PropertyField(winnerLettering);
        EditorGUILayout.PropertyField(youLoseLettering);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audios:");
        EditorGUILayout.PropertyField(postGameWindowAudio);
        EditorGUILayout.PropertyField(postGameAudioClips, true);
    }

    private void DrawCallbacksTab()
    {
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onPrepareGame);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onGameStart);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onGameOver);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onGameWin);
        EditorGUILayout.Space();
    }
}

#endif

#endregion <--- EDITOR --->