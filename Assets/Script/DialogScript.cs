using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogScript : MonoBehaviour {
	public Canvas displayCanvas;
	public Text labelTitle;
	public Text labelMessage;
	public bool isBtnClicked = false;
	public bool isActive = false;
	public bool returnValue;

	// Use this for initialization
	void Start () {
		displayCanvas.enabled = false;
	}

	public void Show(string title, string message) {
		isBtnClicked = false;
		isActive = true;
		returnValue = false;
		labelTitle.text = title;
		labelMessage.text = message;
		displayCanvas.enabled = true;
	}

	public void BtnOK_Click() {
		displayCanvas.enabled = false;
		isBtnClicked = true;
		returnValue = true;
		isActive = false;
	}

	public void BtnCancel_Click() {
		displayCanvas.enabled = false;
		isBtnClicked = true;
		returnValue = false;
		isActive = false;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
