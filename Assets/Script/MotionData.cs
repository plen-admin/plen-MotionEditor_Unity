using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using PLEN;

public class MotionData : MonoBehaviour {

    /// <summary>
    /// 初期状態フレーム
    /// </summary>
    public static Frame defaultFrame;

    /// <summary>
    /// 現在選択中のフレームインデックス
    /// </summary>
    public int Index {
        get {
            return index;
        }
        set {
            index = value;
        }
    }
    /// <summary>
    /// 関節オブジェクトリスト
    /// (LeftShoulderP，...，RightFootR(18関節)がリスト化されている)
    /// </summary>
    public List<GameObject> ModelJointList {
        get {
            return modelJoints;
        }
    }
    public List<Frame> FrameList {
        get {
            return frameList;
        }
    }
    public int SlotNum {
        get {
            return slotNum;
		} set {
			slotNum = value;
		}
    }

    [SerializeField]
    private ObjectsController objects;

    // 関節オブジェクト類（これらにセットされているオブジェクトの角度を変更していく）（インスペクタで初期化）
    [SerializeField]
	private GameObject LeftShoulderP;
    [SerializeField]
    private GameObject LeftThighY;
    [SerializeField]
    private GameObject LeftShoulderR;
    [SerializeField]
    private GameObject LeftElbowR;
    [SerializeField]
    private GameObject LeftThighR;
    [SerializeField]
    private GameObject LeftThighP;
    [SerializeField]
    private GameObject LeftKneeP;
    [SerializeField]
    private GameObject LeftFootP;
    [SerializeField]
    private GameObject LeftFootR;
    [SerializeField]
    private GameObject RightShoulderP;
    [SerializeField]
    private GameObject RightThighY;
    [SerializeField]
    private GameObject RightShoulderR;
    [SerializeField]
    private GameObject RightElbowR;
    [SerializeField]
    private GameObject RightThighR;
    [SerializeField]
    private GameObject RightThighP;
    [SerializeField]
    private GameObject RightKneeP;
    [SerializeField]
    private GameObject RightFootP;
    [SerializeField]
    private GameObject RightFootR;
 
    private List<GameObject> modelJoints = new List<GameObject> ();
	private List<Frame> frameList =  new List<Frame>();
	private int slotNum;

    private int index;

    void Start () {
		Index = 0;
		// リストに関節オブジェクトを追加
		modelJoints.Add (LeftShoulderP);
		modelJoints.Add (LeftThighY);
		modelJoints.Add (LeftShoulderR);
		modelJoints.Add (LeftElbowR);
		modelJoints.Add (LeftThighR);
		modelJoints.Add (LeftThighP);
		modelJoints.Add (LeftKneeP);
		modelJoints.Add (LeftFootP);
		modelJoints.Add (LeftFootR);
		modelJoints.Add (RightShoulderP);
		modelJoints.Add (RightThighY);
		modelJoints.Add (RightShoulderR);
		modelJoints.Add (RightElbowR);
		modelJoints.Add (RightThighR);
		modelJoints.Add (RightThighP);
		modelJoints.Add (RightKneeP);
		modelJoints.Add (RightFootP);
		modelJoints.Add (RightFootR);
		// 初期フレームを作成．
		defaultFrame = new Frame (modelJoints);
		// 初期状態にモデルを調整．
		ModelAllJointRotation (defaultFrame);
		slotNum = 0;
	}

	void Update() {
        //nop
	}

