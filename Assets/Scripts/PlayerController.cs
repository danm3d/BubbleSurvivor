using GooglePlayGames;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    [Serializable]
    private class AudioPlaytime
    {
        public float playTime = 0.0f;
    }
    private AudioPlaytime audioPlaytime;//save play time of the music for replaying a level
    private AudioSource musicAudio, effectsAudio;
    public AudioClip inflateSound, deflateSound;

    public Material redMat, greenMat, blueMat, neutralMat;
    private int bubbleType = 0;
    private float accelInitialX = 0, accelInitialY = 0;
    private float surviveTime = 0.0f;
    private string surviveTimestring;//string used for formatting the time
    public Text gameTimer, startTimer;
    private HighscoreManager scoreManager;
    public GameObject pauseScreen, gameOverScreen;
    private Vector2 direction = Vector3.zero;
    public bool arenaMode = true;

    /// Gets the current move/tilt direction.
    public Vector2 Direction
    {
        get
        {
            return direction;
        }
    }

    //movement direction
    public bool menuMode = false;//is this script currently used in the main menu

    public bool roundStarted = false;//has the round started
    private bool roundOver = false;//has the current round ended
    private Renderer myRend;//reference to the renderer on this object
    private bool isWrappingX, isWrappingY;
    private bool inTrigger = false;//is the player currently in a trigger area
    private Rigidbody2D myRigidbody;//reference to the rigidbody on the player

    [Serializable]
    public class GameSettings
    {
        public bool useAccelerometer = true;
        public bool sounds = true;
        public bool vibes = true;
        public int qualityLevel;
    }
    private GameSettings gameSettings;

    public GameSettings CurrentGameSettings
    {
        get
        {
            if (gameSettings == null)
                LoadSettings();
            return gameSettings;
        }
        set
        {
            gameSettings = value;
        }
    }

    public PositionBorder border;//used to change the color of the borders

	#region unity Ads

    private void Awake()
    {
        musicAudio = Camera.main.GetComponent<AudioSource>();
        if (!menuMode)
        {
            myRend = GetComponent<Renderer>();
            myRigidbody = GetComponent<Rigidbody2D>();
            effectsAudio = GetComponent<AudioSource>();
        }
        if (Advertisement.isSupported)
        {
            Advertisement.allowPrecache = true;
            Advertisement.Initialize("25086", false);
        } else
        {
            Debug.Log("Platform not supported");
        }
    }

    private void ShowAds(bool show)
    {
        if (show && Advertisement.isReady())
        {
            // Show with default zone, pause engine and print result to debug log
            Advertisement.Show(null, new ShowOptions
			{
				pause = true,
				resultCallback = result =>
				{
					Debug.Log(result.ToString());
				}
			});
        }
    }

	#endregion unity Ads

    // Use this for initialization
    private void Start()
    {
        RecordInitialRotation();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        LoadSettings();
        scoreManager = GetComponent<HighscoreManager>();
        if (!menuMode)
        {
            if (arenaMode)
            {
                StartCoroutine(ColorSwitch());
            } else
            {
                bubbleType = 4;
                myRend.material = neutralMat;
            }
            StartCoroutine(TrackSurviveTime());
        }
    }

    private void RecordInitialRotation()
    {
        //Record initial rotation to adjust for starting rotation.
        accelInitialX = Mathf.Clamp(Input.acceleration.x, -0.2f, 0.2f);
        accelInitialY = Mathf.Clamp(Input.acceleration.y, -0.2f, 0.2f);
    }

    private void OnDestroy()
    {
        SaveAudio();
        SaveSettings();
    }

    private void LoadAudio()
    {
        audioPlaytime = Utilities.LoadClass<AudioPlaytime>("/BubbleSurvivor/audPlay.ap");
        if (audioPlaytime == null)
            audioPlaytime = new AudioPlaytime();
        musicAudio.time = audioPlaytime.playTime;
        musicAudio.Play();
    }

    private void SaveAudio()
    {
        Utilities.SaveClass("/BubbleSurvivor/audPlay.ap", audioPlaytime);
    }

    private void LoadSettings()
    {
        gameSettings = Utilities.LoadClass<GameSettings>("/BubbleSurvivor/Settings.sts");
        if (gameSettings == null)
            gameSettings = new GameSettings();
        musicAudio.enabled = gameSettings.sounds;
        QualitySettings.SetQualityLevel(gameSettings.qualityLevel);
        if (!menuMode)
            LoadAudio();
    }

    private void SaveSettings()
    {
        Utilities.SaveClass("/BubbleSurvivor/Settings.sts", gameSettings);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !menuMode && !roundOver)
        {
            PauseGame();
        }
    }

    private void FixedUpdate()
    {
        if (!menuMode)
        {
            MovePlayer();
            ScreenWrap();
        }
    }

    ///Wrap the player around the screen.
    private void ScreenWrap()
    {
        if (!myRend.isVisible)
        {
            if (isWrappingX && isWrappingY)
            {
                return;
            }
            Vector3 newPos = transform.position;
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            if (!isWrappingX && (viewportPos.x > 1 || viewportPos.x < 0))
            {
                newPos.x = -newPos.x;
                isWrappingX = true;
            }

            if (!isWrappingY && (viewportPos.y > 1 || viewportPos.y < 0))
            {
                newPos.y = -newPos.y;
                isWrappingY = true;
            }
            transform.position = newPos;
        } else
        {
            isWrappingX = false;
            isWrappingY = false;
            return;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        musicAudio.Pause();
        pauseScreen.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        musicAudio.UnPause();
        pauseScreen.SetActive(false);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
        audioPlaytime.playTime = musicAudio.time;
        Application.LoadLevel(Application.loadedLevelName);//oh yes indeed... So much replay...
    }

    public void ToMenu()
    {
        Time.timeScale = 1;
        audioPlaytime.playTime = 0.0f;
        Application.LoadLevel("MainMenu");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        inTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        inTrigger = false;
    }

    private void MovePlayer()
    {
        direction.x = (Input.acceleration.x - accelInitialX);
        direction.y = (Input.acceleration.y - accelInitialY);
        if (direction == Vector2.zero)
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
        }

        if (direction.sqrMagnitude > 1)
        {
            direction.Normalize();
        }
        direction = new Vector3(20f * direction.x, 20f * direction.y);
        //anti cheating for trigger areas. you can fight them, but you cant beat them
        if (!inTrigger)
        {
            myRigidbody.velocity = (direction);
        } else
        {
            myRigidbody.AddForce(direction);
        }
    }

    private void SetTimerText()
    {
        surviveTime += Time.fixedDeltaTime;
        AchievementCheck();
        surviveTimestring = string.Format("{0:00}:{1:00}.{2:0}", (int)Mathf.Floor(surviveTime) / 60, (int)Mathf.Floor(surviveTime) % 60, ((surviveTime - Mathf.Floor(surviveTime)) * 10f));
        gameTimer.text = surviveTimestring;
        if (transform.localScale.x <= 0.1f)
        {
            Time.timeScale = 0;
            musicAudio.Pause();
            var reportedScore = surviveTime * 1000f;
            roundOver = true;
            surviveTimestring = string.Format("{0:00}:{1:00}.{2:00}", (int)Mathf.Floor(surviveTime) / 60, (int)Mathf.Floor(surviveTime) % 60, ((surviveTime - Mathf.Floor(surviveTime)) * 100f));
            gameTimer.text = surviveTimestring;
            if (gameSettings.vibes)
                Handheld.Vibrate();

            gameOverScreen.SetActive(true);
            if (scoreManager.RecordValue(surviveTime))
            {
                gameOverScreen.transform.FindChild("HighScoreText").gameObject.SetActive(true);
                //can only post to the online leaderboard when logged in
                if (PlayGamesPlatform.Instance.localUser.authenticated)
                {
                    //TODO ensure logged time here is correct
                    PlayGamesPlatform.Instance.ReportScore((long)reportedScore, "CgkIif2dm5QIEAIQAg", (bool success) =>
                    {
                        Debug.Log("Score Logged");
                    });
                }
            }
            ShowAds(scoreManager.ShowAds());
        }
    }

    private IEnumerator TrackSurviveTime()
    {
        //just a once off set of the timer text so it doesnt look so funny at the start.
        surviveTimestring = string.Format("{00}:{1:00}.{2:0}", (int)surviveTime / 60, (int)surviveTime % 60, (int)((surviveTime - Mathf.Floor(surviveTime)) * 10f));
        gameTimer.text = surviveTimestring;
        int startDelay = 3;
        startTimer.gameObject.SetActive(true);
        while (startDelay > 0)
        {
            startTimer.text = (startDelay).ToString();
            startDelay -= 1;
            yield return new WaitForSeconds(1f);
        }
        startTimer.gameObject.SetActive(false);
        roundStarted = true;
        for (; ;)
        {
            SetTimerText();
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ColorSwitch()
    {
        for (; ;)
        {
            bubbleType = UnityEngine.Random.Range(0, 3);
            if (bubbleType == 0)
                myRend.material = redMat;
            else if (bubbleType == 1)
                myRend.material = greenMat;
            else
                myRend.material = blueMat;
            border.SetMaterial(myRend.material);
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 15f));
        }
    }

    private BubbleBehaviour bubBehav;

    private void OnCollisionEnter2D(Collision2D other)
    {
        bubBehav = other.gameObject.GetComponent<BubbleBehaviour>();
        if (bubBehav != null)
        {
            if ((int)bubBehav.bubbleType == bubbleType)
            {
                StartCoroutine(Absorb(other.transform.localScale.x / 16f));
                bubBehav.Absorb();
                if (gameSettings.sounds)
                {
                    effectsAudio.PlayOneShot(inflateSound);
                }
            } else
            {
                if (gameSettings.vibes)
                    Handheld.Vibrate();
                if (gameSettings.sounds)
                {
                    effectsAudio.PlayOneShot(deflateSound);
                }
                StartCoroutine(Absorb(-other.transform.localScale.x / 16f));
            }
        }
    }

    private IEnumerator Absorb(float scale)
    {
        float timer = 0.0f;
        Vector3 from = Vector3.zero;
        Vector3 to = new Vector3(scale, scale, scale);
        Vector3 newScale = Vector3.zero;
        while (timer < 1.0f)
        {
            newScale = Vector3.Lerp(from, to, timer);
            newScale += transform.localScale;
            transform.localScale = newScale;

            yield return new WaitForSeconds(0.02f);
            timer += 0.02f * 4f;
        }
    }

    private void AchievementCheck()
    {
        //can only get achievements when you are signed in
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            //Baby steps
            if (surviveTime >= 20f)
            {
                PlayGamesPlatform.Instance.ReportProgress("CgkIif2dm5QIEAIQAQ", 100.0f, (bool success) =>
                {
                    Debug.Log("Achievement Unlocked! Baby steps");
                });
            }
            //Century
            if (surviveTime >= 100f)
            {
                PlayGamesPlatform.Instance.ReportProgress("CgkIif2dm5QIEAIQAw", 100.0f, (bool success) =>
                {
                    Debug.Log("Achievement Unlocked! Century");
                });
            }
            //500 miles
            if (surviveTime >= 500f)
            {
                PlayGamesPlatform.Instance.ReportProgress("CgkIif2dm5QIEAIQBg", 100.0f, (bool success) =>
                {
                    Debug.Log("Achievement Unlocked! 500 miles");
                });
            }
            //500 more
            if (surviveTime >= 1000f)
            {
                PlayGamesPlatform.Instance.ReportProgress("CgkIif2dm5QIEAIQBA", 100.0f, (bool success) =>
                {
                    Debug.Log("Achievement Unlocked! 500 more");
                });
            }
            //Steriods
            if (transform.localScale.magnitude >= 17f)
            {
                PlayGamesPlatform.Instance.ReportProgress("CgkIif2dm5QIEAIQBQ", 100.0f, (bool success) =>
                {
                    Debug.Log("Achievement Unlocked! Roids");
                });
            }
        }
    }
}