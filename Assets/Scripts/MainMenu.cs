using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    private PlayerController playerControl;
    private HighscoreManager scoreManager;
    public Text scoreText;
    public Toggle accel, sounds, shake;
    public GameObject quitMenu, mainMenu, optionsMenu, scoreMenu;

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

    void Start()
    {
        playerControl = GetComponent<PlayerController>();
        scoreManager = GetComponent<HighscoreManager>();
    }
	
    void Update()
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
    }

    public void Play()
    {
        Application.LoadLevel(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
