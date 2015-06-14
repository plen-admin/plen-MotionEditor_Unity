using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System.Collections;
using System.IO;

public class MenuGUI : MonoBehaviour {
	public GameObject labelDebug;
	private Vector2 clickPos;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BtnOpen_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();

//		string str = EditorUtility.OpenFilePanel ("File Select", "", "");
//		labelDegugText.text = str;
	}
	public void BtnSave_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnSave Click";
	}
	public void BtnNew_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnNew Click";
	}
	public void BtnDefaultPos_Click(GameObject motionDataObject)
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnDefaultPos Click";

		MotionData motionData = motionDataObject.GetComponent<MotionData> ();
		motionData.FrameInitialize (motionData.index);
	}
	public void BtnPlay_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnPlay Click";
	}
	public void BtnStop_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnStop Click";
	}
	public void BtnBackFrame_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnBackFrame Click";
	}
	public void BtnForwardFrame_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnForwardFrame Click";
	}
	public void BtnConnect_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnConnect Click";
	}
	public void BtnMS_Click()
	{
		Text labelDegugText = labelDebug.GetComponent<Text> ();
		labelDegugText.text = "BtnMS Click";
	}

}
