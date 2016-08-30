using UnityEngine;
using System.Collections;

public interface IObjects {
    void Initialize(ObjectsController controller);
}

public class ObjectsController : MonoBehaviour {

    public MotionData MotionData {
        get { return motionData;
        }
    }
    public MenuGUI MenuController {
        get {
            return menuController;
        }
    }
    public ModelViewCamera ModelViewController {
        get {
            return modelViewController;
        }
    }
    public DialogScript Dialog {
        get {
            return dialog;
        }
    }
	public FileChooser FileChooser {
        get {
            return fileChooser;
        }
    }
    public MotionInstall MotionInstall {
        get {
            return motionInstall;
        }
    }
    public PLENModelAnimation PlenAnimation {
        get {
            return plenAnimation;
        }
    }
    public PanelFramesScript PanelFrames {
        get {
            return panelFrames;
        }
    }

    public RectTransform DispCanvasRectTransform {
        get {
            return dispCanvasRectTransform;
        }
    }
	public bool IsAnimationPlaying {
		get { 
			return isAnimationPlaying;
		}
		set {
			isAnimationPlaying = value;
		}
	}
	public bool IsAllObjectWaitRequest {
		get { 
			return (isAllObjectWaitRequest | IsDialogShowing); 
		}
		set {
			isAllObjectWaitRequest = value;
		}
	}
	public bool IsDialogShowing {
		get {
			return isDialogShowing; 
		}
		set {
			isDialogShowing = value;
		}
	}
	public bool IsFrameRelationWaitRequest {
		get {
			return (IsAllObjectWaitRequest | IsAnimationPlaying | IsAnimationPlaying | isFrameRelationWaitRequest);
		}
		set {
			isFrameRelationWaitRequest = value;
		}
	}

	public  static string TmpFilePath {
		get {
            return tmpFilePath;
        }
	}

	public static string ExternalFilePath {
		get {
            return externalFilePath;
        }
	}
	public static string SampleMotionDirPath {
		get {
            return sampleMotionDirPath;
        }
	}

    [SerializeField]
    private MotionData motionData;
    [SerializeField]
    private MenuGUI menuController;
    [SerializeField]
    private ModelViewCamera modelViewController;
    [SerializeField]
    private DialogScript dialog;
    [SerializeField]
    private FileChooser fileChooser;
    [SerializeField]
    private MotionInstall motionInstall;
    [SerializeField]
    private PLENModelAnimation plenAnimation;
    [SerializeField]
    private PanelFramesScript panelFrames;
    [SerializeField]
    private RectTransform dispCanvasRectTransform;

    private static string tmpFilePath;
	private static string externalFilePath;
	private static string sampleMotionDirPath;
	private bool isAnimationPlaying;
	private bool isFrameRelationWaitRequest;
	private bool isAllObjectWaitRequest;
	private bool isDialogShowing;

	void Awake() {
		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			tmpFilePath = Application.dataPath + "/../tmp/";
			externalFilePath = Application.dataPath + "/../Plugins/Windows/";
			sampleMotionDirPath = Application.dataPath + "/../Plugins/SampleMotion/";
		} else if (Application.platform == RuntimePlatform.OSXPlayer) {
			tmpFilePath = Application.dataPath + "/../../tmp/";
			externalFilePath = Application.dataPath + "/../../Plugins/OSX/";
			sampleMotionDirPath = Application.dataPath + "/../../Plugins/SampleMotion/";
		} else {
			tmpFilePath = Application.dataPath + "/tmp/";
			externalFilePath = Application.dataPath + "/Plugins/";
			sampleMotionDirPath = Application.dataPath + "/Plugins/SampleMotion/";
		}
        if (PlenAnimation != null) {
            PlenAnimation.Initialize(this);
            PlenAnimation.AnimationStarted += (_) => {
                IsAnimationPlaying = true;
            };
            PlenAnimation.AnimationEnded += (_) => {
                IsAnimationPlaying = false;
            };
        }
        if (PanelFrames != null) {
            PanelFrames.Initialize(this);
        }
        if (MotionData != null) {
            MotionData.Initialize(this);
        }
        if (ModelViewController != null) {
            ModelViewController.Initialize(this);
        }
        if (MenuController != null) {
            MenuController.Initialize(this);
        }
        if (FileChooser != null) {
            FileChooser.Initialize(this);
        }
        if (Dialog != null) {
            Dialog.Initialize(this);
        }
    }

	// Use this for initialization
	void Start () {
		motionInstall = this.GetComponent<MotionInstall> ();
	}
	
	// Update is called once per frame
	void Update () {
	    //nop
	}
}
