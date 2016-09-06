using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;


public class FrameImgScript : MonoBehaviour {
    public Frame ThisFrame {
        get {
            return thisFrame;
        } set {
            thisFrame = value;
        }
    }
    public Slider SliderTime {
        get {
            return sliderTime;
        }
    }
    public int Index {
        get {
            return index;
        }
        set {
            index = value;
            labelFrame.text = index.ToString();
        }
    }
    private ObjectsController objects;
    [SerializeField]
    private Frame thisFrame;
    [SerializeField]
    private Text labelFrame;
    [SerializeField]
    private Slider sliderTime;
    [SerializeField]
    private InputField inputFieldTime;
    [SerializeField]
    private PanelFramesScript parentPanelFrames;
    [SerializeField]
    private RenderTexture frameImgTex;
    [SerializeField]
    private GameObject btnRemoveObject;
	private Collider2D thisCollider;
	private Button[] btnArray;
	private int index;
	private string framingSavePath;

	private bool isActive;
	private bool isReplace;
	private bool isWait;

	void Awake() {
		framingSavePath = ObjectsController.TmpFilePath + "Frames/";
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
		objects = GameObject.Find ("ObjectsController").GetComponent<ObjectsController> ();
		parentPanelFrames = GameObject.Find ("PanelFrames").GetComponent<PanelFramesScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (objects.IsFrameRelationWaitRequest == true) {
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

		if (objects.IsAllObjectWaitRequest == false) {
			if (Input.GetMouseButtonDown (0)) {
				if (this.GetComponent<Collider2D> ().OverlapPoint (Input.mousePosition)
				    && !btnRemoveObject.GetComponent<Collider2D> ().OverlapPoint (Input.mousePosition)) {

					parentPanelFrames.ChildFrameImgClick (Index);
				}
			}
		}
	}
	public void BtnBack_Click() {
		if (Index > 0) {
			isReplace = true;
			SaveFrameImgTex ();
			FrameImgTexPngReplace (Index, Index - 1);
			objects.PanelFrames.FrameImgReplace (Index, Index - 1);
			isReplace = false;
		}
	}

	public void BtnNext_Click() {
		if (Index < objects.PanelFrames.FrameCount - 1) {
			isReplace = true;
			SaveFrameImgTex ();
			FrameImgTexPngReplace (Index, Index + 1);
			objects.PanelFrames.FrameImgReplace (Index, Index + 1);
			isReplace = false;
		}
	}


	public void BtnRemove_Click() {
		if (objects.IsAllObjectWaitRequest == false) {
			PanelFramesScript panelFrames = parentPanelFrames.GetComponent<PanelFramesScript> ();
			// フレームが2つ以上ある場合，自フレームを削除する
			if (panelFrames.FrameCount > 1) {
				panelFrames.IsFramePlayingDestroyAnimation = true;
				this.GetComponent<Animator> ().SetBool ("isDestroy", true);
			} 
			// 1フレームのみの場合，そのフレームを初期化する 
			else {
				objects.MotionData.FrameInitialize (0);
				parentPanelFrames.ChildFrameImgClick (0);

			}
		}
	}

	private void EndDestroyAnimation() {
		parentPanelFrames.ChildFrameImgDestroy (Index);
	}

	public void SelectedFrameImgChanged(int selectedIndex, bool isAnimating) {
		if (selectedIndex == Index) {
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
		thisFrame.TransitionTime = (int)sliderTime.value;
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
			thisFrame.TransitionTime = inputNum;
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
		System.IO.File.WriteAllBytes (framingSavePath + Index.ToString() + ".png", savePngBytes);
		Destroy (saveTex);
		RenderTexture.active = renderTexRT;
	}


	private Texture ReadFrameImgTexPng() {
		if (!File.Exists (framingSavePath + Index.ToString () + ".png"))
			return null;

		FileStream readStream = new FileStream (framingSavePath + Index.ToString () + ".png", FileMode.Open, FileAccess.Read);
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
