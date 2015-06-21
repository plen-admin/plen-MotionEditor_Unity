using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;


public class FrameImgScript : MonoBehaviour {
	public Frame thisFrame;
	public Text labelFrame;
	public Slider sliderTime;
//	public Text labelTime;
	public InputField inputFieldTime;
	public PanelFramesScript parentPanelFrames;
	public RenderTexture frameImgTex;
	public GameObject btnRemoveObject;
	private int _index;
	private const string FRAMEIMG_SAVE_PATH = "./tmp/frameImg/";

	private bool isActive;
	public bool isFadeOuted;

	public int index {
		get {
			return _index; 
		}
		set {
			_index = value;
			labelFrame.text = _index.ToString ();
			if (_index == 0) {
				inputFieldTime.enabled = false;
			} else {
				inputFieldTime.enabled = true;
			}
		}
	}

	// Use this for initialization
	void Start () {
		isActive = true;
		parentPanelFrames = GameObject.Find ("PanelFrames").GetComponent<PanelFramesScript> ();
		if (!Directory.Exists (FRAMEIMG_SAVE_PATH)) {
			Directory.CreateDirectory (FRAMEIMG_SAVE_PATH);
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			if(this.GetComponent<Collider2D> ().OverlapPoint(Input.mousePosition)
				&& !btnRemoveObject.GetComponent<Collider2D>().OverlapPoint(Input.mousePosition)){

				parentPanelFrames.ChildFrameImgClick (index);
			}
		}
	}

	public void BtnRemove_Click() {

		if (parentPanelFrames.GetComponent<PanelFramesScript> ().FrameCount > 1) {

			parentPanelFrames.GetComponent<PanelFramesScript> ().isFrameImgFadeOut = true;
			this.GetComponent<Animator> ().SetBool ("isFadeOut", true);



		}
	}

	private void EndFadeOut() {
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
			}
		} else {
			RawImage thisRawImg = this.GetComponent<RawImage> ();
			thisRawImg.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
			if (isActive == true) {
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

	public void SaveFrameImgTex() {
		// モデル画像保存
		RenderTexture renderTexRT = RenderTexture.active;
		RenderTexture.active = frameImgTex;
		Texture2D saveTex = new Texture2D(frameImgTex.width, frameImgTex.height);
		saveTex.ReadPixels (new Rect (0, 0, frameImgTex.width, frameImgTex.height), 0, 0);
		var savePngBytes = saveTex.EncodeToPNG ();
		System.IO.File.WriteAllBytes (FRAMEIMG_SAVE_PATH + index.ToString() + ".png", savePngBytes);
		Destroy (saveTex);
		RenderTexture.active = renderTexRT;
	}
	private Texture ReadFrameImgTexPng() {
		if (!File.Exists (FRAMEIMG_SAVE_PATH + index.ToString () + ".png"))
			return null;

		FileStream readStream = new FileStream (FRAMEIMG_SAVE_PATH + index.ToString () + ".png", FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader (readStream);
		var binaryPng = binaryReader.ReadBytes ((int)binaryReader.BaseStream.Length);
		Texture2D readPngTex = new Texture2D (frameImgTex.width, frameImgTex.height);
		readPngTex.LoadImage (binaryPng);
		readStream.Close ();
		return readPngTex;
	}
}
