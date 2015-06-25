using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PLENModelAnimation : MonoBehaviour {
	/// <summary>
	/// モーションデータを管理するインスタンス（インスペクタで初期化）
	/// </summary>
	public MotionData motionData;
	/// <summary>
	/// フレーム表示区域パネル（インスペクタで初期化）
	/// </summary>
	public PanelFramesScript panelFrames;
	/// <summary>
	/// アニメーション再生中フラグ（読み取り専用）
	/// </summary>
	public bool IsPlaying {
		get { return isPlaying; }
	}
	/// <summary>
	/// アニメーションクリップリスト
	/// </summary>
	private List<AnimationClip> animationClipList = new List<AnimationClip> ();
	/// <summary>
	/// アニメーションに用いるGameObjectの相対パス
	/// </summary>
	private string[] relativePathes;
	/// <summary>
	/// Animation
	/// </summary>
	private Animation thisAnimation;
	/// <summary>
	/// アニメーション再生フラグ
	/// </summary>
	private bool isPlaying = false;
	/// <summary>
	/// 再生中のクリップ番号
	/// </summary>
	private int playingClip;

	// Use this for initialization
	void Start () {
		thisAnimation = this.GetComponent<Animation> ();
		// アニメーションに用いるGameObject（関節オブジェクト）のパス(このスクリプトがアタッチされている場所からの相対パス）
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
			// アニメーションの再生が終了しているか判定
			if (thisAnimation.isPlaying == false) {
				// まだ再生すべきクリップが残っているなら再生
				if (playingClip < animationClipList.Count - 1) {
					playingClip++;
					panelFrames.AnimationClipChanged (playingClip);
					thisAnimation.Play (animationClipList [playingClip].name);
				} else {
					// 全アニメーションクリップが再生終了したので通知
					isPlaying = false;
					panelFrames.PlayAnimationEnded (animationClipList.Count);
				}
			}
		}

	}
	/// <summary>
	/// アニメーション停止メソッド
	/// </summary>
	public void AnimationStop() {
		if (isPlaying == true) {
			thisAnimation.Stop ();
			isPlaying = false;
			panelFrames.PlayAnimationEnded (playingClip);
		}
	}
	/// <summary>
	/// アニメーション再生メソッド
	/// </summary>
	public void AnimationPlay() {
		// フレームが1つしかない → そもそもアニメーションが作れない
		if (motionData.frameList.Count <= 1)
			return;

		// アニメーション作成
		createAnimationClips ();
		// 現在再生中のアニメーションを停止
		thisAnimation.Stop ();
		// 変数初期化
		playingClip = 0;
		isPlaying = true;
		// アニメーション再生通知
		panelFrames.AnimationStarted (0);
		// アニメーション再生
		thisAnimation.Play (animationClipList [0].name);
	}
	/// <summary>
	/// アニメーション作成メソッド
	/// </summary>
	private void createAnimationClips() {
		
		// すでにアニメーションがある場合，全削除
		foreach (AnimationClip clip in animationClipList) {
			thisAnimation.RemoveClip (clip);
		}
		animationClipList.Clear ();

		/*-- ここからフレーム作成 --*/
		for(int i = 0; i < motionData.frameList.Count - 1; i++) {
			// アニメーションクリップ作成．初期設定．
			int clipIndex = animationClipList.Count;
			animationClipList.Add (new AnimationClip ());
			animationClipList [clipIndex].legacy = true;	// SetCurveを使用するにはlegacyをtrueにする必要あり
			animationClipList [clipIndex].name = clipIndex.ToString ();
			// 全関節に対してアニメーションを作成
			for(int j = 0; j < motionData.modelJointList.Count; j++) {
				// 変数初期化 (今のフレームと一つ先のフレームの回転量と遷移時間）
				var angleOld = Quaternion.Euler (motionData.frameList [i].jointAngles [j].eulerAngle);
				var angleNew = Quaternion.Euler (motionData.frameList [i+1].jointAngles [j].eulerAngle);
				var time = (float)motionData.frameList [i + 1].transitionTime / 1000;
				// Rotationプロパティのx,y,z,wに対して一つずつアニメーションを設定
				AnimationCurve curveX = AnimationCurve.Linear (0f, angleOld.x, time, angleNew.x);
				AnimationCurve curveY = AnimationCurve.Linear (0f, angleOld.y, time, angleNew.y);				
				AnimationCurve curveZ = AnimationCurve.Linear (0f, angleOld.z, time, angleNew.z);				
				AnimationCurve curveW = AnimationCurve.Linear (0f, angleOld.w, time, angleNew.w);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.x", curveX);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.y", curveY);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.z", curveZ);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.w", curveW);
			}
			// 作成したアニメーションクリップをAnimationに加える
			thisAnimation.AddClip (animationClipList [clipIndex], animationClipList [clipIndex].name);
		}
	}

}
