using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using Facebook.Unity;

public class GameAnalyticsManager : MonoBehaviour
{

    public static GameAnalyticsManager INSTANCE;


    void Awake()
    {
        if (INSTANCE == null)
            INSTANCE = this;
    }

    // Use this for initialization
    void Start()
    {
        //Para Unity 2018
        GameAnalytics.Initialize();

        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);

        if (FacebookManager.INSTANCE.isReady())
        {
            FacebookManager.INSTANCE.GetUserInfo((result) =>
            {
                if (result.Error != null)
                {
                    Debug.Log("Error Response:\n" + result.Error);
                }
                else if (result.Cancelled)
                {
                    Debug.Log("User cancelled");
                }
                else
                {
                    Debug.Log("result.RawResult  = " + result.RawResult);
                    IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.RawResult) as IDictionary;
                    try
                    {
                        GameAnalytics.SetBirthYear(System.DateTime.Parse(dict["birthday"].ToString()).Year);
                    }
                    catch
                    {
                        GameAnalytics.SetBirthYear(System.DateTime.Now.Year);
                    }
                    GameAnalytics.SetGender(GAGender.Undefined);
                    GameAnalytics.SetFacebookId(dict["id"].ToString());
                }
            }, new string[] { "birthday" });
        }
        else
        {

        }
    }

    public void StartSession()
    {
        GameAnalytics.StartSession();
    }

    public void EndSession()
    {
        GameAnalytics.EndSession();
    }
}
