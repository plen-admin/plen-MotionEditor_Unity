using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.IO;

//public delegate void OpenFileDialogFinishedEventHander(string path, string errorMessage);
//public delegate void SaveFileDialogFinishedEventHander(string path, string errorMessage);

public class FileChooserOrg : MonoBehaviour
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
	///        そのため選択ファイルのパスをファイルに書き出す方法で実装を行った．
	/// </summary>
	public const string SCRIPT_CHOOSE_FILE = 
		"on run argv\n" +
		"set fpath to choose file\n" +
		"set savepath to POSIX path of argv\n" +
		"try\n" +
		"write ((POSIX path of fpath) as text) to savepath\n" +
		"do shell script \"mv \" & savepath & \" \" & savepath & \".org\"\n" +
		"do shell script \"iconv -f Shift_JIS -t UTF-16 \" & savepath & \".org > \" & savepath\n" +
		"do shell script \"rm \" & savepath & \".org\"\n" +
		"end try\n" + 
		"end run";
	/// <summary>
	/// 保存先フォルダ選択・保存ファイル名取得，保存ファイルパス保存スクリプト(applescript記述)
	/// </summary>
	public const string SCRIPT_SAVE_FILE =
		"on run argv\n" +
		"set fpath to choose file name\n" + 
		"set savepath to POSIX path of argv\n" + 
		"try\n" + 
		"write ((POSIX path of fpath) as text) to savepath\n" +
		"do shell script \"mv \" & savepath & \" \" & savepath & \".org\"\n" +
		"do shell script \"iconv -f Shift_JIS -t UTF-16 \" & savepath & \".org > \" & savepath\n" +
		"do shell script \"rm \" & savepath & \".org\"\n" +
		"end try\n" + 
		"end run";
	/// <summary>
	/// スクリプトファイル保存先パス
	/// </summary>
	string scriptPath = "./tmp/script.scpt";
	/// <summary>
	/// スクリプト戻り値ファイル保存先パス
	/// </summary>
	string scriptReturnValuePath = "./tmp/chooseFile.txt";

	void Start() {
	}

	/// <summary>
	/// OpenFileDialog呼び出しメソッド
	/// 終了通知は"OpenFileDialogFinished"イベントにて行う
	/// </summary>
	public void OpenFileDialog_Show() {
		string striptFullPath = scriptFileCreate (SCRIPT_CHOOSE_FILE);
		string savePath = scriptReturnValueFileCreate ();


		// getPathDialogを実行
		// Note...プログラムが固まるのを防止するためコルーチンにて実行
		StartCoroutine (getPathDialog (striptFullPath, savePath, (string path, string errorMessage) => {
			// 終了イベント発生
			OpenFileDialogFinished(path, errorMessage);
		}));
	}
	/// <summary>
	/// SaveFileDialog呼び出しメソッド
	/// 終了通知は"SaveFileDialogFinished"イベントにて行う
	/// </summary>
	public void SaveFileDialog_Show() {
		string striptFullPath = scriptFileCreate (SCRIPT_SAVE_FILE);
		string saveFullPath = scriptReturnValueFileCreate ();

		// getPathDialogを実行
		// Note...プログラムが固まるのを防止するためコルーチンにて実行
		StartCoroutine (getPathDialog (striptFullPath, saveFullPath, (string path, string errorMessage) => {
			// 終了イベント発生
			SaveFileDialogFinished(path, errorMessage);
		}));
	}

	/// <summary>
	/// スクリプトファイル作成メソッド
	/// </summary>
	/// <returns>作成先の絶対パス</returns>
	/// <param name="script">スクリプトファイルに書き込みたいスクリプト</param>
	private string scriptFileCreate(string script) {
		// スクリプトファイル作成（作成済みであれば自動的に元ファイルが削除される）
		using (FileStream stream = File.Create(scriptPath)) {
			using (StreamWriter writer = new StreamWriter (stream)) {
				// スクリプト書き込み
				writer.WriteLine (script);
				writer.Close ();
			}
		}
		return Path.GetFullPath (scriptPath);
	}
	/// <summary>
	/// スクリプト戻り値ファイル作成メソッド
	/// </summary>
	/// <returns>作成先の絶対パス</returns>
	private string scriptReturnValueFileCreate() {
		// スクリプト戻り値ファイル作成（作成済みであれば自動的に元ファイルが削除される）
		using (System.IO.FileStream stream = System.IO.File.Create (scriptReturnValuePath)) {
			stream.Close ();
		}
		return Path.GetFullPath (scriptReturnValuePath);
	}
	/// <summary>
	/// getPathDialogメソッド (コルーチン実行)
	/// </summary>
	/// <param name="scriptPath">Script絶対パス</param>
	/// <param name="argv">スクリプト引数</param>
	/// <param name="onClosed">終了通知</param>
	public IEnumerator getPathDialog (string scriptPath, string argv, System.Action<string, string> onClosed)
	{
		Process fileDialog = new Process ();				// fileDialogプロセス
		StringBuilder path = new StringBuilder ();			// 取得したファイルパス
		StringBuilder errorMessage = new StringBuilder ();	// エラーメッセージ
		// プロセスの初期設定
		fileDialog.StartInfo = new ProcessStartInfo ()
		{
			FileName = "osascript",					// 実行app (osascript : Applescriptを実行するmac標準app)
			Arguments = scriptPath + " " + argv,	// 実行appへの引数 (osascriptの第一引数がスクリプトのパス．第二引数がスクリプトへの引数）
			CreateNoWindow = true,					// terminalは非常にする
			UseShellExecute = false,				// シェル機能を使用しない
			RedirectStandardError = true,			// スクリプトのエラーメッセージを受信する
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
		fileDialog.BeginErrorReadLine ();

		// 1フレーム待機し，その後applescriptのACTIVATE命令を実行
		yield return null;
		Process.Start (new ProcessStartInfo () { FileName = "osascript", Arguments = FORCE_ACTIVATE });

		// fileDialogが終了するまで待機
		while (fileDialog.HasExited == false) {
			yield return null;
		}

		// スクリプト戻り値ファイルが正常に保存されているか判定
		FileInfo fi = new FileInfo (scriptReturnValuePath);
		if (fi.Length > 0 && errorMessage.Length == 0) {
			// 戻り値ファイルから，選択されたファイルパスを取得
			using (StreamReader sr = new StreamReader (scriptReturnValuePath, Encoding.Unicode)) {
				if (sr != null) {
					path.Append (sr.ReadToEnd ());
				} 
			}
		} else {
			path.Append ("");
		}
		// プロセスを終了・破棄
		fileDialog.Close ();
		fileDialog.Dispose ();
		// 終了通知
		onClosed.Invoke (path.ToString (), errorMessage.ToString());
	}
	#else


	void Start()
	{
	}

	public void OpenFileDialog_Show() {
		OpenFileDialogFinished("", "未実装の機能です”);
	}

	public void SaveFileDialog_Show() {

		OpenFileDialogFinished("", "未実装の機能です”);
	}

	#endif
}