using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PLENModelAnimation : MonoBehaviour, IObjects {
    public delegate void AnimationStartedHandler(int startClipIndex);
    public delegate void AnimationClipChangedHandler(int changeClipIndex);
    public delegate void AnimationEndedHandler(int endClipIndex);

    public event AnimationStartedHandler AnimationStarted;
    public event AnimationClipChangedHandler AnimationClipChanged;
    public event AnimationEndedHandler AnimationEnded;

    /// <summary>
    /// 再生中判定フラグ（読み取り専用）
    /// </summary>
    public bool IsPlaying {
        get {
            return isPlaying;
        }
    }
    /// <summary>
    /// 共通利用オブジェクト類管理インスタンス
    /// </summary>
    private ObjectsController objects;
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
	/// <summary>
	///  再生中フレームインデックス
	/// </summary>
	private int playingFrameIndex;
	/// <summary>
	/// 再生終了インデックス
	/// </summary>
	private int endIndex;

    public void Initialize(ObjectsController controller) {
        objects = controller;
    }

	// Use this for initialization
	void Start () {
		thisAnimation = this.GetComponent<Animation> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (isPlaying == true) {
			// アニメーションの再生が終了しているか判定
			if (thisAnimation.isPlaying == false) {
				// まだ再生すべきクリップが残っているなら再生
				if (playingClip < animationClipList.Count - 1) {
					playingClip++;
					playingFrameIndex++;
                    if (AnimationClipChanged != null)
                        AnimationClipChanged(playingClip);
					thisAnimation.Play (animationClipList [playingClip].name);
				} else {
					// 全アニメーションクリップが再生終了したので通知
					isPlaying = false;
                    if (AnimationEnded != null)
                        AnimationEnded(endIndex);
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
            if (AnimationEnded != null)
                AnimationEnded(playingFrameIndex);
		}
	}

	public void AnimationPlay() {
		AnimationPlay (0, objects.PanelFrames.FrameCount - 1);
	}
	/// <summary>
	/// アニメーション再生メソッド
	/// </summary>
	public void AnimationPlay(int startIndex, int endIndex) {
		// フレームが1つしかない → そもそもアニメーションが作れない
		if (objects.MotionData.FrameList.Count <= 1 || startIndex == endIndex)
			return;
		// 各関節パーツの相対パスを生成
		// Note...Start()ではまだmodelJointListが生成されていない場合がある
		if (relativePathes == null || relativePathes.Length == 0) {
			relativePathes = new string[objects.MotionData.ModelJointList.Count];
			for (int i = 0; i < relativePathes.Length; i++) {
				relativePathes [i] = GetObjectPath (objects.MotionData.ModelJointList [i]);
			}
		}

		// アニメーション作成
		CreateAnimationClips (startIndex, endIndex);
		// 現在再生中のアニメーションを停止
		thisAnimation.Stop ();
		// 変数初期化
		this.endIndex = endIndex;
		playingClip = 0;
		playingFrameIndex = startIndex;
		isPlaying = true;
		// アニメーション再生通知
        if (AnimationStarted != null)
            AnimationStarted(startIndex);
		// アニメーション再生
		thisAnimation.Play (animationClipList [0].name);
	}
	/// <summary>
	/// アニメーション作成メソッド
	/// </summary>
	private void CreateAnimationClips(int startIndex, int endIndex) {
		int srcIndex = startIndex;
		int nxtIndex;

		// すでにアニメーションがある場合，全削除
		foreach (AnimationClip clip in animationClipList) {
			thisAnimation.RemoveClip (clip);
		}
		animationClipList.Clear ();

		/*-- ここからフレーム作成 --*/
		for(int i = 0; i < Mathf.Abs(startIndex - endIndex); i++) {
			if (startIndex < endIndex)
				nxtIndex = srcIndex + 1;
			else
				nxtIndex = srcIndex - 1;
			
			// アニメーションクリップ作成．初期設定．
			int clipIndex = animationClipList.Count;
			animationClipList.Add (new AnimationClip ());
			animationClipList [clipIndex].legacy = true;	// SetCurveを使用するにはlegacyをtrueにする必要あり
			animationClipList [clipIndex].name = clipIndex.ToString ();
			// 全関節に対してアニメーションを作成
			for(int j = 0; j < objects.MotionData.ModelJointList.Count; j++) {
				// 変数初期化 (今のフレームと一つ先のフレームの回転量と遷移時間）
				var angleOld = Quaternion.Euler (objects.MotionData.FrameList [srcIndex].JointAngles [j].EulerAngle);
				var angleNew = Quaternion.Euler (objects.MotionData.FrameList [nxtIndex].JointAngles[j].EulerAngle);
				var time = (float)objects.MotionData.FrameList [nxtIndex].TransitionTime / 1000;
				// Rotationプロパティのx,y,z,wに対して一つずつアニメーションを設定
				AnimationCurve curveX = AnimationCurve.EaseInOut (0f, angleOld.x, time, angleNew.x);
				AnimationCurve curveY = AnimationCurve.EaseInOut (0f, angleOld.y, time, angleNew.y);				
				AnimationCurve curveZ = AnimationCurve.EaseInOut (0f, angleOld.z, time, angleNew.z);				
				AnimationCurve curveW = AnimationCurve.EaseInOut (0f, angleOld.w, time, angleNew.w);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.x", curveX);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.y", curveY);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.z", curveZ);
				animationClipList [i].SetCurve (relativePathes [j], typeof(Transform), "localRotation.w", curveW);
			}
			// 作成したアニメーションクリップをAnimationに加える
			thisAnimation.AddClip (animationClipList [clipIndex], animationClipList [clipIndex].name);

			if (startIndex < endIndex)
				srcIndex++;
			else
				srcIndex--;
		}
	}

	private string GetObjectPath(GameObject gameObject) {
		string path = gameObject.transform.name;
		Transform parentTransform = gameObject.transform.parent;

		while (parentTransform != null && parentTransform != this.transform) {
			path = parentTransform.name + "/" + path;
			parentTransform = parentTransform.parent;
		}
		return path;
	}
}
