using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeText : MonoBehaviour {
    	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseEnter()
    {
        Debug.Log("Entered");
    }

    void OnMouseDrag()
    {
        Debug.Log("Draging");
    }
}
