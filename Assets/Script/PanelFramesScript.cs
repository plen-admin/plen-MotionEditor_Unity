using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PanelFramesScript : MonoBehaviour {
	/// <summary>
	/// frameImgのプレハブ（インスペクタで初期化）
	/// </summary>
	public GameObject frameImgPrefub;
	/// <summary>
	/// モーションデータを管理するインスタンス（インスペクタで初期化）
	/// </summary>
	public MotionData motionData;
	/// <summary>
	/// モーションデータを管理するインスタンス（インスペクタで初期化）
	/// </summary>
	public GameObject layoutArea;
	public float colliderPadding;
	public Scrollbar scrollBarH;
	public bool isFrameImgFadeOut = false;
	/// <summary>
	/// frameImgリスト
	/// </summary>
	private List<GameObject> frameImgList = new List<GameObject> ();
	/// <summary>
	/// 現在選択中のframeImgのindex
	/// </summary>
	private int selectedIndex;
	/// <summary>
	/// frameImg選択判定フラグ
	/// </summary>
	private bool isChildFrameImgClicked = false;
	private bool isRequestToMoveScrollBar = false;
	public bool isAnimationPlaying = false;

	private float layoutAreaWidth;
	private BoxCollider2D layoutAreaCollider;
	private RectTransform thisRectTransform;
	private RectTransform layoutAreaRectTransfrom;

	private const float DEFAULT_SLIDER_VALUE = 1000;

	public int FrameCount {
		get { return frameImgList.Count; }
	}
		

	// Use this for initialization
	void Start () {
		// 初期状態としてあらかじめ一つframeImgを作成
/*		frameImgList.Add (GameObject.Instantiate (frameImgPrefub));
		FrameImgScript frameImg = frameImgList [0].GetComponent<FrameImgScript> ();
		frameImg.index = 0;
		frameImgList [0].transform.SetParent (layoutArea.transform, false);
		frameImgList [0].SetActive (true);
*/
		thisRectTransform = this.GetComponent<RectTransform> ();
		layoutAreaRectTransfrom = layoutArea.GetComponent<RectTransform> ();
		selectedIndex = 0;
		CreateNewFrameImg (0, DEFAULT_SLIDER_VALUE);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp (0) && isFrameImgFadeOut == false && isAnimationPlaying == false) {
			if (isChildFrameImgClicked == false && this.GetComponent<Collider2D>().OverlapPoint(Input.mousePosition)) {
				int createIndex = -1;
				RaycastHit2D[] hits = Physics2D.RaycastAll (Input.mousePosition, -Vector2.up);

				for (int i = 0; i < hits.Length; i++) {
					if (hits [i].collider == layoutAreaCollider) {
						float clickPosX = hits[i].point.x * (thisRectTransform.rect.width / Screen.width);
						float normalizedPos = this.GetComponent<ScrollRect>().horizontalNormalizedPosition;
						clickPosX += normalizedPos * (layoutAreaWidth - thisRectTransform.rect.width + colliderPadding);

						for (int j = 0; j < frameImgList.Count; j++) {
							if (clickPosX < (j + 1) * layoutAreaWidth / frameImgList.Count) {
								createIndex = j;
								break;
							}
						}
					}
				}
				if(createIndex < 0){
					createIndex = frameImgList.Count;
					isRequestToMoveScrollBar = true;
				}

				CreateNewFrameImg (createIndex, frameImgList[selectedIndex].GetComponent<FrameImgScript>().sliderTime.value);


				
			} else {
				isChildFrameImgClicked = false;
			}
		}

	}

	void LateUpdate() {
		// FrameImgLayAoutAreaのColliderサイズを調整
		// Note...Content Size FilterによりlayoutAreaのWidthが可変的なため，Colliderのサイズ調整が必要．
		//        かつサイズ変更のタイミングが不明なため
		layoutAreaWidth = layoutAreaRectTransfrom.rect.width - colliderPadding;
		layoutAreaCollider = layoutArea.GetComponent<BoxCollider2D> ();
		layoutAreaCollider.size = new Vector2 (layoutAreaWidth, layoutAreaCollider.size.y);
		layoutAreaCollider.offset = new Vector2 (layoutAreaWidth / 2, 0f);

	}

	public void ScrollBarH_OnValueChanged() {
		if (isRequestToMoveScrollBar == true) {
			scrollBarH.value = 1;
			isRequestToMoveScrollBar = false;
		}

	}

	public void FrameGoBack() {
		if (selectedIndex > 0) {
			selectedIndex--;
			CallSelectedFrameImgChanged ();
			motionData.ChangeSelectFrame (selectedIndex);
		}
	}

	public void FrameGoNext() {
		if (selectedIndex < frameImgList.Count - 1) {
			selectedIndex++;
			CallSelectedFrameImgChanged ();
			motionData.ChangeSelectFrame (selectedIndex);
		}
	}

	public void ChildFrameImgDestroy(int destroyIndex) {
		isChildFrameImgClicked = true;
		isFrameImgFadeOut = false;
		if (frameImgList.Count == 1)
			return;
		Destroy (frameImgList [destroyIndex]);
		frameImgList.RemoveAt (destroyIndex);
		// 各フレームのインデックスを再設定（影響のあるフレームのみ）
		for (int i = destroyIndex; i < frameImgList.Count; i++)
			frameImgList [i].GetComponent<FrameImgScript> ().index = i;
		motionData.FrameRemove (destroyIndex);

		if (selectedIndex > frameImgList.Count - 1) {
			selectedIndex = frameImgList.Count - 1;
			CallSelectedFrameImgChanged ();
			motionData.ChangeSelectFrame (selectedIndex);
		}
		else if (selectedIndex == destroyIndex) {
			CallSelectedFrameImgChanged ();
			motionData.ChangeSelectFrame (selectedIndex);
		}
	}

	public void ChildFrameImgClick(int frameIndex) {
		selectedIndex = frameIndex;
		isChildFrameImgClicked = true;
		CallSelectedFrameImgChanged ();
		motionData.ChangeSelectFrame (selectedIndex);
	}

	public void PlayAnimationEnded(int endClipIndex) {
		isAnimationPlaying = false;
		selectedIndex = endClipIndex;
		CallSelectedFrameImgChanged (false);
		motionData.ChangeSelectFrame (selectedIndex);
	}

	public void PlayAnimationStarted(int startClipIndex) {
		isAnimationPlaying = true;
		frameImgList [selectedIndex].GetComponent<FrameImgScript> ().SaveFrameImgTex ();
		selectedIndex = startClipIndex;
		CallSelectedFrameImgChanged (true);
	}

	public void AnimationClipChanged(int changeClipIndex) {
		selectedIndex = changeClipIndex;
		CallSelectedFrameImgChanged (true);
	}

	private void CallSelectedFrameImgChanged(bool isAnimating = false) {
		Debug.Log ("isAnimating : " + isAnimating.ToString ());
		foreach (GameObject childFrameImg in frameImgList) {
			childFrameImg.GetComponent<FrameImgScript> ().SelectedFrameImgChanged (selectedIndex, isAnimating);
		}
	}
	private void CreateNewFrameImg(int createIndex, float sliderValue) {

		frameImgList.Insert (createIndex, GameObject.Instantiate (frameImgPrefub));

		// 各フレームのインデックスを再設定（影響のあるフレームのみ）
		for (int i = createIndex; i < frameImgList.Count; i++)
			frameImgList [i].GetComponent<FrameImgScript> ().index = i;
		

		frameImgList [createIndex].GetComponent<FrameImgScript> ().index = createIndex;

		layoutArea.transform.DetachChildren ();
		foreach (GameObject frameImg in frameImgList) {
			frameImg.transform.SetParent (layoutArea.transform, false);
			frameImg.transform.localScale = new Vector3 (1f, 1f, 1f);
		}
		motionData.CreateNewFrame (createIndex, selectedIndex);

		for (int i = 0; i < frameImgList.Count; i++)
			frameImgList [i].GetComponent<FrameImgScript> ().thisFrame = motionData.frameList [i];

		frameImgList [createIndex].GetComponent<FrameImgScript> ().sliderTime.value = sliderValue;
		frameImgList [createIndex].SetActive (true);

		selectedIndex = createIndex;
		CallSelectedFrameImgChanged ();
	}
}
