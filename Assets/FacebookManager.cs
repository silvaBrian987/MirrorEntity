using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{
    public static FacebookManager INSTANCE;

    void Awake()
    {
        //InitFacebook();

        if (INSTANCE == null)
        {
            INSTANCE = this;
            DontDestroyOnLoad(this);
        }
    }

    public static void DefaultInitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...            
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    private void AuthCallback(ILoginResult result)
    {

        if (result.Error != null)
        {
            Debug.Log("Error Response:\n" + result.Error);
        }
        else if (result.Cancelled)
        {
            Debug.Log("User cancelled login");
        }
        else
        {
            // AccessToken class will have session details
            var aToken = AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
        }
    }

    public void StartFacebook(FacebookDelegate<ILoginResult> callback = null)
    {
        var perms = new List<string>() { "public_profile", "email", "user_birthday" };
        if (callback != null)
        {
            FB.LogInWithReadPermissions(perms, callback);
        }
        else
        {
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }
    }

    public void GetUserInfo(System.Action<IGraphResult> action, string[] fields = null)
    {
        var url = "/me";

        if (fields != null)
            url = string.Concat(url, "?fields=", string.Join(",", fields));

        Debug.Log("url = " + url);

        FB.API(url, HttpMethod.GET, (result) =>
         {
             action.Invoke(result);
         });
    }

    public bool IsInitialized()
    {
        return FB.IsInitialized;
    }

    public bool IsLoggedIn()
    {
        return FB.IsLoggedIn;
    }

    public bool isReady()
    {
        return IsInitialized() && IsLoggedIn();
    }

    public void GetUserAvatar(System.Action<IGraphResult> action, string userId)
    {
        var url = string.Format("/{0}/picture?redirect=false", userId);

        Debug.Log("url = " + url);

        FB.API(url, HttpMethod.GET, (result) =>
        {
            action.Invoke(result);
        });
    }

    public void Logout()
    {
        FB.LogOut();
    }

    public void InitFacebook(InitDelegate initDelegate = null)
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            if(initDelegate != null)
            {
                FB.Init(initDelegate, OnHideUnity);
            }
            else
            {
                FB.Init(DefaultInitCallback, OnHideUnity);
            }
        }
        else
        {
            // Already initialized, signal an app activation App Event
            ActivateApp();
        }
    }

    public void ActivateApp()
    {
        FB.ActivateApp();
    }
}
