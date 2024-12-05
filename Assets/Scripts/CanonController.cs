using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Add this for scene management
using System.Collections;
using System.Collections.Generic;

public class BasketballCannonController : MonoBehaviour
{
    [Header("Cannon Components")]
    public Transform cannonPivot;
    public Transform firePoint;
    public GameObject basketballPrefab;
    public GameObject trajectoryPointPrefab;
    public ParticleSystem shootParticleEffect;
    public ParticleSystem scoreParticleEffect;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip scoreSound;
    public AudioClip missSound;
    public AudioClip bounceSound;
    public AudioClip lastBallSound;
    public AudioClip buttonClickSound;

    [Header("Audio Settings")]
    public float audioVolume = 0.5f;

    [Header("Shooting Controls")]
    public Slider powerSlider;
    public float maxShotPower = 20f;
    public int trajectoryResolution = 50;
    public float trajectoryTimeStep = 0.1f;
    public float maxTrajectoryLength = 50f;

    [Header("Physics")]
    public float ballMass = 0.6f;
    public float gravity = -9.81f;

    [Header("Cannon Rotation")]
    public float rotationSensitivity = 0.2f;
    public float minVerticalRotation = -10f;
    public float maxVerticalRotation = 45f;
    public float minHorizontalRotation = -45f;
    public float maxHorizontalRotation = 45f;

    [Header("Game Settings")]
    public Text ballsRemainingText;
    public Text currentScoreText;
    public Text highScoreText;
    public Text statusText;
    public int maxBalls = 5;
    public int scoreNetPoints = 3;
    public int hoopPoints = 1;

    private const string HIGH_SCORE_KEY = "BasketballHighScore";
    private Camera mainCamera;
    private Vector3 initialTouchPos;
    private bool isDragging = false;
    private bool isOverSlider = false;
    private int ballsRemaining;
    private int currentScore = 0;
    private int highScore = 0;
    private bool gameOverSoundPlayed = false; // Tracks if the game-over sound has played.
    private List<GameObject> trajectoryPoints = new List<GameObject>();

    private string[] shootingPrompts = new string[] {
        "Aim high, shoot higher!",
        "Channel your inner basketball legend!",
        "Precision is key, young hoopster!",
        "Flex those virtual muscles!",
        "Make Newton proud with some physics magic!"
    };

    private string[] successMessages = new string[] {
        "Boom! Nothing but net!",
        "Stephen Curry who?",
        "Michael Jordan would be impressed!",
        "Swish! You're on fire!",
        "Basketball genius at work!"
    };

    private string[] missMessages = new string[] {
        "Oops! Physics is hard...",
        "Gravity: 1, You: 0",
        "Well, that didn't go as planned!",
        "Air ball! Try again, champ!",
        "The rim is laughing at you..."
    };