	/// <summary>
	///  新規フレーム作成メソッド
	/// </summary>
	/// <param name="newFrameIndex">作成インデックス（0未満：最終フレームへ）</param>
	/// <param name="baseFrameIndex">ベースとなるフレームのインデックス（0未満：初期状態フレーム）</param>
	public void CreateNewFrame(int newFrameIndex = -1, int baseFrameIndex = -1) {
		Frame baseFrame;
		//  ベースとなるフレームを設定
		if (baseFrameIndex < 0 || frameList.Count == 0)
			baseFrame = defaultFrame;
		else
			baseFrame = frameList [baseFrameIndex];
		// フレーム作成．リストに追加
		if (newFrameIndex < 0) {
			frameList.Add (new Frame (baseFrame));
			Index = frameList.Count - 1;
		} else {
			frameList.Insert (newFrameIndex, new Frame (baseFrame));
			Index = newFrameIndex;
		}
	}
	/// <summary>
	///  選択中フレーム変更メソッド
	/// </summary>
	/// <param name="selectedIndex">選択フレームインデックス</param>
	public void ChangeSelectFrame(int selectedIndex) {
		// インデックス変更．モデル回転．
		Index = selectedIndex;
		ModelAllJointRotation (frameList [Index]);
	}
	/// <summary>
	///  フレーム初期化メソッド
	/// </summary>
	/// <param name="initIndex">初期化フレームインデックス</param>
	/// <param name="isTransitionTimeReset"> <c>true</c> : TransitionTimeも初期化 </param>
	public void FrameInitialize(int initIndex, bool isTransitionTimeReset = false) {
		// 
		for (int i = 0; i < defaultFrame.JointAngles.Length; i++)
			frameList [initIndex].JointAngles [i] = new JointAngle(defaultFrame.JointAngles [i]);

		if (isTransitionTimeReset == true)
			frameList [initIndex].TransitionTime = defaultFrame.TransitionTime;

		ModelAllJointRotation (frameList [initIndex]);
	}
	/// <summary>
	/// フレーム削除メソッド
	/// </summary>
	/// <param name="removeIndex">削除フレームインデックス</param>
	public void FrameRemove(int removeIndex) {
		frameList.RemoveAt (removeIndex);
		if (Index > frameList.Count - 1)
			Index = frameList.Count - 1;

	}
	/// <summary>
	///  モデルミラーメソッド（左右反転）
	/// </summary>
	public void ModelTurnOver() {
		// 現在の角度情報をすべて保存
		float[] anglesTmp = new float[modelJoints.Count];
		for (int i = 0; i < frameList [Index].JointAngles.Length; i++) {
			anglesTmp[i] = frameList [Index].JointAngles [i].Angle;
		}
		// 一度フレームを初期状態にし，左右の角度情報を入れ替える
		// Note...DATA_ROTATE_DIRECTIONで符号を調整しているため，必ずかける必要あり
		FrameInitialize (Index, false);
		int offset = modelJoints.Count / 2;
		// 左半身
		for (int i = 0; i < offset; i++) {
			frameList [Index].JointRotate (i, anglesTmp[i+offset] * Frame.DATA_ROTATE_DIRECTION[i+offset], false);
		}
		// 右半身
		for (int i = offset; i < modelJoints.Count; i++) {
			frameList [Index].JointRotate (i,anglesTmp[i-offset] * Frame.DATA_ROTATE_DIRECTION[i-offset], false);
		}
	}
	/// <summary>
	/// モデル半身コピーメソッド（右(左)半身を左(右)半身へコピー）
	/// </summary>
	/// <param name="isBaseTheRight">true...右半身を左半身へコピー</param>
	public void ModelMirror(bool isBaseTheRight) {
		int offset = modelJoints.Count / 2;

		for (int i = 0; i < modelJoints.Count / 2; i++) {
			if (isBaseTheRight == true) {
				frameList [Index].JointAngles [i] = new JointAngle(defaultFrame.JointAngles [i]);
				frameList [Index].JointRotate (i, frameList [Index].JointAngles [i + offset].Angle
					* Frame.DATA_ROTATE_DIRECTION [i + offset], false);
			}
			// 左半身を右半身へ
			else {
				frameList [Index].JointAngles [i + offset] = new JointAngle(defaultFrame.JointAngles [i + offset]);
				frameList [Index].JointRotate (i + offset, frameList [Index].JointAngles [i].Angle
					* Frame.DATA_ROTATE_DIRECTION [i], false);
			}
		}
	}

