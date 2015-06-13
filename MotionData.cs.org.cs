using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MotionData : MonoBehaviour {
	public List<JointAngle> jointList = new List<JointAngle> ();
	public Transform LeftShoulderP;
	public Transform LeftThighY;
	public Transform LeftShoulderR;
	public Transform LeftElbowR;
	public Transform LeftThighR;
	public Transform LeftThighP;
	public Transform LeftKneeP;
	public Transform LeftFootP;
	public Transform LeftFootR;
	public Transform RightShoulderP;
	public Transform RightThighY;
	public Transform RightShoulderR;
	public Transform RightElbowR;
	public Transform RightThighR;
	public Transform RightThighP;
	public Transform RightKneeP;
	public Transform RightFootP;
	public Transform RightFootR;
	public Frame DefaultFrame {
		get { return DefaultFrame; }
	}

	private List<Transform> modelJointList = new List<Transform> ();
	private Frame defaultFrame;

	void Start () {
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
		modelJointList.Add (RightThighP);
		modelJointList.Add (RightThighP);	
		modelJointList.Add (RightKneeP);
		modelJointList.Add (RightFootP);	
		modelJointList.Add (RightFootR);	

		defaultFrame = new Frame (modelJointList);

	}

	void Update() {
	}

}

public class Frame {
	public JointAngle[] jointAngles = new JointAngle[18];


	public Frame(List<Transform> modelJointList) {
		jointAngles [0] =  new JointAngle (0 , 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [0].localEulerAngles);
		jointAngles [1] =  new JointAngle (1 , 0, 0, 0, AdjustPossibleCoord.Yaw, modelJointList [1].localEulerAngles);
		jointAngles [2] =  new JointAngle (2 , 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [2].localEulerAngles);
		jointAngles [3] =  new JointAngle (3 , 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [3].localEulerAngles);
		jointAngles [4] =  new JointAngle (4 , 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [4].localEulerAngles);
		jointAngles [5] =  new JointAngle (5 , 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [5].localEulerAngles);
		jointAngles [6] =  new JointAngle (6 , 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [6].localEulerAngles);
		jointAngles [7] =  new JointAngle (7 , 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [7].localEulerAngles);
		jointAngles [8] =  new JointAngle (8 , 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [8].localEulerAngles);
		jointAngles [9] =  new JointAngle (12, 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [9].localEulerAngles);
		jointAngles [10] = new JointAngle (13, 0, 0, 0, AdjustPossibleCoord.Yaw, modelJointList [10].localEulerAngles);
		jointAngles [11] = new JointAngle (14, 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [11].localEulerAngles);
		jointAngles [12] = new JointAngle (15, 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [12].localEulerAngles);
		jointAngles [13] = new JointAngle (16, 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [13].localEulerAngles);
		jointAngles [14] = new JointAngle (17, 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [14].localEulerAngles);
		jointAngles [15] = new JointAngle (18, 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [16].localEulerAngles);
		jointAngles [16] = new JointAngle (19, 0, 0, 0, AdjustPossibleCoord.Pitch, modelJointList [16].localEulerAngles);
		jointAngles [17] = new JointAngle (20, 0, 0, 0, AdjustPossibleCoord.Roll, modelJointList [17].localEulerAngles);
	}

	public Frame(Frame baseFrame) {
		for (int i = 0; i < jointAngles.Length; i++)
			jointAngles [i] = baseFrame.jointAngles [i];
	}
}

public class JointAngle {
	public readonly int jointIndex;
	public readonly int max;
	public readonly int home;
	public readonly int min;
	public AdjustPossibleCoord coord;
	public readonly Vector3 modelDefaultJointEulerAngle; 

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


	public JointAngle(int _jointIndex, int _max, int _home, int _min, AdjustPossibleCoord _coord, Vector3 _modelDefaultJointEulerAngle) {
		jointIndex = _jointIndex;
		max = _max;
		home = _home;
		min = _min;
		coord = _coord;
		modelDefaultJointEulerAngle = _modelDefaultJointEulerAngle;
	}
}


public enum AdjustPossibleCoord {
	Roll,
	Pitch,
	Yaw
}