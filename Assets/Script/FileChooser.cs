using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.IO;

public delegate void OpenFileDialogFinishedEventHander(string path, string errorMessage);
public delegate void SaveFileDialogFinishedEventHander(string path, string errorMessage);

public class FileChooser : MonoBehaviour
{
	/// <summary>
	/// OpenFileDialog終了イベント
	/// </summary>
	public event OpenFileDialogFinishedEventHander OpenFileDialogFinished;
	/// <summary>
	/// SaveFileDialog終了イベント
	/// </summary>
	public event SaveFileDialogFinishedEventHander SaveFileDialogFinished;

	// OS X (mac)でのファイルダイアログ実装
	#if UNITY_STANDALONE_OSX
	/// <summary>
	/// applescriptのACTIVATE命令
	/// Note... この命令を呼び出さないとchoose fileが動作しない場合がある
	/// </summary>
	public const string FORCE_ACTIVATE =
		"-e 'tell application \"System Events\" to activate'";
	/// <summary>
	/// ファイル選択ダイアログ表示・選択ファイルパス保存スクリプト(applescript記述)
	/// Note...Processが日本語の戻り値を受け取ると必ず文字化けする．
	///        従ってchoose fileで取得したファイルパスをURLエンコード（ php -r "echo urlencode("XXX") ）した状態で受け取る．
	/// </summary>
	public const string SCRIPT_CHOOSE_FILE = 
		"-e 'do shell script \"php -r \\\"echo urlencode(\\\\\\\"\" & ((POSIX path of (choose file)) as text) &  \"\\\\\\\");\\\"\"'";
	/// <summary>
	/// 保存先フォルダ選択・保存ファイル名取得，保存ファイルパス保存スクリプト(applescript記述)
	/// </summary>
	public const string SCRIPT_SAVE_FILE =
		"-e 'do shell script \"php -r \\\"echo urlencode(\\\\\\\"\" & ((POSIX path of (choose file name)) as text) &  \"\\\\\\\");\\\"\"'";


	void Start() {
	}

	/// <summary>
	/// OpenFileDialog呼び出しメソッド
	/// 終了通知は"OpenFileDialogFinished"イベントにて行う
	/// </summary>
	public void OpenFileDialog_Show() {
		// getPathDialogを実行
		// Note...プログラムが固まるのを防止するためコルーチンにて実行
		StartCoroutine (getPathDialog (SCRIPT_CHOOSE_FILE, (string path, string errorMessage) => {
			// 終了イベント発生
			OpenFileDialogFinished(path, errorMessage);
		}));
	}
	/// <summary>
	/// SaveFileDialog呼び出しメソッド
	/// 終了通知は"SaveFileDialogFinished"イベントにて行う
	/// </summary>
	public void SaveFileDialog_Show() {
		// getPathDialogを実行
		// Note...プログラムが固まるのを防止するためコルーチンにて実行
		StartCoroutine (getPathDialog (SCRIPT_SAVE_FILE, (string path, string errorMessage) => {
			// 終了イベント発生
			SaveFileDialogFinished(path, errorMessage);
		}));
	}
	/// <summary>
	/// getPathDialogメソッド (コルーチン実行)
	/// </summary>
	/// <param name="script">スクリプト(AppleScript)</param>
	/// <param name="argv">スクリプト引数</param>
	/// <param name="onClosed">終了通知</param>
	public IEnumerator getPathDialog (string script, System.Action<string, string> onClosed)
	{
		Process fileDialog = new Process ();				// fileDialogプロセス
		StringBuilder path = new StringBuilder ();			// 取得したファイルパス
		StringBuilder errorMessage = new StringBuilder ();	// エラーメッセージ
		// プロセスの初期設定
		fileDialog.StartInfo = new ProcessStartInfo ()
		{
			FileName = "osascript",			// 実行app (osascript : Applescriptを実行するmac標準app)
			Arguments = script,				// 実行appへの引数 (スクリプト自体)
			CreateNoWindow = true,			// terminalは非表示にする
			UseShellExecute = false,		// シェル機能を使用しない
			RedirectStandardOutput = true,	// スクリプトからの戻り値を受信する
			RedirectStandardError = true,	// スクリプトのエラーメッセージを受信する
		};
		// スクリプト戻り値イベント設定
		fileDialog.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>  {
			// 戻り値がないときも呼び出される場合があるので，戻り値があるか判定
			if(string.IsNullOrEmpty(e.Data) == false) {
				// 戻り値はURLエンコードされているのでデコード
				path.Append(WWW.UnEscapeURL(e.Data));
				UnityEngine.Debug.Log(WWW.UnEscapeURL(e.Data));
			}
		};
		// スクリプトエラーメッセージ受信イベント設定
		fileDialog.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
			// エラーがないときも呼び出される場合があるので，エラーメッセージがあるか判定
			if(string.IsNullOrEmpty(e.Data) == false) {
				errorMessage.Append(e.Data);
				UnityEngine.Debug.Log(e.Data);
			}
		};
		// プロセススタート．
		fileDialog.Start ();
		fileDialog.BeginOutputReadLine ();
		fileDialog.BeginErrorReadLine ();

