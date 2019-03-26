
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MirrorEntity;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager INSTANCE;

    public GameObject buttonPrefab;
    public GameObject buttonContainer;
    public Image avatarImage;
    public AudioClip backgroundMusic;

    bool Iniciado = false;
    GameObject BotonFacebook_Login;
    GameObject BotonFacebook_Logout;
    Sprite originalAvatarImage;

    private void Awake()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
        }
    }

    private void Start()
    {
        Debug.Log("FacebookManager.INSTANCE.IsInitialized = " + FacebookManager.INSTANCE.IsInitialized());
        Debug.Log("FacebookManager.INSTANCE.IsLoggedIn = " + FacebookManager.INSTANCE.IsLoggedIn());

        if (!FacebookManager.INSTANCE.IsInitialized())
        {
            FacebookManager.INSTANCE.InitFacebook(() =>
            {
                Debug.Log("Se inicializa Facebook desde HistoryManager");
                FacebookManager.INSTANCE.ActivateApp();

                MostrarBotonesFacebook();
            });
        }

        AudioManager.INSTANCE.StartMusic(backgroundMusic, true, 0.5f);
        originalAvatarImage = avatarImage.sprite;
        IniciarMenu();
    }

    public void CreateButtons(TextAsset[] History)
    {
        foreach (TextAsset story in History)
        {
            GameObject boton = (CrearBoton(story.name, () => OnButtonClick(story.name)));
        }
    }

    //Saves history name to player prefs and loads Scene1
    public void OnButtonClick(string storyName)
    {
        Debug.Log("Cargando escena...");
        PlayerPrefs.SetString("StoryName", storyName);
        SceneManager.LoadScene("Scene1", LoadSceneMode.Single);
        Debug.Log("Escena cargada!");
    }

    public GameObject CrearBoton(string texto, UnityEngine.Events.UnityAction onClickAction)
    {
        var boton = Instantiate(buttonPrefab, buttonContainer.transform);

        if (onClickAction != null)
            boton.GetComponent<Button>().onClick.AddListener(onClickAction);

        var textComponent = boton.GetComponentInChildren<Text>();
        textComponent.text = texto;
        return boton;
    }

    public static TextAsset[] LoadFile()
    {
        return Resources.LoadAll<TextAsset>("Historias/");
    }

    public void IniciarMenu()
    {
        if (!Iniciado)
        {
            var playerName = PlayerPrefs.GetString("PlayerName");
            if (playerName == null || playerName.Length == 0)
            {
                PlayerPrefs.SetString("PlayerName", GameUtils.generarNombre());
            }

            var playerAvatarUrl = PlayerPrefs.GetString("PlayerAvatarUrl");
            if (playerAvatarUrl != null && playerAvatarUrl.Length > 0)
            {
                GameUtils.DownloadImageAndInsertIntoImage(playerAvatarUrl, avatarImage);
            }

            CreateButtons(LoadFile());

            Debug.Log("FacebookManager.INSTANCE.IsInitialized = " + FacebookManager.INSTANCE.IsInitialized());
            Debug.Log("FacebookManager.INSTANCE.IsLoggedIn = " + FacebookManager.INSTANCE.IsLoggedIn());

            BotonFacebook_Login = CrearBoton("Iniciar con Facebook", () =>
            {
                Time.timeScale = 0;
                FacebookManager.INSTANCE.StartFacebook((loginResult) =>
                {
                    if (FacebookManager.INSTANCE.isReady())
                    {
                        FacebookManager.INSTANCE.GetUserInfo((result) =>
                        {
                            if (result.Error != null)
                            {
                                Debug.LogError("Error Response:\n" + result.Error);
                            }
                            else if (result.Cancelled)
                            {
                                Debug.LogError("Cancelled");
                            }
                            else
                            {
                                IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.RawResult) as IDictionary;
                                PlayerPrefs.SetString("PlayerName", dict["first_name"].ToString());
                                string userId = dict["id"] as string;
                                try
                                {

                                    FacebookManager.INSTANCE.GetUserAvatar((avatarResult) =>
                                {
                                    Debug.Log(avatarResult);
                                    if (avatarResult.Error != null)
                                    {
                                        Debug.LogError("Error Response:\n" + avatarResult.Error);
                                    }
                                    else if (avatarResult.Cancelled)
                                    {
                                        Debug.LogError("Cancelled");
                                    }
                                    else
                                    {
                                        Debug.Log(avatarResult.RawResult);
                                        //IDictionary avatarResultDict = Facebook.MiniJSON.Json.Deserialize(avatarResult.RawResult) as IDictionary;
                                        IDictionary<string, object> avatarResultDict = avatarResult.ResultDictionary;
                                        Debug.Log(avatarResultDict);
                                        IDictionary dataDict = avatarResultDict["data"] as IDictionary;
                                        string avatarUrl = dataDict["url"] as string;
                                        Debug.Log(avatarUrl);
                                        PlayerPrefs.SetString("PlayerAvatarUrl", avatarUrl);
                                        GameUtils.DownloadImageAndInsertIntoImage(avatarUrl, avatarImage);
                                    }
                                }, userId);
                                }

                                catch (System.Exception e)
                                {
                                    Debug.LogError("No encontre la foto por el siguiente error: " + e.ToString());
                                }

                                BotonFacebook_Login.SetActive(false);
                                BotonFacebook_Logout.SetActive(true);
                            }
                            Time.timeScale = 1;
                        }, new string[] { "first_name" });
                    }
                    else
                    {
                        Time.timeScale = 1;
                    }
                });
            });
            BotonFacebook_Login.SetActive(false);

            BotonFacebook_Logout = CrearBoton("Cerrar sesion de Facebook", () =>
            {
                FacebookManager.INSTANCE.Logout();
                LimpiarEscena_Facebook();
                BotonFacebook_Login.SetActive(true);
                BotonFacebook_Logout.SetActive(false);
            });
            BotonFacebook_Logout.SetActive(false);

            MostrarBotonesFacebook();

            Iniciado = true;
        }
    }

    private void LimpiarEscena_Facebook()
    {
        PlayerPrefs.SetString("PlayerName", GameUtils.generarNombre());
        PlayerPrefs.SetString("PlayerAvatarUrl", null);
        avatarImage.sprite = originalAvatarImage;
    }

    private void MostrarBotonesFacebook()
    {
        if (FacebookManager.INSTANCE.IsInitialized())
        {
            if (FacebookManager.INSTANCE.IsLoggedIn())
            {
                BotonFacebook_Logout.SetActive(true);
                BotonFacebook_Login.SetActive(false);
            }
            else
            {
                BotonFacebook_Logout.SetActive(false);
                BotonFacebook_Login.SetActive(true);
                LimpiarEscena_Facebook();
            }
        }
    }
}
