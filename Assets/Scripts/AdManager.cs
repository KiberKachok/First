using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    private InterstitialAd interstitial;
    public static AdManager main = null;
    public string adUnitId;
    public int adShowTimes = 0;

    public void Awake()
    {
        if (main != this)
        {
            if (main != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            main = this;
        }
    }

    public void Start()
    {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-1498434318190636/4913993097";
#elif UNITY_IPHONE
            adUnitId = "unexpected_platform";
#else
            adUnitId = "unexpected_platform";
#endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
        this.interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        this.interstitial.LoadAd(request);
    }

    public void DrawBanner()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
        }
    }

    public void OnApplicationQuit()
    {
        // Initialize an InterstitialAd.
        interstitial.Destroy();
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1) {
            if (main != this)
            {
                if (main != null)
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
                main = this;
            }
            else
            {
                Debug.Log("Menu Loaded");


                adShowTimes += 1;
                DrawBanner();

                this.interstitial = new InterstitialAd(adUnitId);
                AdRequest request = new AdRequest.Builder().Build();
                this.interstitial.LoadAd(request);
            }
        }

    }
}