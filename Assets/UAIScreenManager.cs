using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UAIScreenManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Start");
        StartCoroutine(CloseScene());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator CloseScene()
    {
        Debug.Log("CloseScene");
        yield return new WaitForSeconds(5);
        Debug.Log("Closing..");
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
