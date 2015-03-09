using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class PlayerController : MonoBehaviour
{
    public Material redMat, greenMat, blueMat;
    private int bubbleType = 0;
    private float surviveTime = 0.0f;
    public Text gameTimer, startTimer;
    private HighscoreManager scoreManager;
    public GameObject pauseScreen, gameOverScreen;
    private float inputx, inputy;
    public bool menuMode = false;
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

    void Awake()
    {
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
            //TODO add timer before skip shows up
            Advertisement.Show(null, new ShowOptions {
                pause = true,
                resultCallback = result => {
                    Debug.Log(result.ToString());
                }
            });
        }
    }

//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 10, 150, 50), Advertisement.isReady() ? "Show Ad" : "Waiting..."))
//        {
//            // Show with default zone, pause engine and print result to debug log
//            Advertisement.Show(null, new ShowOptions {
//                pause = true,
//                resultCallback = result => {
//                    Debug.Log(result.ToString());
//                }
//            });
//        }
//    }

    #endregion

    // Use this for initialization
    void Start()
    {
        //QualitySettings.vSyncCount = 0;
        //QualitySettings.SetQualityLevel(8, true);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        LoadSettings();
        scoreManager = GetComponent<HighscoreManager>();
        if (!menuMode)
        {
            StartCoroutine(ColorSwitch());
            StartCoroutine(TrackSurviveTime());
        }

    }

    void OnDestroy()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Settings.sts";
        if (File.Exists(fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fileStr = new FileStream (fileName, FileMode.Open, FileAccess.Read))
            {
                gameSettings = (GameSettings)bf.Deserialize(fileStr);
            }
        } else
        {
            gameSettings = new GameSettings();
        }
    }

    private void SaveSettings()
    {
        string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Settings.sts";
        BinaryFormatter bf = new BinaryFormatter();
        Directory.CreateDirectory(Application.persistentDataPath + @"/BubbleSurvivor");
        using (FileStream fileStr = new FileStream (fileName, FileMode.Create, FileAccess.Write))
        {
            bf.Serialize(fileStr, gameSettings);
        }
    }
	
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !menuMode)
        {
            PauseGame();
        }
    }

    void FixedUpdate()
    {
        if (!menuMode)
        {
            MovePlayer();
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
        Application.LoadLevel(1);
    }

    public void ToMenu()
    {
        Time.timeScale = 1;
        Application.LoadLevel(0);
    }

    private void MovePlayer()
    {
        if (gameSettings.useAccelerometer)
        {
            inputx = Input.acceleration.x * 20f;
            inputy = Input.acceleration.y * 20f;
        } else
        {
            inputx = Input.GetAxis("Horizontal") * 10f;
            inputy = Input.GetAxis("Vertical") * 10f;
        }
        GetComponent<Rigidbody2D>().velocity = new Vector2(inputx, inputy);
    }

    private void SetTimerText()
    {
        surviveTime += Time.deltaTime;
        gameTimer.text = surviveTime.ToString("n2");
        if (transform.localScale.x <= 0.1f)
        {
            Time.timeScale = 0;
            gameTimer.text = surviveTime.ToString("n2");
            if (gameSettings.vibes)
                Handheld.Vibrate();

            gameOverScreen.SetActive(true);
            if (scoreManager.RecordValue(surviveTime))
            {
                gameOverScreen.transform.FindChild("HighScoreText").gameObject.SetActive(true);
            }
            ShowAds(scoreManager.ShowAds());
        }
    }

    IEnumerator TrackSurviveTime()
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
        for (;;)
        {
            SetTimerText();
            yield return new WaitForFixedUpdate();
        }

    }

    IEnumerator ColorSwitch()
    {
        for (;;)
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
    void OnCollisionEnter2D(Collision2D other)
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

    IEnumerator Absorb(float scale)
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

}
