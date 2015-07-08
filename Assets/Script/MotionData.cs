using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using PLEN;

public class MotionData : MonoBehaviour {
	public ObjectsController objects;

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
	public List<Frame> frameList =  new List<Frame>();
	public int slotNum;
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
	/// <summary>
	///  新規フレーム作成メソッド
	/// </summary>
	/// <param name="newFrameIndex">作成インデックス（0未満：最終フレームへ）</param>
	/// <param name="baseFrameIndex">ベースとなるフレームのインデックス（0未満：初期状態フレーム）</param>
	public void CreateNewFrame(int newFrameIndex = -1, int baseFrameIndex = -1) {
		Frame baseFrame;
		//  ベースとなるフレームを設定
		if (baseFrameIndex < 0 || frameList.Count == 0)
			baseFrame = DefaultFrame;
		else
			baseFrame = frameList [baseFrameIndex];
		// フレーム作成．リストに追加
		if (newFrameIndex < 0) {
			frameList.Add (new Frame (baseFrame));
			index = frameList.Count - 1;
		} else {
			frameList.Insert (newFrameIndex, new Frame (baseFrame));
			index = newFrameIndex;
		}
	}
	/// <summary>
	///  選択中フレーム変更メソッド
	/// </summary>
	/// <param name="selectedIndex">選択フレームインデックス</param>
	public void ChangeSelectFrame(int selectedIndex) {
		// インデックス変更．モデル回転．
		index = selectedIndex;
		modelAllJointRotation (frameList [index]);
	}
	/// <summary>
	///  フレーム初期化メソッド
	/// </summary>
	/// <param name="initIndex">初期化フレームインデックス</param>
	/// <param name="isTransitionTimeReset"> <c>true</c> : TransitionTimeも初期化 </param>
	public void FrameInitialize(int initIndex, bool isTransitionTimeReset = false) {
		// 
		for (int i = 0; i < DefaultFrame.jointAngles.Length; i++)
			frameList [initIndex].jointAngles [i] = new JointAngle(DefaultFrame.jointAngles [i]);

		if (isTransitionTimeReset == true)
			frameList [initIndex].transitionTime = DefaultFrame.transitionTime;

		modelAllJointRotation (frameList [initIndex]);
	}
	/// <summary>
	/// フレーム削除メソッド
	/// </summary>
	/// <param name="removeIndex">削除フレームインデックス</param>
	public void FrameRemove(int removeIndex) {
		frameList.RemoveAt (removeIndex);
		if (index > frameList.Count - 1)
			index = frameList.Count - 1;
	}

