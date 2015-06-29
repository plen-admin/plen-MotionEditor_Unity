using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MenuGUI : MonoBehaviour {
	public ObjectsController objectsController;
	public GameObject panelMenu;
	public Image uiDisabledMaskImg;
	private Vector2 clickPos;

	private Button[] btnArray;
	private FileChooser fileChooser;
	private bool isBtnsDisabled;

	// Use this for initialization
	void Start () {
		fileChooser = this.GetComponent<FileChooser> ();
		btnArray = panelMenu.GetComponentsInChildren<Button> ();

		isBtnsDisabled = false;	
		uiDisabledMaskImg.enabled = false;
		fileChooser.OpenFileDialogFinished += new OpenFileDialogFinishedEventHander (OpenFileDialogFinished);
		fileChooser.SaveFileDialogFinished += new SaveFileDialogFinishedEventHander (SaveFileDialogFinished);
		objectsController.dialog.DialogFinished += new DialogFinishedEventHandler (DialogFinished);
	}
	
	// Update is called once per frame
	void Update () {
		if (objectsController.isAnimationPlaying == true) {
			if (isBtnsDisabled == false) {
				foreach (Button btn in btnArray) {
					if (btn.name != "BtnStop")
						btn.enabled = false;
				}
				isBtnsDisabled = true;
			}
		} else if (isBtnsDisabled == true) { 
			foreach(Button btn in btnArray) {
				btn.enabled = true;
			}
			isBtnsDisabled = false;
		}
/*		if (isDialogWaiting == true && dialog.isBtnClicked == true) {
			isDialogWaiting = false;
			isWaitRequest = false;
			uiDisabledMaskImg.enabled = false;
			if (dialog.returnValue == true) {
				panelFrames.AllFramesReset ();
			}
		}
*/	}

	private void DialogFinished(bool isBtnOKClicked) {
		uiDisabledMaskImg.enabled = false;
		if (isBtnOKClicked == true) {
			objectsController.panelFrames.AllFramesReset ();
		}
	}

	private void OpenFileDialogFinished(string path, string errorMessage) {
		Debug.Log (path);
		uiDisabledMaskImg.enabled = false;
		objectsController.isAllObjectWaitRequest = false;
		if (string.IsNullOrEmpty (path))
			return;
		
		string readStr;

		using (StreamReader reader = new StreamReader (path)) {
			readStr = reader.ReadToEnd ();
		}
		Debug.Log (objectsController.panelFrames.MotionFramesRead(readStr));


		// スタンドアロンデバッグ用
/*		dialog.Show ("デバッグ用　必ず【キャンセル】を押すように！！！！！！", 
			"path : " + path + System.Environment.NewLine + "errorMessage : " + errorMessage);
		isDialogWaiting = true;
*/	}

	private void SaveFileDialogFinished(string path, string errorMessage) {
		Debug.Log (path);
		uiDisabledMaskImg.enabled = false;
		objectsController.isAllObjectWaitRequest = false;
		if (string.IsNullOrEmpty (path))
			return;
		
		if (Path.GetExtension (path) != ".json") {
			path += ".json";
		}
		string jsonStr = objectsController.motionData.MotionJSONDataCreate (path);

		using (FileStream stream = File.Create (path)) {
			using (StreamWriter writer = new StreamWriter (stream)) {
				writer.Write (jsonStr);
			}
		}
		// スタンドアロンデバッグ用
/*		dialog.Show ("デバッグ用　必ず【キャンセル】を押すように！！！！！！", 
			"path : " + path + System.Environment.NewLine + "errorMessage : " + errorMessage);
		isDialogWaiting = true;
*/	}

	public void BtnOpen_Click()
	{
		objectsController.isAllObjectWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
		fileChooser.OpenFileDialog_Show ();
	}
	public void BtnSave_Click()
	{
		objectsController.isAllObjectWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
		fileChooser.SaveFileDialog_Show ();
	}
	public void BtnNew_Click()
	{
		objectsController.dialog.Show ("本当に新規モーションを作成しますか？", 
			"現在の作業内容が破棄されます．" + System.Environment.NewLine + "保存がまだの場合は”キャンセル”をクリックしてください");
		uiDisabledMaskImg.enabled = true;
	}
	public void BtnDefaultPos_Click(GameObject motionDataObject)
	{
		objectsController.panelFrames.FrameInitialize ();
	}
	public void BtnPlay_Click()
	{
		objectsController.plenAnimation.AnimationPlay ();
	}
	public void BtnStop_Click()
	{
		objectsController.plenAnimation.AnimationStop ();
	}
	public void BtnBackFrame_Click()
	{
		objectsController.panelFrames.FrameGoBack ();
	}
	public void BtnForwardFrame_Click()
	{
		objectsController.panelFrames.FrameGoNext ();
	}
	public void BtnConnect_Click()
	{
	}
	public void BtnMS_Click()
	{
	}

}
