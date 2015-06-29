using UnityEngine;
using System.Collections;

public class ObjectsController : MonoBehaviour {
	public MotionData motionData;
	public MenuGUI menuController;
	public DialogScript dialog;
	public FileChooser fileChooser;
	public PLENModelAnimation plenAnimation;
	public PanelFramesScript panelFrames;

	public bool isAnimationPlaying {
		get { 
			return _isAnimationPlaying;
		}
		set {
			_isAnimationPlaying = value;
		}
	}
	public bool isAllObjectWaitRequest {
		get { 
			return _isAllObjectWaitRequest; 
		}
		set {
			_isAllObjectWaitRequest = value;
		}
	}
	public bool isDialogShowing {
		get {
			return _isDialogShowing; 
		}
		set {
			_isDialogShowing = value;
			if (value == true)
				_isAllObjectWaitRequest = true;
			else
				_isAllObjectWaitRequest = false;
		}
	}
	public bool isFrameRelationWaitRequest {
		get {
			return (_isAllObjectWaitRequest | _isAnimationPlaying | _isAnimationPlaying | _isFrameRelationWaitRequest);
		}
		set {
			_isFrameRelationWaitRequest = value;
		}
	}

	private bool _isAnimationPlaying;
	private bool _isFrameRelationWaitRequest;
	private bool _isAllObjectWaitRequest;
	private bool _isDialogShowing;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
