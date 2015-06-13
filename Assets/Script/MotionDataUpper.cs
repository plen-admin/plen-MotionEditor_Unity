using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MotionDataUpper : MonoBehaviour {
	public List<Frame> frameList =  new List<Frame>();
	public Transform LeftShoulderP;
	public Transform LeftShoulderR;
	public Transform LeftElbowR;
	public Transform RightShoulderP;
	public Transform RightShoulderR;
	public Transform RightElbowR;
	public int index;
	public Frame DefaultFrame {
		get { return defaultFrame; }
	}

	public List<Transform> modelJointList = new List<Transform> ();
	private Frame defaultFrame;

	void Start () {
		index = 0;

		modelJointList.Add (LeftShoulderP);
		modelJointList.Add (LeftShoulderR);
		modelJointList.Add (LeftElbowR);
		modelJointList.Add (RightShoulderP);
		modelJointList.Add (RightShoulderR);
		modelJointList.Add (RightElbowR);

		defaultFrame = new Frame (modelJointList);
		frameList.Add(new Frame(defaultFrame, modelJointList));

		modelAllJointRotation (DefaultFrame);
	}

	void Update() {
	}

	public void FrameInitialize(int index) {
		//frameList [index] = DefaultFrame;
		for (int i = 0; i < DefaultFrame.jointAngles.Length; i++)
			frameList [index].jointAngles [i] = new JointAngle(DefaultFrame.jointAngles [i]);
		
		modelAllJointRotation (frameList [index]);
	}

	private void modelAllJointRotation(Frame frame) {
		for (int i = 0; i < modelJointList.Count; i++)
			modelJointList [i].localEulerAngles = frame.jointAngles[i].eulerAngle;
	}

}

public class Frame {
	public JointAngle[] jointAngles = new JointAngle[6];
	private List<Transform> modelJointList;

	public Frame(List<Transform> jointList) {
		jointAngles [0] =  new JointAngle (0 , 0, 0, 0, AdjustPossibleCoord.Pitch, new Vector3(0.0f, 10.0f, 0.0f));
		jointAngles [1] =  new JointAngle (1 , 0, 0, 0, AdjustPossibleCoord.Roll, new Vector3(0.0f, 70.0f, 0.0f));
		jointAngles [2] =  new JointAngle (2 , 0, 0, 0, AdjustPossibleCoord.Roll, new Vector3(-12.5f, 0.0f, 0.0f));
		jointAngles [3] =  new JointAngle (3 , 0, 0, 0, AdjustPossibleCoord.Pitch, new Vector3(0.0f, -10.0f, 0.0f));
		jointAngles [4] =  new JointAngle (4 , 0, 0, 0, AdjustPossibleCoord.Roll, new Vector3(0.0f, -70.0f, 0.0f));
		jointAngles [5] =  new JointAngle (5 , 0, 0, 0, AdjustPossibleCoord.Roll, new Vector3(0.0f, -12.0f, 0.0f));

		modelJointList = jointList;
	}

	public Frame(Frame baseFrame, List<Transform> jointList) {
		for (int i = 0; i < jointAngles.Length; i++)
			jointAngles [i] = new JointAngle (baseFrame.jointAngles [i]);
		modelJointList = jointList;
	}

	public void JointRotate(JointName jointName, float angle) {
		Debug.Log (jointName.ToString () + " : " + angle.ToString ());

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
			modelJointList [(int)jointName].localEulerAngles = jointAngles [(int)jointName].eulerAngle;
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
	LeftShoulderR,
	LeftElbowR,
	RightShoulderP,
	RightShoulderR,
	RightElbowR
}