	public void ModelTurnOver() {
		Debug.Log (index + " :  : : ");
		float[] anglesTmp = new float[modelJointList.Count];
		for (int i = 0; i < frameList [index].jointAngles.Length; i++) {
			anglesTmp[i] = frameList [index].jointAngles [i].Angle;
		}

		int offset = modelJointList.Count / 2;
		for (int i = 0; i < modelJointList.Count / 2; i++) {
			frameList [index].JointRotate (i, - frameList [index].jointAngles [i].Angle * Frame.DATA_ROTATE_DIRECTION[i], false);
			frameList [index].JointRotate (i, frameList [index].jointAngles [i + offset].Angle * Frame.DATA_ROTATE_DIRECTION[i+offset], false);
		}
		for (int i = offset; i < modelJointList.Count; i++) {
			frameList [index].JointRotate (i, - frameList [index].jointAngles [i].Angle * Frame.DATA_ROTATE_DIRECTION[i], false);
			frameList [index].JointRotate (i,anglesTmp[i-offset] * Frame.DATA_ROTATE_DIRECTION[i-offset], false);
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
				Frame frame = new Frame (DefaultFrame);
				frame.transitionTime = jsonFrame.transition_time_ms;
				// JSONファイルから読み込んだフレームから各関節の情報を読み取り，設定を行う
				foreach (PLEN.JSON.Output output in jsonFrame.outputs) {
					PLEN.JointName parseJointName;
					float angle;
					try {
						// JSONファイル中のdevice情報をJointName型に変換
						parseJointName = (PLEN.JointName)Enum.Parse(typeof(PLEN.JointName), output.device);
						// 関節角度読み込み（左半身は回転量を反転）
						angle = (float)output.value / 10;
						//if ((int)parseJointName < modelJointList.Count / 2)
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
			objects.menuController.InputFieldSlotUpdate(jsonMain.slot);
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
	private void modelAllJointRotation(Frame frame) {
		for (int i = 0; i < modelJointList.Count; i++)
			modelJointList [i].transform.localEulerAngles = frame.jointAngles[i].eulerAngle;
	}
}

public class Frame {
	/// <summary>
	/// 関節配列
	/// </summary>
	public JointAngle[] jointAngles;
	/// <summary>
	/// フレーム遷移時間
	/// </summary>
	public int transitionTime = 1000;
	/// <summary>
	/// 関節オブジェクトリスト
	/// </summary>
	private List<GameObject> modelJointList;

	public static readonly int[] DATA_ROTATE_DIRECTION = { 1, 1, -1, 1, -1, 1, 1, -1 ,1, -1, -1, 1, -1, 1, -1, -1, 1, -1 }; 
	public static readonly int[] DISP_ROTATE_DIRECTION =  { -1, -1, -1, 1, -1, -1, -1, 1, 1, 1, 1, 1, -1, 1, 1, 1, -1, -1 };
	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="jointList">関節オブジェクトリスト</param>
	public Frame(List<GameObject> jointList) {
		jointAngles = new JointAngle[jointList.Count];
		// 関節情報セット
		// 左肩ピッチ，右肩ピッチに関しては回転方向をヨーに（モデルの座標的に）
		jointAngles [0]  =  new JointAngle (0 , RotationCoord.Yaw, jointList[0].transform.localEulerAngles);
		jointAngles [1]  =  new JointAngle (1 , RotationCoord.Yaw, jointList[1].transform.localEulerAngles);
		jointAngles [2]  =  new JointAngle (2 , RotationCoord.Roll, jointList[2].transform.localEulerAngles);
		jointAngles [3]  =  new JointAngle (3 , RotationCoord.Roll, jointList[3].transform.localEulerAngles);
		jointAngles [4]  =  new JointAngle (4 , RotationCoord.Roll, jointList[4].transform.localEulerAngles);
		jointAngles [5]  =  new JointAngle (5 , RotationCoord.Pitch, jointList[5].transform.localEulerAngles);
		jointAngles [6]  =  new JointAngle (6 , RotationCoord.Pitch, jointList[6].transform.localEulerAngles);
		jointAngles [7]  =  new JointAngle (7 , RotationCoord.Pitch, jointList[7].transform.localEulerAngles);
		jointAngles [8]  =  new JointAngle (8 , RotationCoord.Roll, jointList[8].transform.localEulerAngles);
		jointAngles [9]  =  new JointAngle (9 , RotationCoord.Yaw, jointList[9].transform.localEulerAngles);
		jointAngles [10] =  new JointAngle (10, RotationCoord.Yaw, jointList[10].transform.localEulerAngles);
		jointAngles [11] =  new JointAngle (11, RotationCoord.Roll, jointList[11].transform.localEulerAngles);
		jointAngles [12] =  new JointAngle (12, RotationCoord.Roll, jointList[12].transform.localEulerAngles);
		jointAngles [13] =  new JointAngle (13, RotationCoord.Roll, jointList[13].transform.localEulerAngles);
		jointAngles [14] =  new JointAngle (14, RotationCoord.Pitch, jointList[14].transform.localEulerAngles);
		jointAngles [15] =  new JointAngle (15, RotationCoord.Pitch, jointList[15].transform.localEulerAngles);
		jointAngles [16] =  new JointAngle (16, RotationCoord.Pitch, jointList[16].transform.localEulerAngles);
		jointAngles [17] =  new JointAngle (17, RotationCoord.Roll, jointList[17].transform.localEulerAngles);
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
		transitionTime = baseFrame.transitionTime;
	}

	public void JointRotate(PLEN.JointName jointName, float angle, bool isMotionDataRead = false) {
		JointRotate ((int)jointName, angle, isMotionDataRead);
	}
	/// <summary>
	///  関節回転メソッド（angle値も更新）
	/// </summary>
	/// <param name="jointName">回転関節名</param>
	/// <param name="angle">回転角度</param>
	/// <param name="isAdjust"><c>true</c> : 左右でangle値を反転する</param>
	public void JointRotate(int jointIndex, float angle, bool isMotionDataRead = false) {
		float dispAngle = angle;
		Debug.Log (jointIndex+ " : " + angle);
		if (angle != 0.0f) {
			
			// パーツによって回転方向が異なるため，angle値を調整
			if (isMotionDataRead == false) {
				dispAngle *= DISP_ROTATE_DIRECTION [jointIndex];
			} else {
				dispAngle *= DATA_ROTATE_DIRECTION [jointIndex] * DISP_ROTATE_DIRECTION [jointIndex];
			}
			// 回転オイラー角を生成
			switch (jointAngles [jointIndex].coord) {
			//x
			case RotationCoord.Roll:
				jointAngles [jointIndex].eulerAngle += new Vector3 (-dispAngle, 0.0f, 0.0f);
				break;
			// y
			case RotationCoord.Pitch:
				jointAngles [jointIndex].eulerAngle += new Vector3 (0.0f, dispAngle, 0.0f);
				break;
			//z
			case RotationCoord.Yaw:
				jointAngles [jointIndex].eulerAngle += new Vector3 (0.0f, 0.0f, dispAngle);
				break;
			default:
				break;
			}
			// 関節オブジェクト回転
			modelJointList [jointIndex].transform.localEulerAngles = jointAngles [jointIndex].eulerAngle;
			// angle値更新
			if (isMotionDataRead == false) {
				jointAngles [jointIndex].Angle += angle * DATA_ROTATE_DIRECTION [jointIndex];
			} else {
				jointAngles [jointIndex].Angle = angle;
			}
		}
	}
	/// <summary>
	/// モーションファイル(JSON)用データ生成メソッド
	/// </summary>
	/// <returns>JSON用データ</returns>
	public PLEN.JSON.Frame FrameJSONDataCreate() {
		// インスタンス作成．遷移時間設定．
		PLEN.JSON.Frame jsonFrame = new PLEN.JSON.Frame ();
		jsonFrame.transition_time_ms = transitionTime;
		// 各関節データをJSONデータに変換．
		foreach (JointAngle jointAngle in jointAngles) {
			// 各種情報をJSONデータに変換
			PLEN.JSON.Output jsonOutput = new PLEN.JSON.Output ();
			jsonOutput.device = ((PLEN.JointName)(jointAngle.jointIndex)).ToString();
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
	///  関節番号（読み取り専用）
	/// </summary>
	public readonly int jointIndex;
	/// <summary>
	/// 回転方向
	/// </summary>
	public RotationCoord coord;
	/// <summary>
	/// 回転オイラー角
	/// </summary>
	public Vector3 eulerAngle; 
	/// <summary>
	/// 関節の初期位置からの角度（-180＜Angle＜180）
	/// </summary>
	/// <value>The angle.</value>
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
	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="_jointIndex">関節番号</param>
	/// <param name="_coord">回転方向</param>
	/// <param name="_eulerAngle">初期オイラー角</param>
	public JointAngle(int _jointIndex, RotationCoord _coord, Vector3 _eulerAngle) {
		jointIndex = _jointIndex;
		coord = _coord;
		eulerAngle = _eulerAngle;
	}
	/// <summary>
	/// コンストラクタ（ベースとなるJointAngleあり）
	/// </summary>
	/// <param name="baseJointAngle">ベースとなるJointAngle</param>
	public JointAngle(JointAngle baseJointAngle) {
		jointIndex = baseJointAngle.jointIndex;
		coord = baseJointAngle.coord;
		eulerAngle = baseJointAngle.eulerAngle;
		Angle = baseJointAngle.Angle;
	}
}

/// <summary>
/// 関節回転方向
/// </summary>
public enum RotationCoord {
	Roll,
	Pitch,
	Yaw
}
