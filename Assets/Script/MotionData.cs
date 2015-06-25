using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using PLEN;

public class MotionData : MonoBehaviour {
	public List<Frame> frameList =  new List<Frame>();

	// 関節オブジェクト類（これらにセットされているオブジェクトの角度を変更していく）（インスペクタで初期化）
	public GameObject LeftShoulderP;
	public GameObject LeftThighY;
	public GameObject LeftShoulderR;
	public GameObject LeftElbowR;
	public GameObject LeftThighR;
	public GameObject LeftThighP;
	public GameObject LeftKneeP;
	public GameObject LeftFootP;
	public GameObject LeftFootR;
	public GameObject RightShoulderP;
	public GameObject RightThighY;
	public GameObject RightShoulderR;
	public GameObject RightElbowR;
	public GameObject RightThighR;
	public GameObject RightThighP;
	public GameObject RightKneeP;
	public GameObject RightFootP;
	public GameObject RightFootR;
	/// <summary>
	/// 現在選択中のフレームインデックス
	/// </summary>
	public int index;
	/// <summary>
	/// 初期状態フレーム（読み取り専用）
	/// </summary>
	public Frame DefaultFrame {
		get { return defaultFrame; }
	}
	/// <summary>
	/// 関節オブジェクトリスト（インスペクタで初期化）
	/// (LeftShoulderP，...，RightFootR(18関節)がリスト化されている)
	/// </summary>
	public List<GameObject> modelJointList = new List<GameObject> ();
	/// <summary>
	/// 初期状態フレーム（プロパティ用のインスタンス）
	/// </summary>
	private Frame defaultFrame;

	void Start () {
		index = 0;
		// リストに関節オブジェクトを追加
		modelJointList.Add (LeftShoulderP);
		modelJointList.Add (LeftThighY);
		modelJointList.Add (LeftShoulderR);
		modelJointList.Add (LeftElbowR);
		modelJointList.Add (LeftThighR);
		modelJointList.Add (LeftThighP);
		modelJointList.Add (LeftKneeP);
		modelJointList.Add (LeftFootP);
		modelJointList.Add (LeftFootR);
		modelJointList.Add (RightShoulderP);
		modelJointList.Add (RightThighY);
		modelJointList.Add (RightShoulderR);
		modelJointList.Add (RightElbowR);
		modelJointList.Add (RightThighR);
		modelJointList.Add (RightThighP);
		modelJointList.Add (RightKneeP);
		modelJointList.Add (RightFootP);
		modelJointList.Add (RightFootR);
		// 初期フレームを作成．
		defaultFrame = new Frame (modelJointList);
		// 初期状態にモデルを調整．
		modelAllJointRotation (DefaultFrame);
	}

	void Update() {
	}

	public void CreateNewFrame(int newFrameIndex = -1, int baseFrameIndex = -1) {
		Frame baseFrame;
		if (baseFrameIndex < 0 || frameList.Count == 0)
			baseFrame = defaultFrame;
		else
			baseFrame = frameList [baseFrameIndex];
		
		if (newFrameIndex < 0) {
			frameList.Add (new Frame (baseFrame, modelJointList));
			index = frameList.Count - 1;
		} else {
			frameList.Insert (newFrameIndex, new Frame (baseFrame, modelJointList));
			index = newFrameIndex;
		}
	}
	public void ChangeSelectFrame(int selectedIndex) {
		index = selectedIndex;
		modelAllJointRotation (frameList [index]);
	}

	public void FrameInitialize(int initIndex) {
		//frameList [index] = DefaultFrame;
		// 
		for (int i = 0; i < DefaultFrame.jointAngles.Length; i++)
			frameList [initIndex].jointAngles [i] = new JointAngle(DefaultFrame.jointAngles [i]);
		
		modelAllJointRotation (frameList [initIndex]);
	}
	public void FrameRemove(int removeIndex) {
		frameList.RemoveAt (removeIndex);
		if (index > frameList.Count - 1)
			index = frameList.Count - 1;
	}

	public string MotionJSONDataCreate(string savePath) {
		PLEN.JSON.Main jsonMain = new PLEN.JSON.Main ();
		jsonMain.name = "Sample";
		jsonMain.slot = 0;
		foreach (Frame frame in frameList) {
			jsonMain.frames.Add(frame.FrameJSONDataCreate());
		}

		return JsonMapper.ToJson (jsonMain);

	}

	public bool MotionJSONDataRead(string jsonStr) {
		try {
			PLEN.JSON.Main jsonMain = LitJson.JsonMapper.ToObject<PLEN.JSON.Main> (jsonStr);

			if (jsonMain == null)
				return false;
	
			frameList.Clear ();

			foreach (PLEN.JSON.Frame jsonFrame in jsonMain.frames) {
				Frame frame = new Frame (defaultFrame, modelJointList);
				frame.transitionTime = jsonFrame.transition_time_ms;

				foreach (PLEN.JSON.Output output in jsonFrame.outputs) {
					PLEN.JointName parseJointName;
					float angle;
					try {
						parseJointName = (PLEN.JointName)Enum.Parse(typeof(PLEN.JointName), output.device);
						// 関節角度読み込み（左半身は回転量を反転）
						angle = (float)output.value / 10;
						if ((int)parseJointName < modelJointList.Count / 2)
						angle *= -1;
						
						frame.JointRotate(parseJointName, angle);
					} catch(Exception) {
						return false;
					}
				}
				frameList.Add (frame);

			}
			return true;
		}catch(Exception) {
			return false;
		}
	}

	private void modelAllJointRotation(Frame frame) {
		for (int i = 0; i < modelJointList.Count; i++)
			modelJointList [i].transform.localEulerAngles = frame.jointAngles[i].eulerAngle;
	}


}

