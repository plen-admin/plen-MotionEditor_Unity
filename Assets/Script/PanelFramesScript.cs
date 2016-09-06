using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PanelFramesScript : MonoBehaviour, IObjects {
    /// <summary>
    /// フレーム削除アニメーション再生中フラグ
    /// Note...このフラグはフレーム（FrameImgScript)でセットされる
    /// </summary>
    public bool IsFramePlayingDestroyAnimation {
        get {
            return isFramePlayingDestroyAnimation;
        }
        set {
            isFramePlayingDestroyAnimation = value;
        }
    }
    /// <summary>
    /// フレーム数（読み取り専用）
    /// </summary>
    public int FrameCount {
        get {
            return frameImgList.Count;
        }
    }
    /// <summary>
    /// 共通利用オブジェクト類管理インスタンス
    /// </summary>
    private ObjectsController objects;
    /// <summary>
    /// frameImgのプレハブ（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private GameObject frameImgPrefub;
    /// <summary>
    /// モーションデータを管理するインスタンス（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private GameObject layoutArea;
    /// <summary>
    /// 横方向スクロールバー（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private Scrollbar scrollBarH;
    /// <summary>
    /// フレーム間の隙間サイズ（インスペクタで初期化）
    /// </summary>
    [SerializeField]
    private float framePadding;
    /// <summary>
    /// frameImgリスト
    /// </summary>
    private List<GameObject> frameImgList = new List<GameObject>();
    /// <summary>
    /// 現在選択中のframeImgのindex
    /// </summary>
    private int selectedIndex;
	/// <summary>
	/// フレーム選択判定フラグ
	/// </summary>
	private bool isChildFrameImgClicked = false;
	/// <summary>
	/// 末端スクロール要求フラグ
	/// </summary>
	private bool isRequestToMoveScrollBar = false;
	/// <summary>
	/// 先頭フレームから末端フレームまでの幅
	/// </summary>
	private float layoutAreaWidth;
	/// <summary>
	///  フレーム表示パネルのRectTransform
	/// </summary>
	private RectTransform thisRectTransform;
	/// <summary>
	///  フレーム表示パネルのCollider
	/// </summary>
	private BoxCollider2D thisCollider;
	/// <summary>
	///  フレーム表示領域のRectTransforn
	/// </summary>
	private RectTransform layoutAreaRectTransfrom;
	/// <summary>
	/// フレーム表示領域のCollider
	/// </summary>
	private BoxCollider2D layoutAreaCollider;

	private Vector3 clickPos;
    private bool isFramePlayingDestroyAnimation = false;

    public void Initialize(ObjectsController controller) {
        objects = controller;
        if (objects.PlenAnimation != null) {
            objects.PlenAnimation.AnimationStarted += AnimationStarted;
            objects.PlenAnimation.AnimationClipChanged += AnimationClipChanged;
            objects.PlenAnimation.AnimationEnded += AnimationEnded;
        }
    }

    // Use this for initialization
    void Start () {
		// インスタンス類初期化
		thisRectTransform = this.GetComponent<RectTransform> ();
		layoutAreaRectTransfrom = layoutArea.GetComponent<RectTransform> ();
		layoutAreaCollider = layoutArea.GetComponent<BoxCollider2D> ();
		// 初期フレーム作成
		selectedIndex = 0;
		CreateNewFrameImg (0);
		clickPos = new Vector3 (0, 0, 0);

		// ViewerPanelのColliderを調整（画面解像度により大きさが変わるので）
		thisCollider = this.GetComponent<BoxCollider2D> ();
		Rect rect = this.GetComponent<RectTransform> ().rect;
		thisCollider.size = new Vector2 (rect.width, layoutAreaRectTransfrom.rect.height);
		thisCollider.offset = new Vector2 (thisCollider.size.x / 2, -rect.height / 2);
	}
	
	// Update is called once per frame
	void Update () {
		// Frame関連の待機リクエストがあるか判定
		if(objects.IsFrameRelationWaitRequest == false) {

			if (clickPos == new Vector3(0,0,0) && Input.GetMouseButton(0)) {
				clickPos = Input.mousePosition;
			}
			// マウスクリックを検知し，フレーム削除中でないでないことを確認
			else if (Input.GetMouseButtonUp (0) && isFramePlayingDestroyAnimation == false) {
				
				// フレームがクリックされていなく，かつフレーム表示区域をクリック → 新規フレーム作成
				if (isChildFrameImgClicked == false && thisCollider.OverlapPoint(Input.mousePosition)) {

					if (!Vector3NearlyEqual(clickPos, Input.mousePosition, 10)) {
						clickPos = new Vector3 (0, 0, 0);
						return;
					}

					if (frameImgList.Count >= 20) {
						return;
					}

					int createIndex = -1;	// 作成されるフレームのインデックス
					// クリック位置のcollider一覧を取得
					RaycastHit2D[] hits = Physics2D.RaycastAll (Input.mousePosition, -Vector2.up);
					for (int i = 0; i < hits.Length; i++) {
						// クリックされたcolliderの中から先頭フレームから終端フレームまでを覆うcolliderを判別
						// Note...これによりフレームとフレームの隙間をクリックしたことを判定できる
						if (hits [i].collider == layoutAreaCollider) {
							// クリック位置のx座標を取得（クリック座標は画面の座標を基準としているので，基準をフレーム表示区域に）
							float clickPosX = Input.mousePosition.x * (thisRectTransform.rect.width / Screen.width);
							// フレーム表示区域のスクロール量を検知し，スクロール分座標を追加する
							float normalizedPos = this.GetComponent<ScrollRect>().horizontalNormalizedPosition;
							clickPosX += normalizedPos * (layoutAreaWidth - thisRectTransform.rect.width + framePadding);
							// どのフレームの間がクリックされたか判定．フレーム作成位置を決定
							for (int j = 0; j < frameImgList.Count; j++) {
								if (clickPosX < (j + 1) * layoutAreaWidth / frameImgList.Count) {
									createIndex = j;
									break;
								}
							}
						}
					}
					// createIndex < 0 → フレームとフレームとの隙間をクリックしていない → フレーム群の末端に新規作成
					if(createIndex < 0){
						createIndex = frameImgList.Count;
						// スクロール位置を末端にするフラグをセット
						isRequestToMoveScrollBar = true;
					}
					// 新規フレーム作成
					CreateNewFrameImg (createIndex);
				} 
				// フレームが選択されていた際はフラグ解除（すでに処理はChildFrameImgClickで終了済み）
				else {
					isChildFrameImgClicked = false;
				}
				clickPos = new Vector3 (0, 0, 0);
			}
		}

	}

	void LateUpdate() {
		// FrameImgLayAoutAreaのColliderサイズを調整
		// Note...Content Size FilterによりlayoutAreaのWidthが可変的，かつサイズ変更のタイミングが不明なため，
		//        Colliderのサイズ調整が必要．
		layoutAreaWidth = layoutAreaRectTransfrom.rect.width - framePadding;
		layoutAreaCollider.size = new Vector2 (layoutAreaWidth, layoutAreaCollider.size.y);
		layoutAreaCollider.offset = new Vector2 (layoutAreaWidth / 2, 0f);
	}

	bool Vector3NearlyEqual(Vector3 vec1, Vector3 vec2, float delta) {
		if (Mathf.Abs (vec1.x - vec2.x) > delta)
			return false;
		if (Mathf.Abs (vec1.y - vec2.y) > delta)
			return false;
		if (Mathf.Abs (vec1.z - vec2.z) > delta)
			return false;
		
		return true;
	}

	/// <summary>
	/// 全フレームリセット（モーション新規作成）メソッド
	/// </summary>
	public void AllFramesReset() {
		while (frameImgList.Count > 1) {
			ChildFrameImgDestroy (0);
		}
		objects.MotionData.FrameInitialize (0, true);
		frameImgList [0].GetComponent<FrameImgScript> ().SliderTime.value = objects.MotionData.FrameList [0].TransitionTime;
	}
	/// <summary>
	/// 現在選択中のフレームを初期化する（モデルを初期位置にする）メソッド
	/// </summary>
	public void FrameInitialize() {
		objects.MotionData.FrameInitialize (selectedIndex);
	}
	/// <summary>
	/// フレーム表示区域スクロール検知メソッド（イベント発生）
	/// </summary>
	public void ScrollBarH_OnValueChanged() {
		// 末端までスクロールするフラグがセットされていたら末端まで移動
		if (isRequestToMoveScrollBar == true) {
			scrollBarH.value = 1;
			isRequestToMoveScrollBar = false;
		}

	}
	/// <summary>
	/// モーションファイルからフレームを読み込むメソッド
	/// </summary>
	/// <returns><c>true</c> : 成功 <c>false</c> : 失敗</returns>
	/// <param name="motionJsonStr">モーションファイル(JSON）</param>
	public bool MotionFramesRead(string motionJsonStr) {
		// モーションファイル読み込み(motionDataに反映)．失敗した場合本メソッドを終了．
		if (objects.MotionData.MotionJSONDataRead (motionJsonStr) == false)
			return false;

		/*-- ここからフレーム読み込み・作成開始 --*/

		// 現在のフレームをすべて破棄
		foreach (GameObject frameImg in frameImgList) {
			Destroy (frameImg);
		}
		frameImgList.Clear ();

		// フレーム作成
		for (int i = 0; i < objects.MotionData.FrameList.Count; i++) {
			CreateNewFrameImg (i, true);
		}
		// フレーム背景画像を初期化
		StartCoroutine (FrameImgTexInitialize());

		return true;
	}

	/// <summary>
	/// 選択フレームを一つ前にするメソッド
	/// </summary>
	public void FrameGoBack() {
		if (selectedIndex > 0) {
			objects.PlenAnimation.AnimationPlay (selectedIndex, selectedIndex - 1);
		}
	}
	/// <summary>
	/// 選択フレームを一つ次にするメソッド
	/// </summary>
	public void FrameGoNext() {
		if (selectedIndex < frameImgList.Count - 1) {
			objects.PlenAnimation.AnimationPlay (selectedIndex, selectedIndex + 1);
		}
	}
	/// <summary>
	/// フレーム削除メソッド
	/// </summary>
	/// <param name="destroyIndex">削除フレームインデックス</param>
	public void ChildFrameImgDestroy(int destroyIndex) {
		// フレーム選択フラグセット
		isChildFrameImgClicked = true;
		isFramePlayingDestroyAnimation = false;
		// すでにフレームが一つしかない場合は削除できない
		if (frameImgList.Count == 1)
			return;
		// フレームオブジェクト破棄．リストから除去．
		Destroy (frameImgList [destroyIndex]);
		frameImgList.RemoveAt (destroyIndex);
		objects.MotionData.FrameRemove (destroyIndex);
		// 各フレームのインデックスを再設定（影響のあるフレームのみ）
		for (int i = destroyIndex; i < frameImgList.Count; i++) {
			frameImgList [i].GetComponent<FrameImgScript> ().Index = i;
		}
		// 末端フレームを削除した場合，現在の末端フレームを選択状態にする
		if (selectedIndex > frameImgList.Count - 1) {
			selectedIndex = frameImgList.Count - 1;
			CallSelectedFrameImgChanged ();
			objects.MotionData.ChangeSelectFrame (selectedIndex);
		}
		// それ以外の場合，選択フレーム変更を再度通知する（インデックスはそのままで通知のみ）
		else if (selectedIndex == destroyIndex) {
			CallSelectedFrameImgChanged ();
			objects.MotionData.ChangeSelectFrame (selectedIndex);
		}
	}
	/// <summary>
	///  フレームクリックメソッド（フレーム(FrameImgScript)から通知）
	/// </summary>
	/// <param name="frameIndex">クリックされたフレームインデックス</param>
	public void ChildFrameImgClick(int frameIndex) {
		// フレームがクリックされたというフラグを立てる
		isChildFrameImgClicked = true;
		// クリックされたフレームインデックスを選択中にし，通知を行う．
		selectedIndex = frameIndex;
		CallSelectedFrameImgChanged ();
		objects.MotionData.ChangeSelectFrame (selectedIndex);
	}
	/// <summary>
	///  アニメーション再生終了メソッド
	/// </summary>
	/// <param name="endClipIndex">再生終了クリップインデックス</param>
	public void AnimationEnded(int endClipIndex) {
		// 再生終了した時点のインデックスを選択中に，通知する．
		selectedIndex = endClipIndex;
		CallSelectedFrameImgChanged (false);
		objects.MotionData.ChangeSelectFrame (selectedIndex);
	}
	/// <summary>
	///  アニメーション再生開始メソッド（イベント呼び出し）
	/// </summary>
	/// <param name="startClipIndex">Start clip index.</param>
	public void AnimationStarted(int startClipIndex) {
		// 現在選択中のフレーム背景画像を保存．選択フレームを変更．通知
		frameImgList [selectedIndex].GetComponent<FrameImgScript> ().SaveFrameImgTex ();
		selectedIndex = startClipIndex;
		CallSelectedFrameImgChanged (true);
	}
	/// <summary>
	///  再生中アニメーションクリップ変更メソッド（イベント呼び出し）
	/// </summary>
	/// <param name="changeClipIndex">変更クリップインデックス</param>
	public void AnimationClipChanged(int changeClipIndex) {
		// 選択中のフレームを変更．通知（アニメーション再生中であることも）
		selectedIndex = changeClipIndex;
		CallSelectedFrameImgChanged (true);
	}
	/// <summary>
	///  フレーム入れ替えメソッド
	/// </summary>
	/// <param name="srcIndex">Source index.</param>
	/// <param name="dstIndex">Dst index.</param>
	public void FrameImgReplace(int srcIndex, int dstIndex) {
		// 移動前と移動後のオブジェクトを取得．移動前フレームデータのインスタンス作成．
		FrameImgScript srcFrameImg = frameImgList [srcIndex].GetComponent<FrameImgScript>();
		FrameImgScript dstFrameImg = frameImgList [dstIndex].GetComponent<FrameImgScript>();
		Frame srcFrame = new Frame (objects.MotionData.FrameList [srcIndex]);
		// フレームデータの入れ替え
		objects.MotionData.FrameList [srcIndex] = objects.MotionData.FrameList [dstIndex];
		objects.MotionData.FrameList [dstIndex] = srcFrame;
		/*--  ここから入れ替えたフレームデータの情報を移動前フレームと移動後フレームに設定--*/
		// フレームとフレームデータの再関連付け
		dstFrameImg.ThisFrame = objects.MotionData.FrameList [dstIndex];
		srcFrameImg.ThisFrame = objects.MotionData.FrameList [srcIndex];
		// 遷移時間の再割り当て
		dstFrameImg.SliderTime.value = dstFrameImg.ThisFrame.TransitionTime;
		srcFrameImg.SliderTime.value = srcFrameImg.ThisFrame.TransitionTime;
		// 移動後のフレームを選択．各種通知．
		selectedIndex = dstIndex;
		CallSelectedFrameImgChanged (false);
		objects.MotionData.ChangeSelectFrame (selectedIndex);
	}

	/// <summary>
	///  選択フレーム変更通知メソッド
	/// </summary>
	/// <param name="isAnimating">If set to <c>true</c> : アニメーション再生中</param>
	private void CallSelectedFrameImgChanged(bool isAnimating = false) {
		// 全フレームに選択フレーム変更の通知を行う
		foreach (GameObject childFrameImg in frameImgList) {
			childFrameImg.GetComponent<FrameImgScript> ().SelectedFrameImgChanged (selectedIndex, isAnimating);
		}
	}
	/// <summary>
	///  新規フレーム作成メソッド
	/// </summary>
	/// <param name="createIndex">作成フレームインデックス</param>
	/// <param name="isFrameDataExist">If set to <c>true</c> : すでにフレームデータが存在する（新規にフレームデータを作成しない）</param>
	private void CreateNewFrameImg(int createIndex, bool isFrameDataExist = false) {
		// フレーム作成．リストに登録．
		frameImgList.Insert (createIndex, GameObject.Instantiate (frameImgPrefub));

		// 各フレームのインデックスを再設定（影響のあるフレームのみ）
		for (int i = createIndex; i < frameImgList.Count; i++)
			frameImgList [i].GetComponent<FrameImgScript> ().Index = i;
		// フレーム表示領域の再設定
		// Note...一度全フレームの親子関係を解除し，再度フレーム順に親子関係の設定を行う→新規フレームをインサートできる
		layoutArea.transform.DetachChildren ();
		foreach (GameObject frameImg in frameImgList) {
			frameImg.transform.SetParent (layoutArea.transform, false);
			frameImg.transform.localScale = new Vector3 (1f, 1f, 1f);			// ※スケールが崩れる場合があるので再設定
		}
		// すでにフレームデータがあるのであればそのフレームを選択．ないのであれば作成．
		if (isFrameDataExist == false)
			objects.MotionData.CreateNewFrame (createIndex, selectedIndex);
		else
			objects.MotionData.ChangeSelectFrame (createIndex);
		// フレームとフレームデータの再関連付け
		for (int i = 0; i < frameImgList.Count; i++)
			frameImgList [i].GetComponent<FrameImgScript> ().ThisFrame = objects.MotionData.FrameList [i];
		// 新規作成フレームの遷移時間を再設定
		FrameImgScript newFrameImg = frameImgList [createIndex].GetComponent<FrameImgScript> ();
		newFrameImg.SliderTime.value = newFrameImg.ThisFrame.TransitionTime;
		// 新規作成フレームをアクティブに（初期状態：非アクティブ）
		frameImgList [createIndex].SetActive (true);
		// 選択中のフレームインデックスを再設定．通知．
		selectedIndex = createIndex;
		CallSelectedFrameImgChanged ();
	}
	/// <summary>
	///  フレームの背景画像初期化メソッド（コルーチン呼び出し）
	///  Note...それぞれのフレームを一瞬表示し，背景画像を作成する．表示する必要があるのでコルーチン呼び出し．
	/// </summary>
	private IEnumerator FrameImgTexInitialize() {
		// 全フレーム（一番最初のフレームは最終的に選択されるので処理の必要なし）に対して背景画像を初期化
		for (int i = 1; i < frameImgList.Count; i++) {
			// 一瞬だけフレームを選択し，背景画像を作成する．(選択されて，その後選択が解除されたときに画像が保存される）
			selectedIndex = i;
			objects.MotionData.ChangeSelectFrame (i);
			CallSelectedFrameImgChanged (false);
			yield return null;			// 1フレーム待機
		}
		// 最終的に一番最初のフレームを選択
		selectedIndex = 0;
		objects.MotionData.ChangeSelectFrame (0);
		CallSelectedFrameImgChanged (false);
	}
}
