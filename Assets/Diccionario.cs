using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Text.RegularExpressions;

namespace MirrorEntity
{
    public class Diccionario
    {
        private static string ACTION_REGEX = @"(?<=\[\[)(.*?)(?=\]\])";
        private static string PLAYER_TEXT_REGEX = @"(?<=\-)(.*?)(?=\-)";
        private static string NPC_TEXT_REGEX = @"(?<=\*)(.*?)(?=\*)";
        private static string TIME_REGEX = @"(?<=\()(\d+)(?=.*?\))";

        private XmlElement DocumentElement;
        private Dictionary<string, Pantalla> pantallasPorNombre;

        public Historia CrearHistoria(byte[] bytes, string title = null)
        {
            //var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Regex.Replace(new System.IO.StreamReader(new System.IO.MemoryStream(bytes)).ToString(), "", "")));
            Debug.Log("Creando MemoryStream de " + bytes.Length);
            var stream = new System.IO.MemoryStream(bytes);

            var doc = new XmlDocument();
            Debug.Log("Cargando archivo...");
            doc.Load(stream);
            Debug.Log("Archivo cargado!");
        
            return CrearHistoria(doc);
        }

        private Historia CrearHistoria(XmlDocument doc, string title = null)
        {
            DocumentElement = doc.DocumentElement;

            if (title == null)
            {
                var titleNode = DocumentElement.SelectSingleNode("//title");
                title = titleNode != null ? titleNode.InnerText : doc.Name;
            }

            pantallasPorNombre = new Dictionary<string, Pantalla>();
            Debug.Log("Creando historia...");
            Historia historia = new Historia(title, CrearPrimeraPantalla());
            historia.Pantallas = pantallasPorNombre;
            Debug.Log("Historia creada!");

            return historia;
        }

        public Pantalla CrearPrimeraPantalla()
        {
            var primerElemento = DocumentElement.SelectSingleNode("//tw-passagedata[@pid='1']");
            return CrearPantalla(primerElemento.Attributes["name"].Value, primerElemento.InnerText);
        }

        public Pantalla CrearPantalla(string nombreNodo, string textoNodo)
        {
            //Debug.Log(nombreNodo);

            Pantalla pantalla = new Pantalla(nombreNodo);

            Match actionMatch = Regex.Match(textoNodo, ACTION_REGEX);
            while (actionMatch.Success)
            {
                string action = actionMatch.Value;
                var actionRelatedNode = ObtenerNodoPorNombre(action);
                //var nuevaPantalla = CrearPantalla(action, actionRelatedNode.InnerText);
                //nuevaPantalla.Padre = pantalla;
                //pantalla.Botones.Add(new BotonAccion(action, nuevaPantalla));
                pantalla.Botones.Add(new BotonAccion(action, null));
                actionMatch = actionMatch.NextMatch();
            }

            if (pantalla.Botones.Count == 0)
                pantalla = new PantallaFinal(nombreNodo);

            CrearTextos(pantalla, textoNodo);

            //Debug.Log(pantalla.ToString());
            if (!pantallasPorNombre.ContainsKey(pantalla.Nombre))
                pantallasPorNombre.Add(pantalla.Nombre, pantalla);

            return pantalla;
        }

        private void CrearTextos(Pantalla pantalla, string textoNodo)
        {
            textoNodo = textoNodo.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
            Match playerTextMatch = Regex.Match(textoNodo, PLAYER_TEXT_REGEX);
            while (playerTextMatch.Success)
            {
                int retardo = 0;
                Match timeMatch = Regex.Match(playerTextMatch.Value, TIME_REGEX);
                if (timeMatch.Success)
                {
                    retardo = int.Parse(timeMatch.Value);
                }
                pantalla.Textos.Add(new TextoJugador(playerTextMatch.Value.Replace("(" + timeMatch.Value + ")", ""), retardo));
                playerTextMatch = playerTextMatch.NextMatch();
            }

            Match npcTextMatch = Regex.Match(textoNodo, NPC_TEXT_REGEX);
            while (npcTextMatch.Success)
            {
                int retardo = 0;
                Match timeMatch = Regex.Match(npcTextMatch.Value, TIME_REGEX);
                if (timeMatch.Success)
                {
                    retardo = int.Parse(timeMatch.Value);
                }
                pantalla.Textos.Add(new TextoPersonaje(npcTextMatch.Value.Replace("(" + timeMatch.Value + ")", ""), retardo));
                npcTextMatch = npcTextMatch.NextMatch();
            }
        }

        public XmlNode ObtenerNodoPorNombre(string nombre)
        {
            return DocumentElement.SelectSingleNode("//tw-passagedata[@name='" + nombre + "']");
        }
    }

    public class Pantalla
    {
        public string Nombre;
        public List<Texto> Textos;
        public List<BotonAccion> Botones;
        public Pantalla Padre;

        public Pantalla(string nombre)
        {
            this.Nombre = nombre;
            this.Textos = new List<Texto>();
            this.Botones = new List<BotonAccion>();
        }

        public override string ToString()
        {
            return Nombre;
        }
    }

    public class PantallaFinal : Pantalla
    {
        public PantallaFinal(string nombre) : base(nombre)
        {
        }
    }

    public abstract class Texto
    {
        public string Valor;
        public int TiempoRetardo;

        public Texto(string valor, int tiempoRetardo)
        {
            this.Valor = valor;
            this.TiempoRetardo = tiempoRetardo;
        }
    }

    public class TextoJugador : Texto
    {
        public TextoJugador(string valor, int tiempoRetardo) : base(valor, tiempoRetardo)
        {
        }
    }

    public class TextoPersonaje : Texto
    {
        public TextoPersonaje(string valor, int tiempoRetardo) : base(valor, tiempoRetardo)
        {
        }
    }

    public class BotonAccion
    {
        public string Texto;
        public Pantalla Pantalla;
        public BotonAccion(string texto, Pantalla pantalla)
        {
            this.Texto = texto;
            this.Pantalla = pantalla;
        }
    }

    public class Historia
    {
        public string Nombre;
        public Pantalla PantallaInicial;
        public Dictionary<string, Pantalla> Pantallas;

        public Historia(string nombre, Pantalla pantallaInicial)
        {
            this.Nombre = nombre;
            this.PantallaInicial = pantallaInicial;
        }
    }
}