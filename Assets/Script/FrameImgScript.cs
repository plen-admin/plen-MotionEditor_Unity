using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;


public class FrameImgScript : MonoBehaviour {
	public ObjectsController objectsController;
	public Frame thisFrame;
	public Text labelFrame;
	public Slider sliderTime;
	public InputField inputFieldTime;
	public PanelFramesScript parentPanelFrames;
	public RenderTexture frameImgTex;
	public GameObject btnRemoveObject;
	private Collider2D thisCollider;
	private Button[] btnArray;
	private int _index;
	private string framingSavePath;


	private bool isActive;
	private bool isReplace;
	private bool isWait;
	public bool isFadeOuted;

	public int index {
		get {
			return _index; 
		}
		set {
			_index = value;
			labelFrame.text = _index.ToString ();
		}
	}

	void Awake() {
		framingSavePath = ObjectsController.tmpFilePath + "Frames/";
		if (!Directory.Exists (framingSavePath)) {
			Directory.CreateDirectory (framingSavePath);
		}
	}

	// Use this for initialization
	void Start () {
		isActive = true;
		isReplace = false;
		isWait = false;
		thisCollider = this.GetComponent<Collider2D> ();
		btnArray = this.GetComponentsInChildren<Button> ();
		objectsController = GameObject.Find ("ObjectsController").GetComponent<ObjectsController> ();
		parentPanelFrames = GameObject.Find ("PanelFrames").GetComponent<PanelFramesScript> ();


	}
	
	// Update is called once per frame
	void Update () {
		if (objectsController.isFrameRelationWaitRequest == true) {
			if (isWait == false) {
				foreach (Button btn in btnArray)
					btn.enabled = false;
				thisCollider.enabled = false;
				isWait = true;
			}
		} else if(isWait == true) {
			foreach (Button btn in btnArray)
				btn.enabled = true;
			thisCollider.enabled = true;
			isWait = false;
		}


		if (objectsController.isAllObjectWaitRequest == false) {
			if (Input.GetMouseButtonDown (0)) {
				if (this.GetComponent<Collider2D> ().OverlapPoint (Input.mousePosition)
				    && !btnRemoveObject.GetComponent<Collider2D> ().OverlapPoint (Input.mousePosition)) {

					parentPanelFrames.ChildFrameImgClick (index);
				}
			}
		}
	}
	public void BtnBack_Click() {
		if (index > 0) {
			isReplace = true;
			SaveFrameImgTex ();
			FrameImgTexPngReplace (index, index - 1);
			objectsController.panelFrames.FrameImgReplace (index, index - 1);
			isReplace = false;
		}
	}

	public void BtnNext_Click() {
		if (index < objectsController.panelFrames.FrameCount - 1) {
			isReplace = true;
			SaveFrameImgTex ();
			FrameImgTexPngReplace (index, index + 1);
			objectsController.panelFrames.FrameImgReplace (index, index + 1);
			isReplace = false;
		}
	}


	public void BtnRemove_Click() {
		if (objectsController.isAllObjectWaitRequest == false) {
			PanelFramesScript panelFrames = parentPanelFrames.GetComponent<PanelFramesScript> ();
			// フレームが2つ以上ある場合，自フレームを削除する
			if (panelFrames.FrameCount > 1) {
				panelFrames.isFramePlayingDestroyAnimation = true;
				this.GetComponent<Animator> ().SetBool ("isDestroy", true);
			} 
			// 1フレームのみの場合，そのフレームを初期化する 
			else {
				objectsController.motionData.FrameInitialize (0);
				parentPanelFrames.ChildFrameImgClick (0);

			}
		}
	}

	private void EndDestroyAnimation() {
		parentPanelFrames.ChildFrameImgDestroy (index);
	}

	public void SelectedFrameImgChanged(int selectedIndex, bool isAnimating) {
		if (selectedIndex == index) {
			RawImage thisRawImg = this.GetComponent<RawImage> ();
			thisRawImg.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			if (isAnimating == false) {
				thisRawImg.texture = frameImgTex;
				isActive = true;
			} else {
				thisRawImg.texture = ReadFrameImgTexPng ();
				isActive = false;
			}
		} else {
			RawImage thisRawImg = this.GetComponent<RawImage> ();
			thisRawImg.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
			if (isReplace == true) {
				thisRawImg.texture = ReadFrameImgTexPng ();
			} else if (isActive == true) {
				if (isAnimating == false) {
					SaveFrameImgTex ();
				}
				thisRawImg.texture = ReadFrameImgTexPng ();
			}
			isActive = false;
			
		}
	}

	public void SliderTimeUpdate() {
		inputFieldTime.text = sliderTime.value.ToString();
		thisFrame.transitionTime = (int)sliderTime.value;
	}

	public void InputFieldTimeUpdate() {
		int inputNum;

		if (int.TryParse (inputFieldTime.text, out inputNum)) {
			if (inputNum < sliderTime.minValue) {
				inputNum = (int)sliderTime.minValue;
				inputFieldTime.text = inputNum.ToString ();
			} else if (inputNum > sliderTime.maxValue) {
				inputNum = (int)sliderTime.maxValue;
				inputFieldTime.text = inputNum.ToString ();
			}
			sliderTime.value = inputNum;
			thisFrame.transitionTime = inputNum;
		} else {
			inputFieldTime.text = sliderTime.value.ToString ();
		}
	}

	public FrameImgScript Clone() {
		return (FrameImgScript)MemberwiseClone ();
	}

	public void SaveFrameImgTex() {
		// モデル画像保存
		RenderTexture renderTexRT = RenderTexture.active;
		RenderTexture.active = frameImgTex;
		Texture2D saveTex = new Texture2D(frameImgTex.width, frameImgTex.height);
		saveTex.ReadPixels (new Rect (0, 0, frameImgTex.width, frameImgTex.height), 0, 0);
		var savePngBytes = saveTex.EncodeToPNG ();
		System.IO.File.WriteAllBytes (framingSavePath + index.ToString() + ".png", savePngBytes);
		Destroy (saveTex);
		RenderTexture.active = renderTexRT;
	}


	private Texture ReadFrameImgTexPng() {
		if (!File.Exists (framingSavePath + index.ToString () + ".png"))
			return null;

		FileStream readStream = new FileStream (framingSavePath + index.ToString () + ".png", FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader (readStream);
		var binaryPng = binaryReader.ReadBytes ((int)binaryReader.BaseStream.Length);
		Texture2D readPngTex = new Texture2D (frameImgTex.width, frameImgTex.height);
		readPngTex.LoadImage (binaryPng);
		readStream.Close ();
		return readPngTex;
	}

	private void FrameImgTexPngReplace(int srcIndex, int detIndex) {
		string srcPath = framingSavePath + srcIndex.ToString () + ".png";
		string dstPath = framingSavePath + detIndex.ToString () + ".png";
		string tmpPath = framingSavePath + "tmp";

		if (!File.Exists (srcPath) || !File.Exists (dstPath))
			return;

		while (File.Exists (tmpPath + ".png")) {
			tmpPath += "0";
		}
		tmpPath += ".png";

		File.Move (srcPath, tmpPath);
		File.Move (dstPath, srcPath);
		File.Move (tmpPath, dstPath);
	}
}
