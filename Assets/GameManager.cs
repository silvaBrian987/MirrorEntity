using MirrorEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Facebook.Unity;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject textBoxPrefab;
    public GameObject textContainer;
    public GameObject buttonPrefab;
    public GameObject buttonContainer;
    public ScrollRect scrollRect;
    public AudioClip backgroundMusic;

    private RectTransform contentPanel;

    public static GameManager INSTANCE;

    public Dictionary<GameObject, Pantalla> RelacionBotonPantalla = new Dictionary<GameObject, Pantalla>();
    public Dictionary<GameObject, Texto> RelacionTextboxTexto = new Dictionary<GameObject, Texto>();

    //string npcName = GameUtils.generarNombre();
    string npcName = "Elric";
    string playerName = "";

    Diccionario diccionario = new Diccionario();
    Historia historiaActual;
    Pantalla pantallaActual;

    List<string> historia_caminoRecorrido = new List<string>();

    bool JuegoTermino = false;
    NPCWait npcWait = new NPCWait();

    void Awake()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
            //DontDestroyOnLoad(INSTANCE);
        }
    }

    // Use this for initialization
    void Start()
    {
        contentPanel = textContainer.GetComponent<RectTransform>();

        IniciarJuego();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            VolverAlMenu();
        }
    }

    public void IniciarJuego()
    {
        this.playerName = PlayerPrefs.GetString("PlayerName");

        historiaActual = CargarHistoria(PlayerPrefs.GetString("StoryName"));

        if (!CargarPartida(historiaActual))
        {
            CargarPantalla(historiaActual.PantallaInicial);
        }

        AudioManager.INSTANCE.StartMusic(backgroundMusic, true);

        GameAnalyticsManager.INSTANCE.StartSession();
    }

    public void CargarPantalla(Pantalla pantalla, bool cargarAnterior = false)
    {
        var texto = new LinkedList<Texto>(pantalla.Textos).First;

        if (cargarAnterior)
        {
            CargarPantallasAnteriores(pantalla.Padre);
            texto.Value.TiempoRetardo = 1;
        }

        pantallaActual = pantalla;

        LimpiarBotones();

        EsperarTexto(texto, () =>
        {
            pantalla.Botones.ForEach((boton) =>
            {
                RelacionBotonPantalla.Add(CrearBoton(boton.Texto, boton.Texto), boton.Pantalla);
            });
        });
    }

    private void CargarPantallasAnteriores(Pantalla pantalla)
    {
        if (pantalla == null)
            return;

        if (pantalla.Padre != null)
            CargarPantallasAnteriores(pantalla.Padre);

        pantalla.Textos.ForEach((texto) =>
        {
            CrearTexto(texto);
        });
    }

    public void TerminarJuego()
    {
        JuegoTermino = true;

        LimpiarBotones();
        CreateButtonText("GAME OVER");
        BorrarPartida();

        StartCoroutine(Esperar(10, () =>
        {
            VolverAlMenu();
        }));
    }

    private void LimpiarBotones()
    {
        foreach (var button in RelacionBotonPantalla.Keys)
        {
            Destroy(button);
        }
        RelacionBotonPantalla.Clear();
    }

    private void LimpiarTextos()
    {
        for (int i = 0; i < textContainer.transform.childCount; i++)
        {
            var child = textContainer.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        RelacionTextboxTexto.Clear();
    }

    public void CrearTexto(Texto texto)
    {
        Debug.Log("texto.Valor = " + texto.Valor);
        if (texto.GetType().Equals(typeof(TextoJugador)))
        {
            CreatePlayerText(texto);
        }
        else
        {
            CreateNPCText(texto);
        }
    }

    public void CreateNPCText(Texto texto)
    {
        CreateCharacterText(npcName, texto, TextAlignmentOptions.Left);
    }

    public void CreatePlayerText(Texto texto)
    {
        CreateCharacterText(playerName, texto, TextAlignmentOptions.Right);
    }

    public void CreateButtonText(string text)
    {
        CreateTextObject(string.Format("---{0}---", text), TextAlignmentOptions.Center);
    }

    private void CreateCharacterText(string name, Texto texto, TextAlignmentOptions alignment)
    {
        var textBox = CreateTextObject(string.Format("<i><b>{0}</b></i>{1}{2}", name, System.Environment.NewLine, texto.Valor), alignment);
        RelacionTextboxTexto.Add(textBox, texto);
    }

    private GameObject CreateTextObject(string text, TextAlignmentOptions alignment)
    {
        var textBox = Instantiate(textBoxPrefab, textContainer.transform);

        textBox.name = "Textbox_" + System.DateTime.Now.Ticks.ToString();

        var textMesh = textBox.GetComponent<TextMeshProUGUI>();
        textMesh.alignment = alignment;
        textMesh.text = text;

        //Fuerzo la actualizacion para que el textbox tome los cambios
        Canvas.ForceUpdateCanvases();

        var textContainer_rectTransform = textContainer.GetComponent<RectTransform>();
        textContainer_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textContainer_rectTransform.rect.height + textMesh.rectTransform.rect.height);

        SnapTo(textBox.GetComponent<RectTransform>());

        return textBox;
    }

    private GameObject CrearBoton(string texto, string tag)
    {
        GameObject button = Instantiate(buttonPrefab, buttonContainer.transform);
        button.name = "Button_" + tag;

        button.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameButton_OnClickEvent(button);
        });

        var textComponent = button.GetComponentInChildren<Text>();
        textComponent.text = texto;
        return button;
    }

    private void GameButton_OnClickEvent(GameObject button)
    {
        string pantallaName = button.GetComponentInChildren<Text>().text;
        CreateButtonText(pantallaName);

        //var pantalla = RelacionBotonPantalla[button];
        Debug.Log("Se va a buscar la pantalla " + pantallaName + " en el archivo...");
        var pantalla = diccionario.CrearPantalla(pantallaName, diccionario.ObtenerNodoPorNombre(pantallaName).InnerText);

        CargarPantalla(pantalla);
    }

    public IEnumerator Esperar(int retardo, System.Action action)
    {
        print("retardo = " + retardo);
        npcWait.Waiting = true;
        npcWait.TimeInSeconds = retardo;
        yield return new WaitForSeconds(retardo);
        npcWait.Waiting = false;
        print("retardo = " + retardo);
        action.Invoke();
    }

    public void EsperarTexto(LinkedListNode<Texto> textoNode, System.Action lastAction)
    {
        if (textoNode == null)
        {
            if (pantallaActual.GetType().Equals(typeof(MirrorEntity.PantallaFinal)))
            {
                TerminarJuego();
            }
            else
            {
                lastAction.Invoke();
            }

            return;
        }

        Texto texto = textoNode.Value;

        StartCoroutine(Esperar(texto.TiempoRetardo, () =>
        {
            CrearTexto(texto);

            if (textoNode.Next != null)
            {
                EsperarTexto(textoNode.Next, lastAction);
            }
            else
            {
                if (pantallaActual.GetType().Equals(typeof(MirrorEntity.PantallaFinal)))
                {
                    TerminarJuego();
                }
                else
                {
                    lastAction.Invoke();
                }
            }
        }));
    }

    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        contentPanel.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position) - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }

    public void Reiniciar()
    {
        StopAllCoroutines();
        LimpiarTextos();
        LimpiarBotones();
        contentPanel.anchoredPosition = new Vector2(0, 0);
        BorrarPartida();
        CargarPantalla(historiaActual.PantallaInicial);
    }

    private void GuardarPartida()
    {
        if (npcWait.Waiting)
        {
            Debug.Log("Enviando notificacion push");
            Assets.SimpleAndroidNotifications.NotificationManager.SendWithAppIcon(System.TimeSpan.FromSeconds(npcWait.TimeInSeconds), "Mirror Entity", npcName + " te esta hablando...", Color.red);
        }

        Debug.Log("guardando estado actual");

        if (pantallaActual == null)
            //throw new System.Exception("No hay pantalla actual");
            return;

        Savepoint savepoint = new Savepoint();
        savepoint.NombreHistoria = historiaActual.Nombre;
        savepoint.NombrePantalla = pantallaActual.Nombre;

        Debug.Log("savedGame_" + savepoint.NombreHistoria + " guardado!");
        PlayerPrefs.SetString("savedGame_" + savepoint.NombreHistoria, JsonUtility.ToJson(savepoint));
    }

    public bool CargarPartida(Historia historia)
    {
        Pantalla pantalla = null;

        try
        {
            Debug.Log("Cargando savedGame_" + historia.Nombre + " ...");
            Savepoint savepoint = JsonUtility.FromJson<Savepoint>(PlayerPrefs.GetString("savedGame_" + historia.Nombre));
            Debug.Log("savepoint = " + savepoint);
            Debug.Log(savepoint.NombreHistoria + " - " + savepoint.NombrePantalla);
            //cargar historia
            //historiaActual.Pantallas.TryGetValue(savepoint.NombrePantalla, out pantalla);
            pantalla = diccionario.CrearPantalla(savepoint.NombrePantalla, diccionario.ObtenerNodoPorNombre(savepoint.NombrePantalla).InnerText);
        }
        catch (System.Exception ex)
        {
            Debug.Log("No existe archivo de guardado. Causa: " + System.Environment.NewLine + ex.StackTrace.ToString());
        }

        if (pantalla != null)
        {
            CargarPantalla(pantalla, true);
            return true;
        }
        else
        {
            return false;
        }
    }

    public Historia CargarHistoria(string nombre)
    {
        Debug.Log("Se va a cargar la historia " + nombre);
        var resource = Resources.Load<TextAsset>("Historias/" + nombre);
        Debug.Log("Archivo cargado!");
        Debug.Log("Creando historia...");
        Historia historia = diccionario.CrearHistoria(resource.bytes, resource.name);
        Debug.Log("Historia creada!");
        return historia;
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        GameAnalyticsManager.INSTANCE.EndSession();
        if(!JuegoTermino)
            GuardarPartida();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            StopAllCoroutines();
            GameAnalyticsManager.INSTANCE.EndSession();
            if (!JuegoTermino)
                GuardarPartida();
        }
    }

    public void VolverAlMenu()
    {
        StopAllCoroutines();
        GameAnalyticsManager.INSTANCE.EndSession();
        if (!JuegoTermino)
            GuardarPartida();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    private void BorrarPartida()
    {
        Debug.Log("savedGame_" + historiaActual.Nombre);
        //PlayerPrefs.SetString("savedGame_" + historiaActual.Nombre, null);
        PlayerPrefs.DeleteKey("savedGame_" + historiaActual.Nombre);
        //PlayerPrefs.SetString("savedGame_" + historiaActual.Nombre, "");
    }

    private int ObtenerAlturaTextbox(Text textbox, string texto)
    {

        int anchoAux = 0;
        int lineas = 1;

        var chars = texto.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            CharacterInfo charInfo = new CharacterInfo();
            textbox.font.GetCharacterInfo(c, out charInfo);

            anchoAux += charInfo.advance;
            if (anchoAux >= textbox.rectTransform.rect.width)
                lineas++;

        }
        return textbox.fontSize * lineas;
    }
}

[System.Serializable]
public class Savepoint
{
    public string NombreHistoria;
    public string NombrePantalla;
}

class NPCWait
{
    public bool Waiting;
    public int TimeInSeconds;
}