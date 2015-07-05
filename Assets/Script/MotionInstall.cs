using UnityEngine;
using System.Collections;

public class MotionInstall : MonoBehaviour {
	
	#if UNITY_STANDALONE_WIN

	void Start () {
	
	}

	void Update () {
	
	}
	/// <summary>
	///  モーションインストーラ呼び出しメソッド
	/// </summary>
	/// <param name="jsonPath">モーションインストールを行いたいJSONファイルのパス</param>
	/// <param name="fileName">JSONファイル名</param>
	public void StartMotionInstallApp(string jsonPath, string fileName) {
		// プロセス起動の各種設定．（MotionInstallerの仕様上第一引数：JSONファイルパス，第二引数：ファイル名である）
		System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo ();
		//info.FileName = "c:/Git/plen2__ble_motion_installer_gui/bin/Debug/MotionInstaller.exe";
		info.FileName = Application.dataPath + "/Plugins/Windows/MotionInstaller.exe";
		info.Arguments = jsonPath + " " + fileName;
		// モーションインストーラ起動
		System.Diagnostics.Process.Start (info);
	}
	#else
	void Start() {
	}
	void Update() {
	}

	public void StartMotionInstallApp(string jsonPath, string fileName) {
	}

	#endif


}
