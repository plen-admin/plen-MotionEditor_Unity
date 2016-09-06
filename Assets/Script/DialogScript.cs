using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void DialogFinishedEventHandler(bool isBtnOKClicked);

public class DialogScript : MonoBehaviour, IObjects {
    /// <summary>
    /// 共通利用オブジェクト類管理インスタンス
    /// </summary>
    private ObjectsController objects;
    /// <summary>
    ///  ダイアログ表示キャンバス（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private Canvas displayCanvas;
    /// <summary>
    ///  ダイアログタイトル（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private Text labelTitle;
    /// <summary>
    ///  ダイアログメッセージ（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private Text labelMessage;
	/// <summary>
	///  ダイアログ終了イベント（引数...true：OKボタン押下，false：それ以外）
	/// </summary>
	public event DialogFinishedEventHandler DialogFinished;

    public void Initialize(ObjectsController controller) {
        objects = controller;
    }

	// Use this for initialization
	void Start () {
		// デフォルトではダイアログ非表示
		displayCanvas.enabled = false;
	}
	/// <summary>
	///  ダイアログ表示メソッド
	/// </summary>
	/// <param name="title">タイトル</param>
	/// <param name="message">メッセージ</param>
	public void Show(string title, string message) {
		// フラグを設定し，画面表示	
		objects.IsDialogShowing = true;
		labelTitle.text = title;
		labelMessage.text = message;
		displayCanvas.enabled = true;
	}
	/// <summary>
	///  OKボタン押下メソッド（イベント呼び出し）
	/// </summary>
	public void BtnOK_Click() {
		// フラグを整理し，OKが押されたことを通知
		displayCanvas.enabled = false;
		DialogFinished (true);
		objects.IsDialogShowing = false;
	}
	/// <summary>
	/// Cancelボタン押下メソッド（イベント呼び出し）
	/// </summary>
	public void BtnCancel_Click() {
		// フラグを整理し，Cancelが押されたことを通知
		displayCanvas.enabled = false;
		DialogFinished (false);
		objects.IsDialogShowing = false;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
