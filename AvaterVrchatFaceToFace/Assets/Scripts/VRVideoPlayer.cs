using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VRVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    [Tooltip("���¿ո���deltatime")]
    public float deltaTime = 20f;
    private string mp4FilePath;
    private bool isVideoPrepared = false; // ��־λ�������ж���Ƶ�Ƿ���׼����

    public enum mp4filepath
    {
        happy = 1,
        sad = 2,
        surprise = 3,
        angry = 4,
        disgust = 5,
        fear = 6,
        Invalid = -1
    }
    [Tooltip("�ļ�ѡ��")]
    public mp4filepath videoType = mp4filepath.Invalid;

    public void Awake()
    {
        mp4FilePath = @"E:\Desktop\" + videoType + ".mp4";
    }
    void Start()
    {
        // ָ��Ҫ���ŵ���Ƶ�ļ�·��
        videoPlayer.url = "file://" + mp4FilePath;
        // ����ѭ������
        videoPlayer.isLooping = true;
        try
        {
            // ׼����Ƶ��������Ƶ�ļ���
            videoPlayer.Prepare();
        }
        catch (System.Exception)
        {
            Debug.LogError("can't load the video");
            throw;
        }
        // ע�ᡰ׼����ϡ��¼��Ļص�
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void Update()
    {
        if (isVideoPrepared && Input.GetKeyDown(KeyCode.Space))
        {
            // �����Ƶdeltatime
            videoPlayer.time += deltaTime;
        }
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        // ������Ƶ���������ݺ����

        float videoAspect = (float)videoPlayer.texture.width / videoPlayer.texture.height;
        float containerAspect = rawImage.rectTransform.rect.width / rawImage.rectTransform.rect.height;

        //�����ݺ��������RawImage�Ĵ�С��λ��
        float scale = videoAspect >= containerAspect ? rawImage.rectTransform.rect.height / videoPlayer.height : rawImage.rectTransform.rect.width / videoPlayer.width;
        rawImage.rectTransform.sizeDelta = new Vector2(videoPlayer.width * scale * (float)0.75, videoPlayer.height * scale);
        Debug.Log("videoPlayer.tex,width" + videoPlayer.texture.width.ToString() + "videoPlayer.wid" + videoPlayer.width.ToString());
        rawImage.rectTransform.anchoredPosition = new Vector2(0, 0);

        // ������Ƶ��ȾĿ��ΪRawImage����
        int targetWidth = (int)videoPlayer.texture.width;
        int targetHeight = (int)videoPlayer.texture.height;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(targetWidth, targetHeight, 24);
        rawImage.texture = videoPlayer.targetTexture;
        // ���ñ�־λΪtrue����ʾ��Ƶ��׼����
        isVideoPrepared = true;
        // ׼����Ϻ󣬿�ʼ������Ƶ
        videoPlayer.Play();
    }

}
