using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VRVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    [Tooltip("按下空格快进deltatime")]
    public float deltaTime = 20f;
    private string mp4FilePath;
    private bool isVideoPrepared = false; // 标志位，用于判断视频是否已准备好

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
    [Tooltip("文件选择")]
    public mp4filepath videoType = mp4filepath.Invalid;

    public void Awake()
    {
        mp4FilePath = @"E:\Desktop\" + videoType + ".mp4";
    }
    void Start()
    {
        // 指定要播放的视频文件路径
        videoPlayer.url = "file://" + mp4FilePath;
        // 设置循环播放
        videoPlayer.isLooping = true;
        try
        {
            // 准备视频（加载视频文件）
            videoPlayer.Prepare();
        }
        catch (System.Exception)
        {
            Debug.LogError("can't load the video");
            throw;
        }
        // 注册“准备完毕”事件的回调
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void Update()
    {
        if (isVideoPrepared && Input.GetKeyDown(KeyCode.Space))
        {
            // 快进视频deltatime
            videoPlayer.time += deltaTime;
        }
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        // 计算视频和容器的纵横比例

        float videoAspect = (float)videoPlayer.texture.width / videoPlayer.texture.height;
        float containerAspect = rawImage.rectTransform.rect.width / rawImage.rectTransform.rect.height;

        //根据纵横比例调整RawImage的大小和位置
        float scale = videoAspect >= containerAspect ? rawImage.rectTransform.rect.height / videoPlayer.height : rawImage.rectTransform.rect.width / videoPlayer.width;
        rawImage.rectTransform.sizeDelta = new Vector2(videoPlayer.width * scale * (float)0.75, videoPlayer.height * scale);
        Debug.Log("videoPlayer.tex,width" + videoPlayer.texture.width.ToString() + "videoPlayer.wid" + videoPlayer.width.ToString());
        rawImage.rectTransform.anchoredPosition = new Vector2(0, 0);

        // 设置视频渲染目标为RawImage对象
        int targetWidth = (int)videoPlayer.texture.width;
        int targetHeight = (int)videoPlayer.texture.height;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(targetWidth, targetHeight, 24);
        rawImage.texture = videoPlayer.targetTexture;
        // 设置标志位为true，表示视频已准备好
        isVideoPrepared = true;
        // 准备完毕后，开始播放视频
        videoPlayer.Play();
    }

}
