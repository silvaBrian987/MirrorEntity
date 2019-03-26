using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MirrorEntity
{
    public class GameUtils
    {
        static string[] nombres = { "Jose", "Juan", "Lisa", "Sofia" };
        static System.Random random = new System.Random();

        public static string generarNombre()
        {
            return nombres[random.Next(0, nombres.Length - 1)];
        }

        public static void DownloadImageAndInsertIntoImage(string url, Image image)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

            var request = www.SendWebRequest();
            request.completed += (response) =>
            {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            };
        }
    }
}