	/// <summary>
	/// モーションファイル（JSON）データ作成メソッド
	/// </summary>
	/// <returns>JSONモーションファイルデータ</returns>
	public string MotionJSONDataCreate(string name) {
		// 各種設定
		PLEN.JSON.Main jsonMain = new PLEN.JSON.Main ();
		jsonMain.name = name;
		jsonMain.slot = slotNum;
		foreach (Frame frame in frameList) {
			jsonMain.frames.Add(frame.FrameJSONDataCreate());
		}
		// JSONデータを返す(string)
		return JsonMapper.ToJson (jsonMain);
	}
	/// <summary>
	/// モーションファイル(JSON)読み込みメソッド
	/// </summary>
	/// <returns><c>true</c> : 読み込み成功 <c>false</c> 読み込み失敗</returns>
	/// <param name="jsonStr">JSONデータの文字列</param>
	public bool MotionJSONDataRead(string jsonStr) {
		try {
			// JSONファイル読み込み
			PLEN.JSON.Main jsonMain = LitJson.JsonMapper.ToObject<PLEN.JSON.Main> (jsonStr);
			// 読み込み失敗
			if (jsonMain == null)
				return false;
	
			/*-- ここから読み込んだJSONファイルをMotionDataに変換 --*/
			// 全フレームを初期化
			frameList.Clear ();
			// JSONファイルに記述されていたフレーム数ループ
			foreach (PLEN.JSON.Frame jsonFrame in jsonMain.frames) {
				// ベースフレームは初期状態フレームを使用（その後読み込んだangle値で回転させる）
				Frame frame = new Frame (defaultFrame);
				frame.TransitionTime = jsonFrame.transition_time_ms;
				// JSONファイルから読み込んだフレームから各関節の情報を読み取り，設定を行う
				foreach (PLEN.JSON.Output output in jsonFrame.outputs) {
					PLEN.JointName parseJointName;
					float angle;
					try {
						// JSONファイル中のdevice情報をJointName型に変換
						parseJointName = (PLEN.JointName)Enum.Parse(typeof(PLEN.JointName), output.device);
						// 関節角度読み込み（左半身は回転量を反転）
						angle = (float)output.value / 10;
						// 読み込んだ角度情報をもとに関節を回転
						frame.JointRotate(parseJointName, angle, true);
					} catch(Exception) {
						return false;
					}
				}
				// 読み込みが完了したフレームをリストに追加
				frameList.Add (frame);

			}
			// スロット番号更新
			objects.MenuController.InputFieldSlotUpdate(jsonMain.slot);
			return true;
		}catch(Exception ex) {
			Debug.LogError (ex.Message);
			return false;
		}
	}
	/// <summary>
	/// 全関節オブジェクト回転メソッド（オブジェクトの回転のみ）
	/// </summary>
	/// <param name="frame">ベースとなるFrame</param>
	private void ModelAllJointRotation(Frame frame) {
		for (int i = 0; i < modelJoints.Count; i++)
			modelJoints [i].transform.localEulerAngles = frame.JointAngles[i].EulerAngle;
	}
}

public class Frame {
    /// <summary>
    /// 関節配列
    /// </summary>
    public JointAngle[] JointAngles {
        get {
            return jointAngles;
        }
    }
    /// <summary>
    /// フレーム遷移時間
    /// </summary>
    public int TransitionTime {
        get {
            return transitionTime;
        } set {
            transitionTime = value;
        }
    }
	/// <summary>
	/// 関節オブジェクトリスト
	/// </summary>
	private List<GameObject> modelJointList;

    private JointAngle[] jointAngles;
    private int transitionTime = 1000;

