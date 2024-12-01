using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CannonController : MonoBehaviour
{
    [Header("Cannon Components")]
    public Transform cannonPivot; // Rotating part of the cannon
    public Transform firePoint; // The point where the ball is shot
    public GameObject basketballPrefab; // Basketball prefab
    public GameObject trajectoryPointPrefab; // Prefab for trajectory points
    public ParticleSystem shootParticleEffect; // Particle effect when shooting
    public ParticleSystem scoreParticleEffect; // Particle effect when score is made
    public AudioSource shootAudio; // Audio source for shooting sound
    public AudioSource scoreAudio; // Audio source for score sound

    [Header("Shooting Controls")]
    public Slider powerSlider; // UI slider for shot power
    public float maxShotPower = 20f; // Maximum shot power
    public int trajectoryResolution = 50; // Number of trajectory points
    public float trajectoryTimeStep = 0.1f; // Time step for trajectory prediction
    public float maxTrajectoryLength = 50f; // Maximum length of the trajectory line (in units)

    [Header("Physics")]
    public float ballMass = 0.6f; // Basketball mass
    public float gravity = -9.81f; // Gravity in simulation

    [Header("Cannon Rotation")]
    public float rotationSensitivity = 0.2f; // Sensitivity of cannon rotation
    public float minVerticalRotation = -10f; // Minimum vertical rotation angle
    public float maxVerticalRotation = 45f; // Maximum vertical rotation angle
    public float minHorizontalRotation = -45f; // Minimum horizontal rotation angle
    public float maxHorizontalRotation = 45f; // Maximum horizontal rotation angle

    [Header("UI Elements")]
    public Text ballsRemainingText; // Text to display balls remaining
    public Text currentScoreText; // Text to display current score
    public Text highScoreText; // Text to display high score
    public int maxBalls = 5; // Maximum number of balls available for shooting
    private int ballsRemaining; // Track the remaining balls

    private Vector3 initialTouchPos;
    private bool isDragging = false;
    private bool isOverSlider = false;
    private int currentScore = 0;
    private int highScore = 0;

    private List<GameObject> trajectoryPoints = new List<GameObject>();

    private Camera mainCamera;

    void Start()
    {
        // Initialize the main camera
        mainCamera = Camera.main;

        // Initialize slider
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0;
            powerSlider.onValueChanged.AddListener(UpdateTrajectory);
            AddSliderEvents();

            // Add EventTrigger to detect if the mouse is over the slider
            EventTrigger eventTrigger = powerSlider.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = powerSlider.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((data) => { isOverSlider = true; });
            eventTrigger.triggers.Add(pointerEnterEntry);

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((data) => { isOverSlider = false; });
            eventTrigger.triggers.Add(pointerExitEntry);
        }

        // Initialize UI
        ballsRemaining = maxBalls;
        UpdateBallUI();

        // Ensure components are assigned
        if (cannonPivot == null) Debug.LogError("Cannon Pivot is not assigned!");
        if (firePoint == null) Debug.LogError("Fire Point is not assigned!");
    }

    void Update()
    {
        // Prevent rotation if over slider
        if (!isOverSlider)
        {
            HandleCannonRotation();
        }

        // Shoot when the slider is released and we have balls remaining
        if (Input.GetMouseButtonUp(0) && powerSlider != null && powerSlider.value > 0 && ballsRemaining > 0)
        {
            Shoot();
            ResetSlider();
        }
    }

    private void HandleCannonRotation()
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

            // Clamp rotation
            Vector3 localEuler = cannonPivot.localEulerAngles;
            localEuler.x = Mathf.Clamp(WrapAngle(localEuler.x), minVerticalRotation, maxVerticalRotation);
            localEuler.y = Mathf.Clamp(WrapAngle(localEuler.y), minHorizontalRotation, maxHorizontalRotation);
            cannonPivot.localRotation = Quaternion.Euler(localEuler);

            initialTouchPos = currentTouchPos;
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    private void Shoot()
    {
        if (basketballPrefab == null || firePoint == null) return;

        // Decrease the number of balls remaining
        ballsRemaining--;
        UpdateBallUI();

        // Play shooting sound and particle effect
        if (shootAudio != null) shootAudio.Play();
        if (shootParticleEffect != null) shootParticleEffect.Play();

        float shotPower = powerSlider.value * maxShotPower;
        GameObject ball = Instantiate(basketballPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.mass = ballMass;
            rb.AddForce(firePoint.forward * shotPower, ForceMode.Impulse); // Direct force based on power slider
        }

        ClearTrajectoryPoints();
    }

    private void ResetSlider()
    {
        if (powerSlider != null) powerSlider.value = 0;
    }

    private void UpdateTrajectory(float sliderValue)
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

            // Calculate distance from last point and check if we exceeded max length
            totalDistance += Vector3.Distance(lastPosition, position);
            if (totalDistance > maxTrajectoryLength) break;

            GameObject point = Instantiate(trajectoryPointPrefab, position, Quaternion.identity);
            trajectoryPoints.Add(point);
            lastPosition = position;
        }
    }

    private Vector3 CalculateTrajectoryPoint(Vector3 startPosition, Vector3 startVelocity, float time)
    {
        // Gravity effect is applied downward over time
        Vector3 gravityEffect = new Vector3(0, gravity * time * time * 0.5f, 0);
        return startPosition + startVelocity * time + gravityEffect;
    }

    private void ClearTrajectoryPoints()
    {
        foreach (GameObject point in trajectoryPoints)
        {
            Destroy(point);
        }
        trajectoryPoints.Clear();
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null && powerSlider != null)
        {
            float shotPower = powerSlider.value * maxShotPower;
            Vector3 startPosition = firePoint.position;
            Vector3 startVelocity = firePoint.forward * shotPower;

            Gizmos.color = Color.green;
            float totalDistance = 0f;
            Vector3 lastPosition = startPosition;

            for (int i = 0; i < trajectoryResolution - 1; i++)
            {
                float time1 = i * trajectoryTimeStep;
                float time2 = (i + 1) * trajectoryTimeStep;

                Vector3 point1 = CalculateTrajectoryPoint(startPosition, startVelocity, time1);
                Vector3 point2 = CalculateTrajectoryPoint(startPosition, startVelocity, time2);

                // Calculate distance between points and check if max length is reached
                totalDistance += Vector3.Distance(lastPosition, point1);
                if (totalDistance > maxTrajectoryLength) break;

                Gizmos.DrawLine(point1, point2);
                lastPosition = point2;
            }
        }
    }

    // Update UI for balls remaining
    private void UpdateBallUI()
    {
        if (ballsRemainingText != null)
        {
            ballsRemainingText.text = "Balls Remaining: " + ballsRemaining;
        }

        if (ballsRemaining == 0)
        {
            // Disable shooting when no balls are left
            powerSlider.interactable = false;
        }
        else
        {
            powerSlider.interactable = true;
        }
    }

    // Call this function when a basket is scored
    public void OnScore(GameObject ball, string tag)
    {
        // Check if the ball hits the ScoreNet and whether it has passed through the ring
        if (tag == "ScoreNet")
        {
            currentScore += 10; // Or any other score increment logic
            UpdateScoreUI();

            if (scoreAudio != null) scoreAudio.Play();
            if (scoreParticleEffect != null) scoreParticleEffect.Play();

            // Destroy the ball after scoring
            Destroy(ball);
        }
    }

    private void UpdateScoreUI()
    {
        if (currentScoreText != null) currentScoreText.text = "Score: " + currentScore;
        if (highScore < currentScore)
        {
            highScore = currentScore;
            if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
        }
    }

    private float WrapAngle(float angle)
    {
        // Normalize the angle to be between -180 and 180
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        return angle;
    }

    private void AddSliderEvents()
    {
        if (powerSlider != null)
        {
            powerSlider.onValueChanged.AddListener(UpdateTrajectory);
        }
    }
}
