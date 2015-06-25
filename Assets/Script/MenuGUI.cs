using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System.Collections;
using System.IO;

public class MenuGUI : MonoBehaviour {
	public PLENModelAnimation plenAnimation;
	public PanelFramesScript panelFrames;
	public DialogScript dialog;
	public FileChooser fileChooser;
	public MotionData motionData;
	public Image uiDisabledMaskImg;
	public bool isWaitRequest;
	private Vector2 clickPos;

	private bool isDialogWaiting = false;

	// Use this for initialization
	void Start () {
		uiDisabledMaskImg.enabled = false;
		isWaitRequest = false;
		fileChooser.OpenFileDialogFinished += new OpenFileDialogFinishedEventHander (OpenFileDialogFinished);
		fileChooser.SaveFileDialogFinished += new SaveFileDialogFinishedEventHander (SaveFileDialogFinished);
	}
	
	// Update is called once per frame
	void Update () {
		if (isDialogWaiting == true && dialog.isBtnClicked == true) {
			isDialogWaiting = false;
			isWaitRequest = false;
			uiDisabledMaskImg.enabled = false;
			if (dialog.returnValue == true) {
				panelFrames.AllFramesReset ();
			}
		}
	}

	private void OpenFileDialogFinished(string path, string errorMessage) {
		Debug.Log (path);
		isWaitRequest = false;
		uiDisabledMaskImg.enabled = false;

		if (string.IsNullOrEmpty (path))
			return;
		
		string readStr;

		using (StreamReader reader = new StreamReader (path)) {
			readStr = reader.ReadToEnd ();
		}
		Debug.Log (panelFrames.MotionFramesRead(readStr));


		// スタンドアロンデバッグ用
/*		dialog.Show ("デバッグ用　必ず【キャンセル】を押すように！！！！！！", 
			"path : " + path + System.Environment.NewLine + "errorMessage : " + errorMessage);
		isDialogWaiting = true;
*/	}

	private void SaveFileDialogFinished(string path, string errorMessage) {
		Debug.Log (path);
		isWaitRequest = false;
		uiDisabledMaskImg.enabled = false;

		if (string.IsNullOrEmpty (path))
			return;
		
		if (Path.GetExtension (path) != ".json") {
			path += ".json";
		}
		string jsonStr = motionData.MotionJSONDataCreate (path);

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
		fileChooser.OpenFileDialog_Show ();
		isWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
	}
	public void BtnSave_Click()
	{
		fileChooser.SaveFileDialog_Show ();
		isWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
	}
	public void BtnNew_Click()
	{
		dialog.Show ("本当に新規モーションを作成しますか？", 
			"現在の作業内容が破棄されます．" + System.Environment.NewLine + "保存がまだの場合は”キャンセル”をクリックしてください");
		isDialogWaiting = true;
		isWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
	}
	public void BtnDefaultPos_Click(GameObject motionDataObject)
	{
		panelFrames.FrameInitialize ();
	}
	public void BtnPlay_Click()
	{
		plenAnimation.AnimationPlay ();
	}
	public void BtnStop_Click()
	{
		plenAnimation.AnimationStop ();
	}
	public void BtnBackFrame_Click()
	{
		panelFrames.FrameGoBack ();
	}
	public void BtnForwardFrame_Click()
	{
		panelFrames.FrameGoNext ();
	}
	public void BtnConnect_Click()
	{
	}
	public void BtnMS_Click()
	{
	}

}
