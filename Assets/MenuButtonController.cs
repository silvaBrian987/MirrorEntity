using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour {

    public GameObject MenuReference;
	
    public void OnClick()
    {
        MenuReference.SetActive(!MenuReference.activeSelf);
    }
}
