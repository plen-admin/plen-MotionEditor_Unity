using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PLENModelAnimation : MonoBehaviour {

	public MotionData motionData;
	public PanelFramesScript panelFrames;

	private List<AnimationClip> animationClipList = new List<AnimationClip> ();
	private string[] relativePathes;
	private Animation thisAnimation;

	private bool isPlaying = false;
	private int playingClip;

	// Use this for initialization
	void Start () {
		thisAnimation = this.GetComponent<Animation> ();

		relativePathes = new string[18];
		relativePathes [0] = "Armature/Core/Body/Connect_Shoulder0_L/Connect_Shoulder1_L/Shoulder_L";
		relativePathes [1] = "Armature/Core/Connect_HipJoint0_L/Connect_HipJoint1_L/HipJoint_L";
		relativePathes [2] = relativePathes [0] + "/Connect_UpperArm_L/UpperArm_L";
		relativePathes [3] = relativePathes [2] + "/Arm_L";
		relativePathes [4] = relativePathes [1] + "/Connect_UpperThigh0_L/Connect_UpperThigh1_L/UpperThigh_L";
		relativePathes [5] = relativePathes [4] + "/LowerThigh_L";
		relativePathes [6] = relativePathes [5] + "/LowerLeg_L";
		relativePathes [7] = relativePathes [6] + "/Ankle_L";
		relativePathes [8] = relativePathes [7] + "/Foot_L";
		relativePathes [9]  = "Armature/Core/Body/Connect_Shoulder0_R/Connect_Shoulder1_R/Shoulder_R";
		relativePathes [10] = "Armature/Core/Connect_HipJoint0_R/Connect_HipJoint1_R/HipJoint_R";
		relativePathes [11] = relativePathes [9] + "/Connect_UpperArm_R/UpperArm_R";
		relativePathes [12] = relativePathes [11] + "/Arm_R";
		relativePathes [13] = relativePathes [10] + "/Connect_UpperThigh0_R/Connect_UpperThigh1_R/UpperThigh_R";
		relativePathes [14] = relativePathes [13] + "/LowerThigh_R";
		relativePathes [15] = relativePathes [14] + "/LowerLeg_R";
		relativePathes [16] = relativePathes [15] + "/Ankle_R";
		relativePathes [17] = relativePathes [16] + "/Foot_R";
	}
	
	// Update is called once per frame
	void Update () {
		if (isPlaying == true) {
			if (thisAnimation.isPlaying == false) {
				if (playingClip < animationClipList.Count - 1) {
					playingClip++;
					panelFrames.AnimationClipChanged (playingClip);
					thisAnimation.Play (animationClipList [playingClip].name);
				} else {
					isPlaying = false;
					panelFrames.PlayAnimationEnded (animationClipList.Count);
				}
			}
		}

	}
	public void AnimationStop() {
		if (isPlaying == true) {
			thisAnimation.Stop ();
			isPlaying = false;
			panelFrames.PlayAnimationEnded (playingClip);
		}
	}

	public void AnimationPlay() {
		createAnimationClips ();
		isPlaying = true;

		panelFrames.PlayAnimationStarted (0);

		playingClip = 0;
		thisAnimation.Stop ();
		thisAnimation.Play (animationClipList [0].name);
	}
	public void createAnimationClips() {

		if (motionData.frameList.Count <= 1)
			return;
		
		foreach (AnimationClip clip in animationClipList) {
			thisAnimation.RemoveClip (clip);
		}
		animationClipList.Clear ();

		for(int i = 0; i < motionData.frameList.Count - 1; i++) {
			int clipIndex = animationClipList.Count;
			animationClipList.Add (new AnimationClip ());
			animationClipList [clipIndex].legacy = true;
			animationClipList [clipIndex].name = clipIndex.ToString ();

			for(int j = 0; j < motionData.modelJointList.Count; j++) {
				var angleOld = Quaternion.Euler (motionData.frameList [i].jointAngles [j].eulerAngle);
				var angleNew = Quaternion.Euler (motionData.frameList [i+1].jointAngles [j].eulerAngle);
				var time = (float)motionData.frameList [i + 1].transitionTime / 1000;

				AnimationCurve curveX = AnimationCurve.Linear (0f, angleOld.x, time, angleNew.x);
				AnimationCurve curveY = AnimationCurve.Linear (0f, angleOld.y, time, angleNew.y);				
				AnimationCurve curveZ = AnimationCurve.Linear (0f, angleOld.z, time, angleNew.z);				
				AnimationCurve curveW = AnimationCurve.Linear (0f, angleOld.w, time, angleNew.w);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.x", curveX);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.y", curveY);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.z", curveZ);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.w", curveW);
			}
			thisAnimation.AddClip (animationClipList [clipIndex], animationClipList [clipIndex].name);
		}
		Debug.Log ("ClipCnt : " + thisAnimation.GetClipCount().ToString());
		foreach (AnimationClip clip in animationClipList)
			thisAnimation.PlayQueued (clip.name);
		
	}

}