    public static readonly int[] DATA_ROTATE_DIRECTION = { 1, 1, -1, 1, -1, 1, 1, -1 ,1, -1, -1, 1, -1, 1, -1, -1, 1, -1 }; 
	public static readonly int[] DISP_ROTATE_DIRECTION = { -1, -1, -1, 1, -1, -1, -1, 1, 1, 1, 1, 1, -1, 1, 1, 1, -1, -1 };
	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="jointList">関節オブジェクトリスト</param>
	public Frame(List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];
		// 関節情報セット
		// 左肩ピッチ，右肩ピッチに関しては回転方向をヨーに（モデルの座標的に）
		jointAngles [0]  =  new JointAngle (0 , new Vector3(0, 0, 1), jointList[0].transform.localEulerAngles);
		jointAngles [1]  =  new JointAngle (1 , new Vector3(0, 0, 1), jointList[1].transform.localEulerAngles);
		jointAngles [2]  =  new JointAngle (2 , new Vector3(1, 0, 0), jointList[2].transform.localEulerAngles);
		jointAngles [3]  =  new JointAngle (3 , new Vector3(1, 0, 0), jointList[3].transform.localEulerAngles);
		jointAngles [4]  =  new JointAngle (4 , new Vector3(1, 0, 0), jointList[4].transform.localEulerAngles);
		jointAngles [5]  =  new JointAngle (5 , new Vector3(0, 1, 0), jointList[5].transform.localEulerAngles);
		jointAngles [6]  =  new JointAngle (6 , new Vector3(0, 1, 0), jointList[6].transform.localEulerAngles);
		jointAngles [7]  =  new JointAngle (7 , new Vector3(0, 1, 0), jointList[7].transform.localEulerAngles);
		jointAngles [8]  =  new JointAngle (8 , new Vector3(1, 0, 0), jointList[8].transform.localEulerAngles);
		jointAngles [9]  =  new JointAngle (9 , new Vector3(0, 0, 1), jointList[9].transform.localEulerAngles);
		jointAngles [10] =  new JointAngle (10, new Vector3(0, 0, 1), jointList[10].transform.localEulerAngles);
		jointAngles [11] =  new JointAngle (11, new Vector3(1, 0, 0), jointList[11].transform.localEulerAngles);
		jointAngles [12] =  new JointAngle (12, new Vector3(1, 0, 0), jointList[12].transform.localEulerAngles);
		jointAngles [13] =  new JointAngle (13, new Vector3(1, 0, 0), jointList[13].transform.localEulerAngles);
		jointAngles [14] =  new JointAngle (14, new Vector3(0, 1, 0), jointList[14].transform.localEulerAngles);
		jointAngles [15] =  new JointAngle (15, new Vector3(0, 1, 0), jointList[15].transform.localEulerAngles);
		jointAngles [16] =  new JointAngle (16, new Vector3(0, 1, 0), jointList[16].transform.localEulerAngles);
		jointAngles [17] =  new JointAngle (17, new Vector3(1, 0, 0), jointList[17].transform.localEulerAngles);
		modelJointList = jointList;
	}
	/// <summary>
	/// コンストラクタ（ベースフレームあり）
	/// </summary>
	/// <param name="baseFrame">ベースとなるフレームBase frame.</param>
	public Frame(Frame baseFrame) {
		jointAngles = new JointAngle[baseFrame.modelJointList.Count];
		// 
		for (int i = 0; i < jointAngles.Length; i++)
			jointAngles [i] = new JointAngle (baseFrame.jointAngles [i]);
		modelJointList = baseFrame.modelJointList;
		transitionTime = baseFrame.TransitionTime;
	}

	public void JointRotate(PLEN.JointName jointName, float angle, bool isMotionDataRead = false) {
		JointRotate ((int)jointName, angle, isMotionDataRead);
	}
	/// <summary>
	///  関節回転メソッド（angle値も更新）
	/// </summary>
	/// <param name="index">回転関節インデックス</param>
	/// <param name="angle">回転角度</param>
	/// <param name="isMotionDataRead"><c>true</c> : モーション読み込み時（回転補正なし）</param>
	public void JointRotate(int index, float angle, bool isMotionDataRead = false) {
//		float dispAngle = angle;
		const int ADJ_DIRECTION = -1;

		// angle値更新
		if (isMotionDataRead == false) {
			jointAngles [index].Angle += angle * DATA_ROTATE_DIRECTION [index];
		} else {
			jointAngles [index].Angle = angle;
		}
		// 回転角度計算（初期状態の回転量に加算する形で）
		jointAngles[index].EulerAngle = 
			MotionData.defaultFrame.jointAngles[index].EulerAngle + 
			jointAngles [index].Angle * ADJ_DIRECTION * jointAngles [index].CoordUnitVec;
		// 回転
		modelJointList [index].transform.localEulerAngles = jointAngles [index].EulerAngle;
		
	}
	/// <summary>
	/// モーションファイル(JSON)用データ生成メソッド
	/// </summary>
	/// <returns>JSON用データ</returns>
	public PLEN.JSON.Frame FrameJSONDataCreate() {
		// インスタンス作成．遷移時間設定．
		PLEN.JSON.Frame jsonFrame = new PLEN.JSON.Frame ();
		jsonFrame.transition_time_ms = TransitionTime;
		// 各関節データをJSONデータに変換．
		foreach (JointAngle jointAngle in jointAngles) {
			// 各種情報をJSONデータに変換
			PLEN.JSON.Output jsonOutput = new PLEN.JSON.Output ();
			jsonOutput.device = ((PLEN.JointName)(jointAngle.JointIndex)).ToString();
			jsonOutput.value = (int)(jointAngle.Angle * 10);
			jsonFrame.outputs.Add (jsonOutput);
		}
		return jsonFrame;
	}
	/// <summary>
	/// フレームインスタンスクローンメソッド
	/// </summary>
	public Frame Clone() {
		return (Frame)MemberwiseClone ();
	}
}

