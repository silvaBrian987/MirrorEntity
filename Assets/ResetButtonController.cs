using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class ResetButtonController : MonoBehaviour
{

    public void onClick()
    {
        AdManager.INSTANCE.ShowAd((result) =>
        {
            if (result == ShowResult.Finished)
            {
                GameManager.INSTANCE.Reiniciar();

                if (transform.parent.gameObject.CompareTag("MenuPanel"))
                {
                    transform.parent.gameObject.SetActive(false);
                }
            }
        });
    }
}