		// 1フレーム待機し，その後applescriptのACTIVATE命令を実行
		yield return null;
		Process.Start (new ProcessStartInfo () { FileName = "osascript", Arguments = FORCE_ACTIVATE });
		// fileDialogが終了するまで待機
		while (fileDialog.HasExited == false) {
			yield return null;
		}
		// プロセスを終了・破棄
		fileDialog.Close ();
		fileDialog.Dispose ();
		// 終了通知
		onClosed.Invoke (path.ToString (), errorMessage.ToString());
	}

	// WindwosでのFileDialog実装(.NET (Mono))
	#elif UNITY_STANDALONE_WIN
	void Start()
	{
        //nop
	}
	/// <summary>
	/// OpenFileDialog呼び出しメソッド
	/// 終了通知は"OpenFileDialogFinished"イベントにて行う
	/// </summary>
	public void OpenFileDialog_Show() {
		StartCoroutine (getFilePath(true, (string path, string errorMassage) => {
			OpenFileDialogFinished(path, errorMassage);
		}));
	}
	/// <summary>
	/// SaveFileDialog呼び出しメソッド
	/// 終了通知は"SaveFileDialogFinished"イベントにて行う
	/// </summary>
	public void SaveFileDialog_Show() {
		StartCoroutine (getFilePath (false, (string Path, string errorMassage) => {
			SaveFileDialogFinished (Path, errorMassage);
		}));
	}
	/// <summary>
	/// FileDialog呼び出しメソッド (コルーチン実行)
	/// </summary>
	/// <param name="isModeOpen">true...OpenFileDialog, false...SaveFileDialog</param>
	/// <param name="onClosed">終了通知</param>
	public IEnumerator getFilePath(bool isModeOpen, System.Action<string, string> onClosed) {
		System.Windows.Forms.FileDialog fileDialog;
		// fileDialogインスタンス生成（isModeOpen : true...Open, false...Save）
		if (isModeOpen == true) {
			fileDialog = new System.Windows.Forms.OpenFileDialog ();
		} else {
			fileDialog = new System.Windows.Forms.SaveFileDialog ();
		}

		fileDialog.Filter =  "JSON files (*.json)|*.json|All files (*.*)|*.*";
		// 1フレーム待機
		yield return null;
		// fileDialog表示．OKボタンが押された場合取得したfilePathを戻り値として返す
		// Cancelされた場合エラーを返す
		if (fileDialog.ShowDialog () == System.Windows.Forms.DialogResult.OK) {
			onClosed.Invoke (fileDialog.FileName, "");
		} else {
			onClosed.Invoke ("", "Cancel");
		}
	}	
	#elif UNITY_STANDALONE_LINUX
	void Start()
	{
	}

	public void OpenFileDialog_Show() {
	OpenFileDialogFinished("", "未実装の機能です”);
	}

	public void SaveFileDialog_Show() {

	OpenFileDialogFinished("", "未実装の機能です”);
	}
	#else

	void Start()
	{
	}

	public void OpenFileDialog_Show() {
		OpenFileDialogFinished ("", "未実装の機能です");
	}

	public void SaveFileDialog_Show() {
		OpenFileDialogFinished ("", "未実装の機能です");
	}

	#endif
}