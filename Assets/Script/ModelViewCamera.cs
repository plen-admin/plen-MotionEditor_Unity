using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ModelViewCamera : MonoBehaviour { 
	/// <summary>
	/// 共通利用オブジェクト類管理インスタンス（インスペクタで初期化）
	/// </summary>
	public ObjectsController objects;
	/// <summary>
	/// カメラの映像を表示するスペースを示すパネル（インスペクタで初期化）
	/// </summary>
	public RectTransform viewerPanel;
	/// <summary>
	/// viewerPanelを格納しているCanvas（インスペクタで初期化）
	/// </summary>
	public RectTransform viewerCanvas;
	/// <summary>
	/// カメラが捉えたいオブジェクト（インスペクタで初期化）
	/// </summary>
	public Transform lookAtModel;
	/// <summary>
	/// 回転基準座標
	/// </summary>
	public Transform rotReferenceCoord;
	/// <summary>
	///  角度インジケータオブジェクト（インスペクタで初期化）
	/// </summary>
	public GameObject angleIndicator;
	/// <summary>
	/// マウスボタン押下フラグ(左，右，中）
	/// </summary>
	private bool[] isMouseDown = {false, false, false};
	/// <summary>
	/// 旧マウスポインタ座標
	/// </summary>
	private Vector3 posBefore;
	/// <summary>
	/// 回転中心座標
	/// </summary>
	private Vector3 posRotationCenter;
	/// <summary>
	/// ズームゲイン値
	/// </summary>
	private const float ZOOM_GAIN = 0.05f;
	/// <summary>
	/// オブジェクト並行移動ゲイン値
	/// </summary>
	private const float MOVE_GAIN = 0.002f;
	/// <summary>
	/// オブジェクトとの距離のゲイン値（オブジェクト並行移動時に利用）
	/// </summary>
	private float moveDistanceGain;
	/// <summary>
	/// クリックされた3Dモデル可動パーツ
	/// </summary>
	private Transform clickedModelPart = null;
	/// <summary>
	/// インジケータのラベル
	/// </summary>
	private Text labelAngleIndicator;
	/// <summary>
	/// 3D可動パーツリスト
	/// </summary>
	private  List<GameObject> AdjustableModelParts =  null;
	private Vector3 defaultCameraPos;
	private Vector3 defaultModelPos;
	private Quaternion defaultCameraRot;

	/***** 初回実行メソッド（オーバーライド） *****/
	void Start () {
		// カメラの表示座標を調整（viewerPanelにぴったりはまるように調整）
		this.GetComponent<Camera>().rect = new Rect (1 - viewerPanel.rect.width / viewerCanvas.rect.width
		                             , 1 - viewerPanel.rect.height / viewerCanvas.rect.height
		                             , viewerPanel.rect.width / viewerCanvas.rect.width
		                              , viewerPanel.rect.height / viewerCanvas.rect.height);

		labelAngleIndicator = angleIndicator.GetComponentInChildren<Text> ();
		angleIndicator.SetActive (false);
		// ViewerPanelのColliderを調整（画面解像度により大きさが変わるので）
		BoxCollider2D collider = viewerPanel.GetComponent<BoxCollider2D> ();
		Rect rect = viewerPanel.GetComponent<RectTransform> ().rect;
		collider.size = rect.size;
		collider.offset = new Vector2 (rect.width / 2, rect.height / 2);

		defaultCameraPos = transform.position;
		defaultCameraRot = transform.rotation;
		defaultModelPos = lookAtModel.position;
	}

	/***** 1フレームごとに呼び出されるメソッド（オーバーライド） *****/
	void Update () {
		/*----- ここからマウスボタンのクリックイベント -----*/
		// マウスポインタが指定のパネル内にあり，かつmenu（ダイアログ表示
		// note...ダイアログ表示時は誤作動防止のため操作を無効にする
		if (viewerPanel.GetComponent<Collider2D> ().OverlapPoint (Input.mousePosition)
			&& objects.isAllObjectWaitRequest == false) {

			// ホイールの回転量に合わせてカメラをズームイン（or ズームアウト）させる
			transform.Translate (new Vector3 (0.0f, 0.0f, Input.GetAxis ("Mouse ScrollWheel")));

			// 左ボタンクリック（画面回転）
			// note : 回転中心をクリックした位置にすることで操作感を向上
			if (Input.GetMouseButton (0)) {
				// モデル可動パーツリストを作成
				if (AdjustableModelParts == null) {
					AdjustableModelParts = new List<GameObject> (objects.motionData.modelJointList);
				}
				// 押下した瞬間
				if (isMouseDown [0] == false) {
					clickedModelPart = null;
					RaycastHit hit;
					// マウスのがクリックした方向へのベクトルを作成
					Ray ray = this.GetComponent<Camera> ().ScreenPointToRay (Input.mousePosition);
					// マウスクリックした位置に何らかのオブジェクトがあるか判断（あった場合hitに情報が格納される．検知距離は適当に1000としている．）
					// クリックした先にオブジェクトがあった場合，そのオブジェクトクリック位置を回転中心とする
					if (Physics.Raycast (ray, out hit, 1000f)) {
						// クリックしたオブジェクトが可動パーツか判定
						foreach (GameObject adjustableModelPart in AdjustableModelParts) {
							if (adjustableModelPart.GetComponent<Collider> () == hit.collider) {
								
								clickedModelPart = adjustableModelPart.transform;
								// 角度インジケータの位置調整・表示
								angleIndicator.SetActive (true);
								angleIndicator.transform.position = Input.mousePosition;
							}
						}
					} 
					// カメラの回転中心はPLEN本体の中心
					posRotationCenter = rotReferenceCoord.position;
					// 旧マウスポインタ座標を更新（この更新がないとカメラが予期せぬ方向に回転する）
					posBefore = Input.mousePosition;
					isMouseDown [0] = true;
				}
				if (clickedModelPart == null) {
					//可動パーツ以外をクリック
					CameraRotation ();
				} else {
					// 可動パーツをクリック（アニメーション再生時は操作不可に）
					if (objects.isAnimationPlaying == false) {
						JointRotation ();
					}
				}
			} 
			// 左ボタンリリース
			else if (isMouseDown [0] == true && Input.GetMouseButtonUp (0)) {
				isMouseDown [0] = false;
				angleIndicator.SetActive (false);	// 角度インジケータ非表示
			} 
			// 右ボタン押下（オブジェクト水平移動）
			// note : 物体との距離に応じて移動距離を変化させ，できるだけ画面上での物体移動量を同じにする
			else if (Input.GetMouseButton (1)) {
				//押下した瞬間
				if (isMouseDown [1] == false) {
					RaycastHit hit;
					// マウスのがクリックした方向へのベクトルを作成
					Ray ray = this.GetComponent<Camera> ().ScreenPointToRay (Input.mousePosition);
					// マウスクリックした位置に何らかのオブジェクトがあるか判断（あった場合hitに情報が格納される．検知距離は今回適当に1000としている．）
					// クリックした先にオブジェクトがあった場合，物体との距離を格納
					if (Physics.Raycast (ray, out hit, 1000f)) {
						moveDistanceGain = hit.distance;
					} 
					// なにもオブジェクトがない場合，モデルとの距離を格納
					else
						moveDistanceGain = Vector3.Distance (this.transform.position, lookAtModel.position);
					// 旧マウスポインタ座標を更新（この更新がないと予期せぬ位置に移動する）
					posBefore = Input.mousePosition;
					isMouseDown [1] = true;
				}
				ModelMove ();
			} 
			// 右ボタンリリース
			else if (isMouseDown [1] == true && Input.GetMouseButtonUp (1)) {
				isMouseDown [1] = false;
			} 
			// 中ボタン押下（ズーム）
			else if (Input.GetMouseButton (2)) {
				//押下した瞬間
				if (isMouseDown [2] == false) {
					// 旧マウスポインタ座標を更新（この更新がないと予期せぬズームが行われる）
					posBefore = Input.mousePosition;
					isMouseDown [2] = true;
				}
				CameraZoom ();
			} 
			// 中ボタンリリース
			else if (isMouseDown [2] == true && Input.GetMouseButtonUp (2)) {
				isMouseDown [2] = false;
			}
		} else {
			isMouseDown [0] = false;
			isMouseDown [1] = false;
			isMouseDown [2] = false;
		}

	}
	/// <summary>
	/// カメラ表示区域・位置初期化メソッド
	/// </summary>
	public void CameraViewInitalize() {
		transform.position = defaultCameraPos;
		transform.rotation = defaultCameraRot;
		lookAtModel.position = defaultModelPos;
	}

	/// <summary>
	/// モデル平行移動メソッド
	/// </summary>
	private void ModelMove () {
		// モデルの移動（モデルとの距離を重みにし，できるだけ画面上での移動量を同じに見えるようにする）
		lookAtModel.Translate ((this.transform.right * (Input.mousePosition.x - posBefore.x) * MOVE_GAIN * moveDistanceGain) 
			+ (this.transform.up * (Input.mousePosition.y - posBefore.y) * MOVE_GAIN * moveDistanceGain));
		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;
	}
	// カメラズームメソッド
	private void CameraZoom() {
		// カメラズーム（マウスポインタの座標変異に応じてズーム量を調整）
		transform.Translate (new Vector3 (0.0f, 0.0f, ZOOM_GAIN * (Input.mousePosition.y - posBefore.y)));
		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;
	}
	/// <summary>
	///  カメラ回転メソッド
	/// </summary>
	private void CameraRotation() {
		// マウスポインタ座標の差異を取得，回転座標化
		Vector3 eulerAngle = new Vector3 (Input.mousePosition.y - posBefore.y, Input.mousePosition.x - posBefore.x, 0.00f);
		// 回転前のカメラの情報を保存する
		Vector3 preUpV, preAngle, prePos;
		preUpV = this.transform.up;
		preAngle = this.transform.localEulerAngles;
		prePos = this.transform.position;

		// カメラの回転
		// 横方向の回転はグローバル座標系のY軸で回転する
		this.transform.RotateAround (posRotationCenter, Vector3.up, eulerAngle.y);
		// 縦方向の回転はカメラのローカル座標系のX軸で回転する
		this.transform.RotateAround (posRotationCenter, this.transform.right, -eulerAngle.x);

		// 　ジンバルロック対策
		Vector3 up = this.transform.up;
		if (Vector3.Angle (preUpV, up) > 90.0f) {
			this.transform.localEulerAngles = preAngle;
			this.transform.position = prePos;
		}

		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;
	}
	/// <summary>
	///  関節オブジェクト回転メソッド
	/// </summary>
	private void JointRotation() {
		int frameIndex = objects.motionData.index;
		// 選択した関節名を取得
		PLEN.JointName clickedJointName = clickedModelPart.GetComponent<JointParameter> ().Name;

		// オブジェクト回転（回転量はマウスy座標の変位量）
		objects.motionData.frameList [frameIndex].JointRotate (clickedJointName, 
			(Input.mousePosition.y - posBefore.y) * 2.0f);

		labelAngleIndicator.text = objects.motionData.frameList [frameIndex].jointAngles [(int)clickedJointName].Angle.ToString ();
		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;

	}
	
}
