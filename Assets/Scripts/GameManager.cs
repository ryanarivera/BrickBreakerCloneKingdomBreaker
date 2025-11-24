using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject playerPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ballsText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI highscoreText;

    public GameObject panelMenu;
    public GameObject panelPlay;
    public GameObject panelLevelCompleted;
    public GameObject panelGameOver;

    public GameObject[] levels;

    public static GameManager Instance { get; private set; }

    public enum State { MENU, INIT, PLAY, LEVELCOMPLETED, LOADLEVEL, GAMEOVER}
    State _state;
    GameObject _currentBall;
    GameObject _currentLevel;
    bool _isSwitchingState;

    private int _score;

    public int Score
    {
        get { return _score; }
        set { _score = value; 
            scoreText.text = "SCORE: " + _score;
        }
    }
    
    private int _level;

    public int Level
    {
        get { return _level; }
        set { _level = value;
            levelText.text = "LEVEL: " + _level;
        }
    }

    private int _balls;

    public int Balls
    {
        get { return _balls; }
        set { _balls = value;
            ballsText.text = "BALLS: " + _balls;
        }
    }
    

    public void PlayClicked()
    {
        SwitchState(State.INIT);
    }

    void Start()
    {
        Instance = this;
        SwitchState(State.MENU);
        //PlayerPrefs.DeleteKey("highscore"); // uncomment and start game to reset high score
    }


    public void SwitchState(State newState, float delay = 0)
    {
        StartCoroutine(SwitchDelay(newState, delay));
    }

    IEnumerator SwitchDelay(State newState, float delay)
    {
        _isSwitchingState = true;
        yield return new WaitForSeconds(delay);
        EndState();
        _state = newState;
        BeginState(newState);
        _isSwitchingState = false;
    }

    void BeginState(State newState)
    {
        switch (newState)
        {
            case State.MENU:
                Cursor.visible = true;
                highscoreText.text = "HIGHSCORE: " + PlayerPrefs.GetInt("highscore");
                panelMenu.SetActive(true);
                break;
            case State.INIT:
                Cursor.visible = false;
                panelPlay.SetActive(true);
                Score = 0;
                Level = 0;
                Balls = 3;
                if (_currentLevel != null)
                {
                    Destroy(_currentLevel);
                }
                Instantiate(playerPrefab);
                SwitchState(State.LOADLEVEL);
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                Destroy(_currentBall);
                Destroy(_currentLevel);
                Level++;
                panelLevelCompleted.SetActive(true);
                SwitchState(State.LOADLEVEL, 2f);
                break;
            case State.LOADLEVEL:
                if (Level >= levels.Length)
                {
                    SwitchState(State.GAMEOVER);
                }
                else
                {
                    _currentLevel = Instantiate(levels[Level]);
                    SwitchState(State.PLAY);
                }
                break;
            case State.GAMEOVER:
                if (Score > PlayerPrefs.GetInt("highscore"))
                {
                    PlayerPrefs.SetInt("highscore", Score);
                }
                panelGameOver.SetActive(true);
                break;
        }
    }

    void Update()
    {
        switch (_state)
        {
            case State.MENU:
                break;
            case State.INIT:
                break;
            case State.PLAY:
                if (_currentBall == null)
                {
                    if (Balls > 0)
                    {
                        _currentBall = Instantiate(ballPrefab);
                    }
                    else
                    {
                        SwitchState(State.GAMEOVER);
                    }
                }
                if (_currentLevel != null && _currentLevel.transform.childCount == 0 && !_isSwitchingState)
                {
                    SwitchState(State.LEVELCOMPLETED);
                }
                break;
            case State.LEVELCOMPLETED:
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                bool anyKey = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
                bool anyMouse = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);
                if (anyKey || anyMouse)
                {
                    SwitchState(State.MENU);
                }
                break;
        }
    }

    void EndState()
    {
        switch (_state)
        {
            case State.MENU:
                panelMenu.SetActive(false);
                break;
            case State.INIT:
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                panelLevelCompleted.SetActive(false);
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                panelPlay.SetActive(false);
                panelGameOver.SetActive(false);
                break;
        }
    }
}
