using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MenuGUI : MonoBehaviour {	/// <summary>
	/// 共通利用オブジェクト類管理インスタンス（インスペクタで初期化）
	/// </summary>
	public ObjectsController objects;
	/// <summary>
	/// メニュー部パネルのオブジェクト（インスペクタで初期化）
	/// </summary>
	public GameObject panelMenu;
	/// <summary>
	///  画面無効化用画像オブジェクト（インスペクタで初期化）
	/// </summary>
	public GameObject uiDisabledMaskImgObject;
	/// <summary>
	///  スロット番号入力部（インスペクタで初期化）
	/// </summary>
	public InputField inputFieldSlot;
	/// <summary>
	///  画面無効化用画像（uiDisabledMaskingObjectより）
	/// </summary>
	private Image uiDisabledMaskImg;
	/// <summary>
	///  スロット番号
	/// </summary>
	private int slotNum = 0;
	/// <summary>
	///  スロット最小値
	/// </summary>
	private const int SLOT_MINIMIZE = 0;
	/// <summary>
	///  スロット最大値
	/// </summary>
	private const int SLOT_MAXIMIZE = 99;
	/// <summary>
	///  ボタン配列（start()で初期化．PanelMenu中のすべてのボタンを格納）
	/// </summary>
	private Button[] btnArray;
	/// <summary>
	///  ファイル選択インスタンス（start()で初期化．MenuGUIにアタッチ済み）
	/// </summary>
	private FileChooser fileChooser;
	/// <summary>
	///  ボタン有効・無効フラグ
	/// </summary>
	private bool isBtnsDisabled;

	// Use this for initialization
	void Start () {
		// スロット番号入力部初期化
		inputFieldSlot.text = slotNum.ToString ();
		// 初期化
		fileChooser = this.GetComponent<FileChooser> ();
		btnArray = panelMenu.GetComponentsInChildren<Button> ();
		isBtnsDisabled = false;	
		// イベント登録
		fileChooser.OpenFileDialogFinished += new OpenFileDialogFinishedEventHander (OpenFileDialogFinished);
		fileChooser.SaveFileDialogFinished += new SaveFileDialogFinishedEventHander (SaveFileDialogFinished);
		objects.Dialog.DialogFinished += new DialogFinishedEventHandler (DialogFinished);

		// PanelMenuのColliderを調整（画面解像度により大きさが変わるので）
		BoxCollider2D collider = panelMenu.GetComponent<BoxCollider2D> ();
		Rect rect = panelMenu.GetComponent<RectTransform> ().rect;
		collider.size = rect.size;
		collider.offset = new Vector2 (rect.width / 2, rect.height / 2);

		//  画面無効用画像のサイズを調整（画面解像度により大きさが変わるので）
		RectTransform maskingRectTransform = uiDisabledMaskImgObject.GetComponent<RectTransform> ();	
		maskingRectTransform.sizeDelta = objects.DispCanvasRectTransform.rect.size;
		//  画面無効用画像インスタンス設定．非表示に．
		uiDisabledMaskImg = uiDisabledMaskImgObject.GetComponent<Image> ();
		uiDisabledMaskImg.enabled = false;
	}

	void Update () {
		// アニメーション再生時は停止ボタン以外を無効化する
		if (objects.IsAnimationPlaying == true) {
			if (isBtnsDisabled == false) {
				foreach (Button btn in btnArray) {
					if (btn.name != "BtnStop")
						btn.enabled = false;
				}
				isBtnsDisabled = true;
			}
		} 
		// ボタン無効化を解除
		else if (isBtnsDisabled == true) { 
			foreach(Button btn in btnArray) {
				btn.enabled = true;
			}
			isBtnsDisabled = false;
		}

}
	/// <summary>
	///  ダイアログ終了メソッド（イベント呼び出し）
	/// </summary>
	/// <param name="isBtnOKClicked">If set to <c>true</c> : OKボタンがクリックされた</param>
	private void DialogFinished(bool isBtnOKClicked) {
		// OKボタンがクリックされた場合，全フレームをリセットする．
		uiDisabledMaskImg.enabled = false;
		if (isBtnOKClicked == true) {
			objects.PanelFrames.AllFramesReset ();
		}
	}
	/// <summary>
	///  OpenFileDialog終了通知メソッド（イベント呼び出し）
	/// </summary>
	/// <param name="path">選択されたファイルパス</param>
	/// <param name="errorMessage">エラーメッセージ</param>
	private void OpenFileDialogFinished(string path, string errorMessage) {
		// 画面有効化．フラグをリセット．
		uiDisabledMaskImg.enabled = false;
		objects.IsAllObjectWaitRequest = false;
		// パスがセットされていない→メソッド終了
		if (string.IsNullOrEmpty (path))
			return;
		// JSONファイルを読出し，モーションデータに変換・表示．
		string readStr;
		using (StreamReader reader = new StreamReader (path)) {
			readStr = reader.ReadToEnd ();
		}
		objects.PanelFrames.MotionFramesRead (readStr);
	}
	/// <summary>
	///  SaveFileDialog終了通知メソッド（イベント呼び出し）
	/// </summary>
	/// <param name="path">選択されたファイルパス</param>
	/// <param name="errorMessage">エラーメッセージ</param>
	private void SaveFileDialogFinished(string path, string errorMessage) {
		// 画面有効化．フラグをリセット．
		uiDisabledMaskImg.enabled = false;
		objects.IsAllObjectWaitRequest = false;
		// パスがセットされていない→メソッド終了
		if (string.IsNullOrEmpty (path))
			return;
		// 拡張子が入力されていないとき，拡張子をつける
		if (Path.GetExtension (path) != ".json") {
			path += ".json";
		}
		// モーションデータからJSONファイルに変換し，そのファイルを保存する
		string fileName = System.IO.Path.GetFileNameWithoutExtension (path);
		string jsonStr = objects.MotionData.MotionJSONDataCreate (fileName);
		using (FileStream stream = File.Create (path)) {
			using (StreamWriter writer = new StreamWriter (stream)) {
				writer.Write (jsonStr);
			}
		}
	}

	public void BtnOpen_Click() {
		// 画面無効化，フラグセットを行いOpenFileDialogを表示
		objects.IsAllObjectWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
		fileChooser.OpenFileDialog_Show ();
	}
	public void BtnSave_Click() {
		// 画面無効化，フラグセットを行いSaveFileDialogを表示
		objects.IsAllObjectWaitRequest = true;
		uiDisabledMaskImg.enabled = true;
		fileChooser.SaveFileDialog_Show ();
	}
	public void BtnNew_Click() {
		objects.Dialog.Show ("本当に新規モーションを作成しますか？", 
			"現在の作業内容が破棄されます．" + System.Environment.NewLine + "保存がまだの場合は”キャンセル”をクリックしてください");
		uiDisabledMaskImg.enabled = true;
	}
	public void BtnDefaultPos_Click(GameObject motionDataObject) {
		// フレーム初期化
		objects.PanelFrames.FrameInitialize ();
	}
	public void BtnPlay_Click() {
		// アニメーション再生
		objects.PlenAnimation.AnimationPlay ();
	}
	public void BtnStop_Click() {
		// アニメーション停止
		objects.PlenAnimation.AnimationStop ();
	}
	public void BtnBackFrame_Click() {
		// ひとつ前のフレームを選択
		objects.PanelFrames.FrameGoBack ();
	}
	public void BtnForwardFrame_Click()
	{
		// ひとつ次のフレームを選択
		objects.PanelFrames.FrameGoNext ();
	}
	public void BtnInstall_Click() {
		// モーションインストールを行う
		string fileName = "fromMotionEditor";
		string jsonPath = ObjectsController.TmpFilePath + "/" + fileName + ".json";
		// JSONファイル作成．保存．
		string jsonStr = objects.MotionData.MotionJSONDataCreate (fileName);
		using (FileStream stream = File.Create (jsonPath)) {
			using (StreamWriter writer = new StreamWriter (stream)) {
				writer.Write (jsonStr);
			}
		}
		// 保存したJSONファイルをもとにモーションインストーラを起動．
		objects.MotionInstall.StartMotionInstallApp (@jsonPath,  @fileName);
	}
	public void BtnSync_Click() {
	}
	public void BtnMirror_Click() {
		objects.MotionData.ModelTurnOver ();
	}
	public void BtnMirrorRtoL_Click() {
		objects.MotionData.ModelMirror (true);
	}
	public void BtnMirrorLtoR_Click() {
		objects.MotionData.ModelMirror (false);
	}
	public void BtnCameraReset_Click() {
		objects.ModelViewController.CameraViewInitalize ();
	}
	public void BtnSample1_Click() {
		SampleMotionRead (1);
	}
	public void BtnSample2_Click() {
		SampleMotionRead (2);
	}
	public void BtnSample3_Click() {
		SampleMotionRead (3);
	}
	/// <summary>
	/// サンプルモーション読み込みメソッド
	/// </summary>
	/// <param name="index">Index</param>
	private void SampleMotionRead(int index) {
		string path = ObjectsController.SampleMotionDirPath + "Sample" + index.ToString () + ".json";
		// パスがセットされていない→メソッド終了
		if (string.IsNullOrEmpty (path))
			return;
		// JSONファイルを読出し，モーションデータに変換・表示．
		string readStr;
		using (StreamReader reader = new StreamReader (path)) {
			readStr = reader.ReadToEnd ();
		}
		objects.PanelFrames.MotionFramesRead (readStr);
	}
	/// <summary>
	///  スロット番号更新メソッド（イベント呼び出し．InputFieldSlotの値が変更された．）
	/// </summary>
	public void InputFieldSlotUpdate() {
		int tmp;
		// 入力された文字が数値か判別
		if (int.TryParse (inputFieldSlot.text, out tmp)) {
			// 入力された数値が最大値・最小値を超えていないか判別，値セット
			if (tmp > SLOT_MAXIMIZE) {
				slotNum = SLOT_MAXIMIZE;
				inputFieldSlot.text = SLOT_MAXIMIZE.ToString ();
			} else if (tmp < SLOT_MINIMIZE) {
				slotNum = SLOT_MINIMIZE;
				inputFieldSlot.text = SLOT_MINIMIZE.ToString ();
			} else {
				slotNum = tmp;
			}
			objects.MotionData.SlotNum = slotNum;
		} else {
			// 入力された文字が数値でなかったので復元
			inputFieldSlot.text = slotNum.ToString();
		}
	}
	/// <summary>
	///  スロット番号更新メソッド（InputFieldSlotの値を変更する）
	/// </summary>
	/// <param name="value">変更値（初期値：int.MinValue）．</param>
	public void InputFieldSlotUpdate(int value = int.MinValue) {
		// valueが最大値・最小値を超えていないか判別し，値をセット
		if (value >= SLOT_MINIMIZE && value <= SLOT_MAXIMIZE) {
			slotNum = value;
			inputFieldSlot.text = value.ToString();
			objects.MotionData.SlotNum = value;
		}

	}

}
