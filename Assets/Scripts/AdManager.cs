using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidGameId = "5744153";
    [SerializeField] private string iosGameId = "5744152";

    [SerializeField] private string adUnitIdAndroid = "Interstitial_Android"; // Corrected ID
    [SerializeField] private string adUnitIdIOS = "Interstitial_iOS";         // Corrected ID

    [SerializeField] private bool testMode = true;

    private string gameId;
    private string adUnitId;

    void Awake()
    {
        // Determine game ID based on platform
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? iosGameId
            : androidGameId;

        // Set adUnitId based on platform
        adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? adUnitIdIOS
            : adUnitIdAndroid;

        // Initialize Unity Ads
        InitializeAds();
    }

    void InitializeAds()
    {
        Debug.Log($"Initializing Ads with Game ID: {gameId}, Test Mode: {testMode}");
        Advertisement.Initialize(gameId, testMode, this);
    }

    // IUnityAdsInitializationListener methods
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads Initialization Complete");
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Ads Initialization Failed: {error} - {message}");
    }

    // Load an ad
    void LoadAd()
    {
        Debug.Log($"Loading Ad: {adUnitId}");
        Advertisement.Load(adUnitId, this);
    }

    // IUnityAdsLoadListener methods
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Ad Loaded: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Ad Load Failed: {placementId}, {error} - {message}");
    }

    // Show the ad
    public void ShowAd()
    {
        Debug.Log($"Showing Ad: {adUnitId}");
        Advertisement.Show(adUnitId, this);
    }

    // IUnityAdsShowListener methods
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Show Complete: {placementId}, {showCompletionState}");

        // Reload ad after showing
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ad Show Failed: {placementId}, {error} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"Ad Show Started: {placementId}");
    }

    // New method to implement the missing interface method
    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"Ad Clicked: {placementId}");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optional: Show ad when scene loads
        ShowAd();
    }
}