    private string[] lastBallMessages = new string[] {
        "Last chance, basketball hero!",
        "Do or die moment!",
        "Everything rides on this shot!",
        "The crowd is holding its breath...",
        "No pressure... just kidding, ALL the pressure!"
    };

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        HandleCannonRotation();
        HandleShooting();
    }

    void InitializeGame()
    {
        mainCamera = Camera.main;
        ballsRemaining = maxBalls;
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        gameOverSoundPlayed = false; // Reset the game-over sound flag.
        UpdateBallUI();
        UpdateScoreUI();
        SetupPowerSlider();
        UpdateStatusText("Ssup Hooper! Let's shoot some hoops!");
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, audioVolume);
        }
    }

    void SetupPowerSlider()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0;
            powerSlider.onValueChanged.AddListener(UpdateTrajectory);

            EventTrigger eventTrigger = powerSlider.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((data) => {
                isOverSlider = true;
                PlaySound(buttonClickSound);
            });
            eventTrigger.triggers.Add(pointerEnterEntry);

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((data) => { isOverSlider = false; });
            eventTrigger.triggers.Add(pointerExitEntry);
        }
    }

    void HandleCannonRotation()
    {
        if (cannonPivot == null || mainCamera == null || isOverSlider) return;

        if (Input.GetMouseButtonDown(0) && !isOverSlider)
        {
            isDragging = true;
            initialTouchPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging && !isOverSlider)
        {
            Vector3 currentTouchPos = Input.mousePosition;
            Vector3 dragDelta = currentTouchPos - initialTouchPos;

            Vector3 rotationDelta = new Vector3(-dragDelta.y, dragDelta.x, 0) * rotationSensitivity;
            cannonPivot.Rotate(rotationDelta, Space.Self);

            Vector3 localEuler = cannonPivot.localEulerAngles;
            localEuler.x = Mathf.Clamp(WrapAngle(localEuler.x), minVerticalRotation, maxVerticalRotation);
            localEuler.y = Mathf.Clamp(WrapAngle(localEuler.y), minHorizontalRotation, maxHorizontalRotation);
            cannonPivot.localRotation = Quaternion.Euler(localEuler);

            initialTouchPos = currentTouchPos;
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonUp(0) && powerSlider != null && powerSlider.value > 0 && ballsRemaining > 0)
        {
            Shoot();
            ResetSlider();
        }
        else if (ballsRemaining <= 0 && !gameOverSoundPlayed)
        {
            gameOverSoundPlayed = true; // Prevent further execution of this block.
            EvaluateScore();
        }
    }

    void Shoot()
    {
        if (basketballPrefab == null || firePoint == null) return;

        ballsRemaining--;
        UpdateBallUI();

        if (ballsRemaining == 0)
        {
            UpdateStatusText(lastBallMessages[Random.Range(0, lastBallMessages.Length)]);
            PlaySound(lastBallSound);
        }
        else
        {
            UpdateStatusText(shootingPrompts[Random.Range(0, shootingPrompts.Length)]);
        }

        PlayShootEffects();

        float shotPower = powerSlider.value * maxShotPower;
        GameObject ball = Instantiate(basketballPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.mass = ballMass;
            rb.AddForce(firePoint.forward * shotPower, ForceMode.Impulse);
        }

        BasketballCollision collisionScript = ball.AddComponent<BasketballCollision>();
        collisionScript.SetCannonController(this);

        ClearTrajectoryPoints();
    }

    void PlayShootEffects()
    {
        PlaySound(shootSound);
        if (shootParticleEffect != null) shootParticleEffect.Play();
    }

    void ResetSlider()
    {
        if (powerSlider != null) powerSlider.value = 0;
    }

    void UpdateTrajectory(float sliderValue)
    {
        ClearTrajectoryPoints();

        float shotPower = sliderValue * maxShotPower;
        Vector3 startPosition = firePoint.position;
        Vector3 startVelocity = firePoint.forward * shotPower;

        float totalDistance = 0f;
        Vector3 lastPosition = startPosition;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 position = CalculateTrajectoryPoint(startPosition, startVelocity, time);

            totalDistance += Vector3.Distance(lastPosition, position);
            if (totalDistance > maxTrajectoryLength) break;

            GameObject point = Instantiate(trajectoryPointPrefab, position, Quaternion.identity);
            trajectoryPoints.Add(point);
            lastPosition = position;
        }
    }

    Vector3 CalculateTrajectoryPoint(Vector3 startPosition, Vector3 startVelocity, float time)
    {
        Vector3 gravityEffect = new Vector3(0, gravity * time * time * 0.5f, 0);
        return startPosition + startVelocity * time + gravityEffect;
    }

    void ClearTrajectoryPoints()
    {
        foreach (GameObject point in trajectoryPoints)
        {
            Destroy(point);
        }
        trajectoryPoints.Clear();
    }

    public void OnBallCollision(GameObject ball, string tag, int points)
    {
        if (ball == null) return;

        AddScore(points);
        UpdateStatusText(successMessages[Random.Range(0, successMessages.Length)]);
        PlaySound(scoreSound);

        if (scoreParticleEffect != null) scoreParticleEffect.Play();
        Destroy(ball);
    }

    void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        }
    }

    void UpdateBallUI()
    {
        if (ballsRemainingText != null) ballsRemainingText.text = "Balls: " + ballsRemaining;
    }

    void UpdateScoreUI()
    {
        if (currentScoreText != null) currentScoreText.text = "Score: " + currentScore;
        if (highScoreText != null) highScoreText.text = "Best: " + highScore;
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    void EvaluateScore()
    {
        float successRate = (float)currentScore / (maxBalls * scoreNetPoints);
        if (successRate >= 0.333f)
        {
            UpdateStatusText("Congrats! Moving to the next level!");
            PlaySound(scoreSound);
            StartCoroutine(LoadNextScene());
        }
        else
        {
            UpdateStatusText("Try again! Replay the level!");
            PlaySound(missSound);
            StartCoroutine(ReloadCurrentScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds to show message
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            UpdateStatusText("You've completed all levels!"); // Final level message
        }
    }

    IEnumerator ReloadCurrentScene()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds to show message
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // To wrap angles for smooth rotation
    float WrapAngle(float angle)
    {
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;
        return angle;
    }
}
