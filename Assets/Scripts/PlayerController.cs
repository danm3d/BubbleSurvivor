﻿using GooglePlayGames;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Material redMat, greenMat, blueMat;
    private int bubbleType = 0;
    public float accelInitialX = 0, accelInitialY = 0;
    private float surviveTime = 0.0f;
    public Text gameTimer, startTimer;
    private HighscoreManager scoreManager;
    public GameObject pauseScreen, gameOverScreen;
    private Vector2 direction = Vector3.zero;
    public float scaleMagnitude = 0;

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

    [Serializable]
    public class GameSettings
    {
        public bool useAccelerometer = true;
        public bool sounds = true;
        public bool vibes = true;
    }

    private GameSettings gameSettings;

    public GameSettings CurrentGameSettings
    {
        get
        {
            return gameSettings;
        }
        set
        {
            gameSettings = value;
        }
    }

    public PositionBorder border;

	#region unity Ads

    private void Awake()
    {
        if (!menuMode)
        {
            myRend = GetComponent<Renderer>();
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
        //Record initial rotation to adjust for starting rotation.
        accelInitialX = Input.acceleration.x;
        accelInitialY = Input.acceleration.y;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        LoadSettings();
        scoreManager = GetComponent<HighscoreManager>();
        if (!menuMode)
        {
            StartCoroutine(ColorSwitch());
            StartCoroutine(TrackSurviveTime());
        }
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Settings.sts";
        if (File.Exists(fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fileStr = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                gameSettings = (GameSettings)bf.Deserialize(fileStr);
            }
        } else
        {
            gameSettings = new GameSettings();
        }
        Camera.main.gameObject.GetComponent<AudioSource>().enabled = gameSettings.sounds;
    }

    private void SaveSettings()
    {
        string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Settings.sts";
        BinaryFormatter bf = new BinaryFormatter();
        Directory.CreateDirectory(Application.persistentDataPath + @"/BubbleSurvivor");
        using (FileStream fileStr = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            bf.Serialize(fileStr, gameSettings);
        }
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
            scaleMagnitude = transform.localScale.magnitude;
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
        pauseScreen.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
        Application.LoadLevel(Application.loadedLevelName);//oh yes indeed... So much replay...
    }

    public void ToMenu()
    {
        Time.timeScale = 1;
        Application.LoadLevel("MainMenu");
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
        GetComponent<Rigidbody2D>().velocity = direction;
    }

    private void SetTimerText()
    {
        surviveTime += Time.deltaTime;
        AchievementCheck();
        gameTimer.text = surviveTime.ToString("n2");
        if (transform.localScale.x <= 0.1f)
        {
            Time.timeScale = 0;
            roundOver = true;
            gameTimer.text = surviveTime.ToString("n2");
            if (gameSettings.vibes)
                Handheld.Vibrate();

            gameOverScreen.SetActive(true);
            if (scoreManager.RecordValue(surviveTime))
            {
                gameOverScreen.transform.FindChild("HighScoreText").gameObject.SetActive(true);
                PlayGamesPlatform.Instance.ReportScore((long)surviveTime, "CgkIif2dm5QIEAIQAg", (bool success) =>
                {
                    Debug.Log("Score Logged");
                });

            }
            ShowAds(scoreManager.ShowAds());
        }
    }

    private IEnumerator TrackSurviveTime()
    {
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
                GetComponent<Renderer>().material = redMat;
            else if (bubbleType == 1)
                GetComponent<Renderer>().material = greenMat;
            else
                GetComponent<Renderer>().material = blueMat;
            border.SetMaterial(GetComponent<Renderer>().material);
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
            } else
            {
                if (gameSettings.vibes)
                    Handheld.Vibrate();
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
