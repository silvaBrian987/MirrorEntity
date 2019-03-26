using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour
{
    public static AdManager INSTANCE;

    ShowOptions options;

    private void Awake()
    {
        options = new ShowOptions();

        if (INSTANCE == null)
            INSTANCE = this;
    }

    public void ShowAd(System.Action<ShowResult> callback = null)
    {
        if (Advertisement.IsReady("rewardedVideo") && !Advertisement.isShowing)
        {
            options.resultCallback = callback;
            Advertisement.Show("rewardedVideo", options);
        }
    }
}