public class JointAngle {
    /// <summary>
    ///  関節番号
    /// </summary>
    public int JointIndex {
        get {
            return jointIndex;
        }
    }
    public Vector3 CoordUnitVec {
        get {
            return coordUnitVec;
        }
    }
    /// <summary>
    /// 回転オイラー角
    /// </summary>
    public Vector3 EulerAngle {
        get {
            return eulerAngle;
        } set {
            eulerAngle = value;
        }
    }
	/// <summary>
	/// 関節の初期位置からの角度（-180＜Angle＜180）
	/// </summary>
	/// <value>The angle.</value>
	public float Angle {
        get {
            return angle;
        }
        set {
            angle = value;
            while (angle > 180)
                angle -= 360;
            while (angle < -180)
                angle += 360;
        }
    }
    private int jointIndex;
    /// <summary>
    /// 回転方向単位ベクトル
    /// </summary>
    private Vector3 coordUnitVec;
    private Vector3 eulerAngle;
    private float angle;
	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="jointIndex">関節番号</param>
	/// <param name="coordUnitVec">回転方向</param>
	/// <param name="eulerAngle">初期オイラー角</param>
	public JointAngle(int jointIndex, Vector3 coordUnitVec, Vector3 eulerAngle) {
		this.jointIndex = jointIndex;
		this.coordUnitVec = coordUnitVec;
		this.eulerAngle = eulerAngle;
        angle = 0;
	}
	/// <summary>
	/// コンストラクタ（ベースとなるJointAngleあり）
	/// </summary>
	/// <param name="baseJointAngle">ベースとなるJointAngle</param>
	public JointAngle(JointAngle baseJointAngle) {
		jointIndex = baseJointAngle.jointIndex;
		coordUnitVec = baseJointAngle.coordUnitVec;
		eulerAngle = baseJointAngle.eulerAngle;
		Angle = baseJointAngle.Angle;
	}
}
