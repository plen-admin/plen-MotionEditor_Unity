using UnityEngine;
using System.Collections;

public class ObjectsController : MonoBehaviour {
	/// <summary>
	///  モーションデータ
	/// </summary>
	public MotionData motionData;
	/// <summary>
	/// メニューコントローラ
	/// </summary>
	public MenuGUI menuController;
	/// <summary>
	/// モデル表示コントローラ
	/// </summary>
	public ModelViewCamera modelViewController;
	/// <summary>
	/// ダイアログ
	/// </summary>
	public DialogScript dialog;
	/// <summary>
	/// ファイル選択（OpenFileDialog，SaveFileDialog）
	/// </summary>
	public FileChooser fileChooser;
	/// <summary>
	///  モーションインストーラ
	/// </summary>
	public MotionInstall motionInstall;
	/// <summary>
	/// モデルアニメーション
	/// </summary>
	public PLENModelAnimation plenAnimation;
	/// <summary>
	/// 全フレーム表示区域．フレーム関連の処理も担う．
	/// </summary>
	public PanelFramesScript panelFrames;
	/// <summary>
	///  キャンバスのRectTransform．ディスプレイにはこのキャンバス領域が表示される．
	/// </summary>
	public RectTransform dispCanvasRectTransform;
	/// <summary>
	/// アニメーション再生フラグ
	/// </summary>
	public bool isAnimationPlaying {
		get { 
			return _isAnimationPlaying;
		}
		set {
			_isAnimationPlaying = value;
		}
	}
	/// <summary>
	/// 全オブジェクト待機フラグ
	/// </summary>
	/// <value><c>true</c> if is all object wait request; otherwise, <c>false</c>.</value>
	public bool isAllObjectWaitRequest {
		get { 
			return (_isAllObjectWaitRequest | _isDialogShowing); 
		}
		set {
			_isAllObjectWaitRequest = value;
		}
	}
	/// <summary>
	///  Dialog表示中メソッド
	/// </summary>
	public bool isDialogShowing {
		get {
			return _isDialogShowing; 
		}
		set {
			_isDialogShowing = value;
		}
	}
	/// <summary>
	/// フレーム表示区域待機フラグ
	/// </summary>
	public bool isFrameRelationWaitRequest {
		get {
			return (_isAllObjectWaitRequest | _isAnimationPlaying | _isAnimationPlaying | _isFrameRelationWaitRequest);
		}
		set {
			_isFrameRelationWaitRequest = value;
		}
	}
	/// <summary>
	///  一時ファイル保存先（読み取り専用）
	/// </summary>
	public  static string tmpFilePath {
		get { return _tmpFilePath; }
	}

	public static string externalFilePath {
		get { return _externalFilePath; }
	}
	public static string sampleMotionDirPath {
		get { return _sampleMotionDirPath; }
	}
	private static string _tmpFilePath;
	private static string _externalFilePath;
	private static string _sampleMotionDirPath;
	private bool _isAnimationPlaying;
	private bool _isFrameRelationWaitRequest;
	private bool _isAllObjectWaitRequest;
	private bool _isDialogShowing;

	void Awake() {
		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			_tmpFilePath = Application.dataPath + "/../tmp/";
			_externalFilePath = Application.dataPath + "/../Plugins/Windows/";
			_sampleMotionDirPath = Application.dataPath + "/../Plugins/SampleMotion/";
		} else if (Application.platform == RuntimePlatform.OSXPlayer) {
			_tmpFilePath = Application.dataPath + "/../../tmp/";
			_externalFilePath = Application.dataPath + "/../../Plugins/OSX/";
			_sampleMotionDirPath = Application.dataPath + "/../../Plugins/SampleMotion/";
		} else {
			_tmpFilePath = Application.dataPath + "/tmp/";
			_externalFilePath = Application.dataPath + "/Plugins/";
			_sampleMotionDirPath = Application.dataPath + "/Plugins/SampleMotion/";
		}
	}

	// Use this for initialization
	void Start () {
		motionInstall = this.GetComponent<MotionInstall> ();



	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
