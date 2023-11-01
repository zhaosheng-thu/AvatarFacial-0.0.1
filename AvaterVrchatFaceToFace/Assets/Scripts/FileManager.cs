using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using OVRServer;
using Oculus.Movement.Tracking;

public class FileManager : MonoBehaviour
{
    public FileSelector fileSelector;
    public OVRControllerServer OVRController;
    private string filepathFace, filepathEyes, filepathBody, filepathVoice;

    [Tooltip("���Ե�����")]
    public string nameOfExp;
    [Tooltip("ģʽѡ��")]
    public TypeOfExp typeOfExp;
    [Tooltip("����ѡ��")]
    public VRVideoPlayer.mp4filepath videoType;
    private void Awake()
    {
        DateTime currentDate = DateTime.Now;
        int currentMonth = currentDate.Month;
        // ��ȡ��ǰ��
        int currentDay = currentDate.Day;
        int currentHour = currentDate.Hour;
        int currentMin = currentDate.Minute;
        int currentSec = currentDate.Second;
        string timeState = string.Format("MM{0}DD{1}_{2}_{3}_{4}", currentMonth, currentDay, currentHour, currentMin, currentSec);
        Debug.Log("time" + timeState);

        //����ģʽ���ɼ����Թۿ�ʱ�ı������chatģʽ��������ʱ�ع�filepath
        nameOfExp = this.GetComponent<BlendshapeModifier>().GetGlobalMultiplier().ToString() + "times_" + nameOfExp;

        filepathFace = Path.Combine(Application.dataPath, "File/Face/face_" + nameOfExp + "_" + videoType + "_" + timeState + ".txt");
        filepathEyes = Path.Combine(Application.dataPath, "File/Eye/eye_" + nameOfExp + "_" + videoType + "_" + timeState + ".txt");
        filepathBody = Path.Combine(Application.dataPath, "File/Body/body_" + nameOfExp + "_" + videoType + "_" + timeState + ".txt");
        filepathVoice = Path.Combine(Application.dataPath, "File/Voice/voice_" + nameOfExp + "_" + videoType + "_" + timeState + ".wav");

        OVRController.filePathFace = filepathFace;
        OVRController.filePathEyes = filepathEyes;
        OVRController.filePathBody = filepathBody;
        OVRController.filePathVoice = filepathVoice;
        if (OVRController.applyResetPose)
        {
            OVRController.presetBody = fileSelector.filepathBody;
            OVRController.presetEyes = fileSelector.filepathEyes;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public enum TypeOfExp
    {
        Invalid = -1,
        Videoplayer = 1,
        Chat = 2
    }

}
