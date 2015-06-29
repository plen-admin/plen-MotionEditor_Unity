using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void DialogFinishedEventHandler(bool isBtnOKClicked);

public class DialogScript : MonoBehaviour {
	public ObjectsController objectsController;
	public Canvas displayCanvas;
	public Text labelTitle;
	public Text labelMessage;
	public bool isBtnClicked = false;
	public bool isActive = false;
	public bool returnValue;
	public event DialogFinishedEventHandler DialogFinished;

	// Use this for initialization
	void Start () {
		displayCanvas.enabled = false;
	}

	public void Show(string title, string message) {
		objectsController.isDialogShowing = true;
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
		DialogFinished (true);
		objectsController.isDialogShowing = false;
	}

	public void BtnCancel_Click() {
		displayCanvas.enabled = false;
		isBtnClicked = true;
		returnValue = false;
		isActive = false;
		DialogFinished (false);
		objectsController.isDialogShowing = false;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