public class Frame {
	public JointAngle[] jointAngles;
	public int transitionTime = 1000;
	private List<GameObject> modelJointList;

	public Frame(List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];
		// 関節情報セット
		// 左肩ロール，右肩ロールに関しては回転方向をヨーに（モデルの座標的に）
		jointAngles [0]  =  new JointAngle (0 , AdjustPossibleCoord.Yaw, jointList[0].transform.localEulerAngles);
		jointAngles [1]  =  new JointAngle (1 , AdjustPossibleCoord.Yaw, jointList[1].transform.localEulerAngles);
		jointAngles [2]  =  new JointAngle (2 , AdjustPossibleCoord.Roll, jointList[2].transform.localEulerAngles);
		jointAngles [3]  =  new JointAngle (3 , AdjustPossibleCoord.Roll, jointList[3].transform.localEulerAngles);
		jointAngles [4]  =  new JointAngle (4 , AdjustPossibleCoord.Roll, jointList[4].transform.localEulerAngles);
		jointAngles [5]  =  new JointAngle (5 , AdjustPossibleCoord.Pitch, jointList[5].transform.localEulerAngles);
		jointAngles [6]  =  new JointAngle (6 , AdjustPossibleCoord.Pitch, jointList[6].transform.localEulerAngles);
		jointAngles [7]  =  new JointAngle (7 , AdjustPossibleCoord.Pitch, jointList[7].transform.localEulerAngles);
		jointAngles [8]  =  new JointAngle (8 , AdjustPossibleCoord.Roll, jointList[8].transform.localEulerAngles);
		jointAngles [9]  =  new JointAngle (9 , AdjustPossibleCoord.Yaw, jointList[9].transform.localEulerAngles);
		jointAngles [10] =  new JointAngle (10, AdjustPossibleCoord.Yaw, jointList[10].transform.localEulerAngles);
		jointAngles [11] =  new JointAngle (11, AdjustPossibleCoord.Roll, jointList[11].transform.localEulerAngles);
		jointAngles [12] =  new JointAngle (12, AdjustPossibleCoord.Roll, jointList[12].transform.localEulerAngles);
		jointAngles [13] =  new JointAngle (13, AdjustPossibleCoord.Roll, jointList[13].transform.localEulerAngles);
		jointAngles [14] =  new JointAngle (14, AdjustPossibleCoord.Pitch, jointList[14].transform.localEulerAngles);
		jointAngles [15] =  new JointAngle (15, AdjustPossibleCoord.Pitch, jointList[15].transform.localEulerAngles);
		jointAngles [16] =  new JointAngle (16, AdjustPossibleCoord.Pitch, jointList[16].transform.localEulerAngles);
		jointAngles [17] =  new JointAngle (17, AdjustPossibleCoord.Roll, jointList[17].transform.localEulerAngles);


		modelJointList = jointList;
	}

	public Frame(Frame baseFrame, List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];

		for (int i = 0; i < jointAngles.Length; i++)
			jointAngles [i] = new JointAngle (baseFrame.jointAngles [i]);
		modelJointList = jointList;
		transitionTime = baseFrame.transitionTime;
	}

	public void JointRotate(PLEN.JointName jointName, float angle) {
		//Debug.Log (jointName.ToString () + " : " + angle.ToString ());

		if (angle != 0.0f) {
			if ((int)jointName >= modelJointList.Count / 2)
				angle *= -1;
			
			switch (jointAngles [(int)jointName].coord) {
			//x
			case AdjustPossibleCoord.Pitch:
				jointAngles [(int)jointName].eulerAngle += new Vector3 (-angle, 0.0f, 0.0f);
				break;
			// y
			case AdjustPossibleCoord.Roll:
				jointAngles [(int)jointName].eulerAngle += new Vector3 (0.0f, angle, 0.0f);
				break;
			//z
			case AdjustPossibleCoord.Yaw:
				jointAngles [(int)jointName].eulerAngle += new Vector3 (0.0f, 0.0f, angle);
				break;
			default:
				break;
			}
			modelJointList [(int)jointName].transform.localEulerAngles = jointAngles [(int)jointName].eulerAngle;
			jointAngles [(int)jointName].Angle += angle;
		}
	}

	public PLEN.JSON.Frame FrameJSONDataCreate() {
		PLEN.JSON.Frame jsonFrame = new PLEN.JSON.Frame ();
		jsonFrame.transition_time_ms = transitionTime;
		foreach (JointAngle jointAngle in jointAngles) {
			PLEN.JSON.Output jsonOutput = new PLEN.JSON.Output ();
			jsonOutput.device = ((PLEN.JointName)(jointAngle.jointIndex)).ToString();
			jsonOutput.value = (int)(jointAngle.Angle * -10);
			jsonFrame.outputs.Add (jsonOutput);
		}
		return jsonFrame;
	}
}

public class JointAngle {
	public readonly int jointIndex;
	public AdjustPossibleCoord coord;
	public Vector3 eulerAngle; 

	public float Angle {
		get{
			return _angle;
		}
		set{
			_angle = value;
			while (_angle > 180)
				_angle -= 360;
			while (_angle < -180)
				_angle += 360;
		}
	}
	private float _angle;

	public JointAngle(int _jointIndex, AdjustPossibleCoord _coord, Vector3 _eulerAngle) {
		jointIndex = _jointIndex;
		coord = _coord;
		eulerAngle = _eulerAngle;
	}
	public JointAngle(JointAngle baseJointAngle) {
		jointIndex = baseJointAngle.jointIndex;
		coord = baseJointAngle.coord;
		eulerAngle = baseJointAngle.eulerAngle;
	}
}


public enum AdjustPossibleCoord {
	Roll,
	Pitch,
	Yaw
}
