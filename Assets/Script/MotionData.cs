using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MotionData : MonoBehaviour {
	public List<Frame> frameList =  new List<Frame>();

	// 関節オブジェクト類（これらにセットされているオブジェクトの角度を変更していく）
	// Note...これらはUnityのInspector上で設定されるので，ソース上で初期化等の処理はしない．
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
	/// 現在編集中のフレームインデックス
	/// </summary>
	public int index;
	/// <summary>
	/// 初期状態フレーム（読み取り専用）
	/// </summary>
	public Frame DefaultFrame {
		get { return defaultFrame; }
	}
	/// <summary>
	/// 関節オブジェクトリスト
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
		// 初期フレームを作成．フレームリストに追加．
		defaultFrame = new Frame (modelJointList);
		frameList.Add(new Frame(defaultFrame, modelJointList));
		// 初期状態にモデルを調整．
		modelAllJointRotation (DefaultFrame);
	}

	void Update() {
	}

	public void FrameInitialize(int index) {
		//frameList [index] = DefaultFrame;
		// 
		for (int i = 0; i < DefaultFrame.jointAngles.Length; i++)
			frameList [index].jointAngles [i] = new JointAngle(DefaultFrame.jointAngles [i]);
		
		modelAllJointRotation (frameList [index]);
	}

	private void modelAllJointRotation(Frame frame) {
		for (int i = 0; i < modelJointList.Count; i++)
			modelJointList [i].transform.localEulerAngles = frame.jointAngles[i].eulerAngle;
	}

}

public class Frame {
	public JointAngle[] jointAngles;
	private List<GameObject> modelJointList;

	public Frame(List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];
		// 関節情報セット
		// 左肩ロール，右肩ロールに関しては回転方向をヨーに（モデルの座標的に）
		jointAngles [0]  =  new JointAngle (0 , 0, 0, 0, AdjustPossibleCoord.Yaw, jointList[0].transform.localEulerAngles);
		jointAngles [1]  =  new JointAngle (1 , 0, 0, 0, AdjustPossibleCoord.Yaw, jointList[1].transform.localEulerAngles);
		jointAngles [2]  =  new JointAngle (2 , 0, 0, 0, AdjustPossibleCoord.Roll, jointList[2].transform.localEulerAngles);
		jointAngles [3]  =  new JointAngle (3 , 0, 0, 0, AdjustPossibleCoord.Roll, jointList[3].transform.localEulerAngles);
		jointAngles [4]  =  new JointAngle (4 , 0, 0, 0, AdjustPossibleCoord.Roll, jointList[4].transform.localEulerAngles);
		jointAngles [5]  =  new JointAngle (5 , 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[5].transform.localEulerAngles);
		jointAngles [6]  =  new JointAngle (6 , 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[6].transform.localEulerAngles);
		jointAngles [7]  =  new JointAngle (7 , 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[7].transform.localEulerAngles);
		jointAngles [8]  =  new JointAngle (8 , 0, 0, 0, AdjustPossibleCoord.Roll, jointList[8].transform.localEulerAngles);
		jointAngles [9]  =  new JointAngle (9 , 0, 0, 0, AdjustPossibleCoord.Yaw, jointList[9].transform.localEulerAngles);
		jointAngles [10] =  new JointAngle (10, 0, 0, 0, AdjustPossibleCoord.Yaw, jointList[10].transform.localEulerAngles);
		jointAngles [11] =  new JointAngle (11, 0, 0, 0, AdjustPossibleCoord.Roll, jointList[11].transform.localEulerAngles);
		jointAngles [12] =  new JointAngle (12, 0, 0, 0, AdjustPossibleCoord.Roll, jointList[12].transform.localEulerAngles);
		jointAngles [13] =  new JointAngle (13, 0, 0, 0, AdjustPossibleCoord.Roll, jointList[13].transform.localEulerAngles);
		jointAngles [14] =  new JointAngle (14, 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[14].transform.localEulerAngles);
		jointAngles [15] =  new JointAngle (15, 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[15].transform.localEulerAngles);
		jointAngles [16] =  new JointAngle (16, 0, 0, 0, AdjustPossibleCoord.Pitch, jointList[16].transform.localEulerAngles);
		jointAngles [17] =  new JointAngle (17, 0, 0, 0, AdjustPossibleCoord.Roll, jointList[17].transform.localEulerAngles);


		modelJointList = jointList;
	}

	public Frame(Frame baseFrame, List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];

		for (int i = 0; i < jointAngles.Length; i++)
			jointAngles [i] = new JointAngle (baseFrame.jointAngles [i]);
		modelJointList = jointList;
	}

	public void JointRotate(JointName jointName, float angle) {
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
		}
	}

}

public class JointAngle {
	public readonly int jointIndex;
	public readonly int max;
	public readonly int home;
	public readonly int min;
	public AdjustPossibleCoord coord;
	public Vector3 eulerAngle; 

	public int Angle {
		get{
			return _angle;
		}
		set{
			if (value > max)		_angle = max;
			else if (value < min)	_angle = min;
			else					_angle = value;
		}
	}
	private int _angle;


	public JointAngle(int _jointIndex, int _max, int _home, int _min, AdjustPossibleCoord _coord, Vector3 _eulerAngle) {
		jointIndex = _jointIndex;
		max = _max;
		home = _home;
		min = _min;
		coord = _coord;
		eulerAngle = _eulerAngle;
	}
	public JointAngle(JointAngle baseJointAngle) {
		jointIndex = baseJointAngle.jointIndex;
		max = baseJointAngle.max;
		home = baseJointAngle.home;
		min = baseJointAngle.min;
		coord = baseJointAngle.coord;
		eulerAngle = baseJointAngle.eulerAngle;
	}
}


public enum AdjustPossibleCoord {
	Roll,
	Pitch,
	Yaw
}


public enum JointName {
	LeftShoulderP,
	LeftThighY,
	LeftShoulderR,
	LeftElbowR,
	LeftThighR,
	LeftThighP,
	LeftKneeP,
	LeftFootP,
	LeftFootR,
	RightShoulderP,
	RightThighY,
	RightShoulderR,
	RightElbowR,
	RightThighR,
	RightThighP,
	RightKneeP,
	RightFootP,
	RightFootR
}