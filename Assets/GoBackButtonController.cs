using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoBackButtonController : MonoBehaviour {

	public void OnClick()
    {
        GameManager.INSTANCE.VolverAlMenu();
    }
}
