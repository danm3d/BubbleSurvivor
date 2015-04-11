using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private PlayerController playerControl;
    private HighscoreManager scoreManager;
    public Text scoreText, qualityLabel;
    public Toggle accel, sounds, shake;
    public GameObject quitMenu, mainMenu, optionsMenu, scoreMenu, social;
    private AudioSource clickAudio;

    public bool Accelerometer
    {
        get
        {
            return playerControl.CurrentGameSettings.useAccelerometer;
        }
        set
        {
            playerControl.CurrentGameSettings.useAccelerometer = value;
        }
    }

    public bool Sounds
    {
        get
        {
            return playerControl.CurrentGameSettings.sounds;
        }
        set
        {
            playerControl.CurrentGameSettings.sounds = value;
            //Camera.main.GetComponents<AudioSource>().enabled = value;
            foreach (AudioSource audioSource in Camera.main.GetComponents<AudioSource>())
            {
                audioSource.enabled = value;
            }
        }
    }

    public bool Vibes
    {
        get
        {
            return playerControl.CurrentGameSettings.vibes;
        }
        set
        {
            playerControl.CurrentGameSettings.vibes = value;
        }
    }

    private void Start()
    {
        playerControl = GetComponent<PlayerController>();
        scoreManager = GetComponent<HighscoreManager>();
        social.GetComponent<GooglePlay_Social>().SignInUser();
        Sounds = playerControl.CurrentGameSettings.sounds;
        clickAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenu.activeSelf)
            {
                optionsMenu.SetActive(false);
                mainMenu.SetActive(true);
            } else if (scoreMenu.activeSelf)
            {
                scoreMenu.SetActive(false);
                mainMenu.SetActive(true);
            } else
                quitMenu.SetActive(true);
        }
    }

    public void DisplayHighscore()
    {
        scoreText.text = scoreManager.GetScores();
    }

    public void Options()
    {
        accel.isOn = Accelerometer;
        sounds.isOn = Sounds;
        shake.isOn = Vibes;
        qualityLabel.text = QualitySettings.names [QualitySettings.GetQualityLevel()];
    }

    public void Play(int level)
    {
        Application.LoadLevel(level);
    }

    public void Play(string levelName)
    {
        Application.LoadLevel(levelName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void IncreaseQuality()
    {
        QualitySettings.IncreaseLevel();
        qualityLabel.text = QualitySettings.names [QualitySettings.GetQualityLevel()];
        playerControl.CurrentGameSettings.qualityLevel = QualitySettings.GetQualityLevel();
    }

    public void DecreaseQuality()
    {
        QualitySettings.DecreaseLevel();
        qualityLabel.text = QualitySettings.names [QualitySettings.GetQualityLevel()];
        playerControl.CurrentGameSettings.qualityLevel = QualitySettings.GetQualityLevel();
    }

    public void ButtonClickSound()
    {
        if (Sounds)
        {
            clickAudio.Play();
        }
    }

}