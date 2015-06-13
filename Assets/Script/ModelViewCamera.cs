using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ModelViewCamera : MonoBehaviour { 
	/// <summary>
	/// カメラの映像を表示するスペースを示すパネル
	/// </summary>
	public RectTransform viewerPanel;
	/// <summary>
	/// viewerPanelを格納しているCanvas
	/// </summary>
	public RectTransform viewerCanvas;
	/// <summary>
	/// カメラが捉えたいオブジェクト
	/// </summary>
	public Transform lookAtModel;
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
	private const float ZOOM_GAIN = 0.2f;
	/// <summary>
	/// オブジェクト並行移動ゲイン値
	/// </summary>
	private const float MOVE_GAIN = 0.002f;
	/// <summary>
	/// オブジェクトとの距離のゲイン値（オブジェクト並行移動時に利用）
	/// </summary>
	private float moveDistanceGain;

	private Transform clickedModelPart = null;

	public List<GameObject> AdjustableModelParts;
	public GameObject MotionDataObject;

	/***** 初回実行メソッド（オーバーライド） *****/
	void Start () {
		// カメラの表示座標を調整（viewerPanelにぴったりはまるように調整）
		this.GetComponent<Camera>().rect = new Rect (1 - viewerPanel.rect.width / viewerCanvas.rect.width
		                             , 1 - viewerPanel.rect.height / viewerCanvas.rect.height
		                             , viewerPanel.rect.width / viewerCanvas.rect.width
		                              , viewerPanel.rect.height / viewerCanvas.rect.height);
	}

	/***** 1フレームごとに呼び出されるメソッド（オーバーライド） *****/
	void Update () {
		// ホイールの回転量に合わせてカメラをズームイン（or ズームアウト）させる
		transform.Translate (new Vector3 (0.0f, 0.0f, Input.GetAxis ("Mouse ScrollWheel")));


		/*----- ここからマウスボタンのクリックイベント -----*/
		// 左ボタンクリック（画面回転）
		// note : 回転中心をクリックした位置にすることで操作感を向上
		//if(this.GetComponent<Camera>().rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) 
		{
			if (Input.GetMouseButton (0)) {
				// 押下した瞬間
				if (isMouseDown [0] == false) {
					clickedModelPart = null;

					RaycastHit hit;
					// マウスのがクリックした方向へのベクトルを作成
					Ray ray = this.GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
					// マウスクリックした位置に何らかのオブジェクトがあるか判断（あった場合hitに情報が格納される．検知距離は今回適当に1000としている．）
					// クリックした先にオブジェクトがあった場合，そのオブジェクトクリック位置を回転中心とする
					if (Physics.Raycast (ray, out hit, 1000f)) {
						foreach (GameObject adjustableModelPart in AdjustableModelParts) {
							if (adjustableModelPart.GetComponent<Collider>() == hit.collider) {
								clickedModelPart = adjustableModelPart.transform;
							}
						}
						posRotationCenter = hit.point;
					} 
					// クリックした位置に何もなかった場合，カメラの中央を回転中心とする
					else
						posRotationCenter = this.transform.TransformDirection (this.transform.forward);
					// 旧マウスポインタ座標を更新（この更新がないとカメラが予期せぬ方向に回転する）
					posBefore = Input.mousePosition;
					isMouseDown [0] = true;
				}
				if (clickedModelPart == null) {
					CameraRotation ();
				} else {
					JointRotation ();
				}


			} 
			// 左ボタンリリース
			else if (isMouseDown [0] == true && Input.GetMouseButtonUp (0)) {
				isMouseDown [0] = false;
			} 
			// 右ボタン押下（オブジェクト水平移動）
			// note : 物体との距離に応じて移動距離を変化させ，できるだけ画面上での物体移動量を同じにする
			else if (Input.GetMouseButton (1)) {
				//押下した瞬間
				if (isMouseDown [1] == false) {
					RaycastHit hit;
					// マウスのがクリックした方向へのベクトルを作成
					Ray ray = this.GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
					// マウスクリックした位置に何らかのオブジェクトがあるか判断（あった場合hitに情報が格納される．検知距離は今回適当に1000としている．）
					// クリックした先にオブジェクトがあった場合，物体との距離を格納
					if (Physics.Raycast (ray, out hit, 1000f)) {
						moveDistanceGain = hit.distance;
					} 
					// なにもオブジェクトがない場合，モデルとの距離を格納
					else
						moveDistanceGain = Vector3.Distance(this.transform.position, lookAtModel.position);
					// 旧マウスポインタ座標を更新（この更新がないと予期せぬ位置に移動する）
					posBefore = Input.mousePosition;
					isMouseDown [1] = true;
				}
				// モデルの移動（モデルとの距離を重みにし，できるだけ画面上での移動量を同じに見えるようにする）
				lookAtModel.Translate ((this.transform.right * (Input.mousePosition.x - posBefore.x) * MOVE_GAIN * moveDistanceGain) 
				                       + (this.transform.up * (Input.mousePosition.y - posBefore.y) * MOVE_GAIN * moveDistanceGain));
				// 旧マウスポインタ座標更新
				posBefore = Input.mousePosition;

			} 
			// 右ボタンリリース
			else if (isMouseDown [1] == true && Input.GetMouseButtonUp (1)) {
				isMouseDown [1] = false;
			} 
			// 中ボタン押下（ズーム）
			else if (Input.GetMouseButton (2)) {
				//押下した瞬間
				if(isMouseDown[2] == false) {
					// 旧マウスポインタ座標を更新（この更新がないと予期せぬズームが行われる）
					posBefore = Input.mousePosition;
					isMouseDown[2] = true;
				}
				// カメラズーム（マウスポインタの座標変異に応じてズーム量を調整）
				transform.Translate (new Vector3 (0.0f, 0.0f, ZOOM_GAIN * (Input.mousePosition.y - posBefore.y)));
				// 旧マウスポインタ座標更新
				posBefore = Input.mousePosition;
			} 
			// 中ボタンリリース
			else if (isMouseDown[2] == true && Input.GetMouseButtonUp (2)) {
				isMouseDown[2] = false;
			}
		}
	}
	/***** モデル平行移動メソッド *****/
	private void ModelMove () {
	}


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

		// カメラを注視点に向ける
		//trans.LookAt (forward);

		// ジンバルロック対策
		// カメラが真上や真下を向くとジンバルロックがおきる
		// ジンバルロックがおきるとカメラがぐるぐる回ってしまうので、一度に90度以上回るような計算結果になった場合は回転しないようにする(計算を元に戻す)
		Vector3 up = this.transform.up;
		if (Vector3.Angle (preUpV, up) > 90.0f) {
			this.transform.localEulerAngles = preAngle;
			this.transform.position = prePos;
		}

		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;
	}
	private void JointRotation() {
		JointName clickedJointName = clickedModelPart.GetComponent<JointParameter> ().name;
		MotionDataUpper motionData = MotionDataObject.GetComponent<MotionDataUpper> ();

		motionData.frameList [motionData.index].JointRotate (clickedJointName, 
			(posBefore.y - Input.mousePosition.y) * 5f);
		// 旧マウスポインタ座標更新
		posBefore = Input.mousePosition;

	}
	
}
