using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace OVRServer
{
    public class AllString
    {
        public string faceExp;
        public string eyeGaze;
        public string body;
        public AllString()
        {
            this.body = null;
            this.eyeGaze = null;
            this.faceExp = null;
        }
        public AllString(string faceExp, string eyeGaze, string body)
        {
            this.faceExp = faceExp;
            this.eyeGaze = eyeGaze;
            this.body = body;
        }
        public string GetAllString()
        {
            if (this.body == null || this.eyeGaze == null || this.faceExp == null) return null;
            return this.faceExp + "%" + this.eyeGaze + "%" + this.body;
        }
    }

    public class OVRControllerServer : MonoBehaviour
    {
        /// <summary>
        /// 需要挂载初始化的变量
        /// </summary>
        public OVRFaceExpressions ovrFaceExpressions;
        public OVRBody ovrBody;
        public OVREyeGaze ovrEyeGazeLeft;
        public OVREyeGaze ovrEyeGazeRight;
        // AudioSource组件
        public AudioSource audioSource;
        private AudioClip recordedClip, playClip;
        private const int bufferSize = 1024; // 缓冲区大小
                                            
        [HideInInspector]
        public string filePathFace;
        [HideInInspector]
        public string filePathEyes;
        [HideInInspector]
        public string filePathBody;
        [HideInInspector]
        public string filePathVoice;
        [HideInInspector]
        public string presetEyes;
        [HideInInspector]
        public string presetBody;
        private List<string> faceList;
        private List<string> eyeList;
        private List<string> bodyList;
        /// <summary>
        /// 要发送的和接受的数据
        /// </summary>
        public AllString allStringGet, allStringSend;

        public bool applyResetPose = false;
        private string[] bodyArray, eyesArray;
        private int index = 0;
        /// <summary>
        /// FaceExpressions脸部60多个weights的对应名称
        /// </summary>
        private readonly OVRFaceExpressions.FaceExpression[] interstedFaceExpressions = new OVRFaceExpressions.FaceExpression[] {
        OVRFaceExpressions.FaceExpression.BrowLowererL,
        OVRFaceExpressions.FaceExpression.BrowLowererR,
        OVRFaceExpressions.FaceExpression.CheekPuffL,
        OVRFaceExpressions.FaceExpression.CheekPuffR,
        OVRFaceExpressions.FaceExpression.CheekRaiserL,
        OVRFaceExpressions.FaceExpression.CheekRaiserR,
        OVRFaceExpressions.FaceExpression.CheekSuckL,
        OVRFaceExpressions.FaceExpression.CheekSuckR,
        OVRFaceExpressions.FaceExpression.ChinRaiserB,
        OVRFaceExpressions.FaceExpression.ChinRaiserT,
        OVRFaceExpressions.FaceExpression.DimplerL,
        OVRFaceExpressions.FaceExpression.DimplerR,
        OVRFaceExpressions.FaceExpression.EyesClosedL,
        OVRFaceExpressions.FaceExpression.EyesClosedR,
        OVRFaceExpressions.FaceExpression.EyesLookDownL,
        OVRFaceExpressions.FaceExpression.EyesLookDownR,
        OVRFaceExpressions.FaceExpression.EyesLookLeftL,
        OVRFaceExpressions.FaceExpression.EyesLookLeftR,
        OVRFaceExpressions.FaceExpression.EyesLookRightL,
        OVRFaceExpressions.FaceExpression.EyesLookRightR,
        OVRFaceExpressions.FaceExpression.EyesLookUpL,
        OVRFaceExpressions.FaceExpression.EyesLookUpR,
        OVRFaceExpressions.FaceExpression.InnerBrowRaiserL,
        OVRFaceExpressions.FaceExpression.InnerBrowRaiserR,
        OVRFaceExpressions.FaceExpression.JawDrop,
        OVRFaceExpressions.FaceExpression.JawSidewaysLeft,
        OVRFaceExpressions.FaceExpression.JawSidewaysRight,
        OVRFaceExpressions.FaceExpression.JawThrust,
        OVRFaceExpressions.FaceExpression.LidTightenerL,
        OVRFaceExpressions.FaceExpression.LidTightenerR,
        OVRFaceExpressions.FaceExpression.LipCornerDepressorL,
        OVRFaceExpressions.FaceExpression.LipCornerDepressorR,
        OVRFaceExpressions.FaceExpression.LipCornerPullerL,
        OVRFaceExpressions.FaceExpression.LipCornerPullerR,
        OVRFaceExpressions.FaceExpression.LipFunnelerLB,
        OVRFaceExpressions.FaceExpression.LipFunnelerLT,
        OVRFaceExpressions.FaceExpression.LipFunnelerRB,
        OVRFaceExpressions.FaceExpression.LipFunnelerRT,
        OVRFaceExpressions.FaceExpression.LipPressorL,
        OVRFaceExpressions.FaceExpression.LipPressorR,
        OVRFaceExpressions.FaceExpression.LipPuckerL,
        OVRFaceExpressions.FaceExpression.LipPuckerR,
        OVRFaceExpressions.FaceExpression.LipStretcherL,
        OVRFaceExpressions.FaceExpression.LipStretcherR,
        OVRFaceExpressions.FaceExpression.LipSuckLB,
        OVRFaceExpressions.FaceExpression.LipSuckLT,
        OVRFaceExpressions.FaceExpression.LipSuckRB,
        OVRFaceExpressions.FaceExpression.LipSuckRT,
        OVRFaceExpressions.FaceExpression.LipTightenerL,
        OVRFaceExpressions.FaceExpression.LipTightenerR,
        OVRFaceExpressions.FaceExpression.LipsToward,
        OVRFaceExpressions.FaceExpression.LowerLipDepressorL,
        OVRFaceExpressions.FaceExpression.LowerLipDepressorR,
        OVRFaceExpressions.FaceExpression.MouthLeft,
        OVRFaceExpressions.FaceExpression.MouthRight,
        OVRFaceExpressions.FaceExpression.NoseWrinklerL,
        OVRFaceExpressions.FaceExpression.NoseWrinklerR,
        OVRFaceExpressions.FaceExpression.OuterBrowRaiserL,
        OVRFaceExpressions.FaceExpression.OuterBrowRaiserR,
        OVRFaceExpressions.FaceExpression.UpperLidRaiserL,
        OVRFaceExpressions.FaceExpression.UpperLidRaiserR,
        OVRFaceExpressions.FaceExpression.UpperLipRaiserL,
        OVRFaceExpressions.FaceExpression.UpperLipRaiserR,

    };
        /// <summary>
        /// 眼睛ID
        /// </summary>
        private readonly OVREyeGaze.EyeId[] eyeIds = new OVREyeGaze.EyeId[]{
        OVREyeGaze.EyeId.Left,
        OVREyeGaze.EyeId.Right,
    };
        
        string faceExpressionWeightStr;
        string eyeGazeStr;
        string bodyStr;

        private void Awake()
        {
            faceList = new List<string>();
            eyeList = new List<string>();
            bodyList = new List<string>();
            if (applyResetPose)
            {
                try
                {
                    eyesArray = File.ReadAllLines(presetEyes);
                    bodyArray = File.ReadAllLines(presetBody);
                }
                catch (FileNotFoundException)
                {
                    Debug.LogError("FileNotFoundException");
                }
            }
            
        }

        // Start is called before the first frame update
        void Start()
        {
            allStringGet = new AllString();
            allStringSend = new AllString();
        }

        // Update is called once per frame
        void Update()
        {
            //采集
            if (ovrFaceExpressions.ValidExpressions)
            {
                //记载佩戴头显本人的数据,无论是客户端还是服务器
                GetFaceExpression(interstedFaceExpressions, out faceExpressionWeightStr);;
                GetEyeGaze(out eyeGazeStr);
                GetBodyState(out bodyStr);
                //要发送的文本
                allStringSend = new AllString(faceExpressionWeightStr, eyeGazeStr, bodyStr);

                // 将录音数据存储到缓冲区
                if (!Microphone.IsRecording(null))
                {
                    // 初始化录音设备并开始录音
                    recordedClip = Microphone.Start(null, false, 240, 44100);
                }
                else 
                {
                    float[] buffer = new float[bufferSize];
                    int position = Microphone.GetPosition(null);
                    recordedClip.GetData(buffer, position);
                }
            }
          
        }

        private void FixedUpdate()
        {
            if (applyResetPose)
            {
                index++;
                if(index == Math.Min(bodyArray.Length, eyesArray.Length))
                {
                    index = 0;
                }
            }
            if (faceExpressionWeightStr != null)
            {
                faceList.Add(faceExpressionWeightStr);
                eyeList.Add(eyeGazeStr);
                bodyList.Add(bodyStr);
            }

        }

        //destroy
        void OnDestroy()
        {

            StopRecordVoice();
            for (int i = 0; i < Math.Min(Math.Min(faceList.Count, eyeList.Count), bodyList.Count); i++)
            {
                FileWriter(faceList[i], filePathFace);
                if (!applyResetPose)
                {
                    FileWriter(eyeList[i], filePathEyes);
                    FileWriter(bodyList[i], filePathBody);
                }
                
            }

        }


        /// <summary>
        /// 脸部数据获取
        /// </summary>
        /// <param name="interstedFaceExpressions"></param>
        /// <param name="faceExpressionWeight"></param>
        /// <returns></returns>
        public bool GetFaceExpression(OVRFaceExpressions.FaceExpression[] interstedFaceExpressions, out string faceExpressionWeight)
        {
            faceExpressionWeight = null;
            foreach (OVRFaceExpressions.FaceExpression value in interstedFaceExpressions)
            {
                // Returns weight of expression ranged between 0.0 to 100.0.
                if (ovrFaceExpressions.TryGetFaceExpressionWeight(value, out float weight))
                {
                    string expressionWeight = "(" + value.ToString() + "," + ((float)Math.Round(weight, 6)).ToString() + ")";
                    if (!value.Equals(OVRFaceExpressions.FaceExpression.UpperLipRaiserR))
                    {
                        expressionWeight += ';';
                    }
                    faceExpressionWeight += expressionWeight;
                }
            }
            //Debug.Log("FaceExpression" + faceExpressionWeight);
            return true;
        }

        /// <summary>
        /// 左右眼球的位置和看向获取
        /// </summary>
        /// <param name="EyeGaze"></param>
        /// <returns></returns>
        public bool GetEyeGaze(out string EyeGaze)
        {
            EyeGaze = null;
            if (ovrEyeGazeLeft.TryGetEyeGazeState(eyeIds[0], out Vector3 position, out Quaternion rotation))
            {
                EyeGaze = position.ToString() + ";" + rotation.ToString();
            }
            if (ovrEyeGazeRight.TryGetEyeGazeState(eyeIds[1], out position, out rotation))
            {
                string tmpGaze = "/" + position.ToString() + ";" + rotation.ToString();
                EyeGaze += tmpGaze;
            }
            //Debug.Log("EyeGaze" + EyeGaze);
            return true;
        }

        /// <summary>
        /// 得到身体追踪的pos和rot
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public bool GetBodyState(out string body)
        {
            body = null;
            if (ovrBody.TryGetBodyState(out List<OVRBody.PosAndRot> pos))
            {
                int count = 0;
                foreach (OVRBody.PosAndRot posAndRot in pos)
                {
                    if (count != 0) body += "/";
                    count++;
                    string tmpBody = String.Format("{0};{1}", posAndRot.vector3F.ToString(), posAndRot.Quatf.ToString());
                    body += tmpBody;
                }
            }
            //Debug.Log("BodyState");
            return true;
        }

        

        /// <summary>
        /// 面部_currentFaceState改变
        /// </summary>
        /// <param name="_currentFaceState"></param>
        /// <param name="weight"></param>
        /// <param name="faceExpression"></param>
        /// <returns></returns>
        public bool TrySetFaceExpression(ref OVRPlugin.FaceState _currentFaceState)
        {
            string fileBuffer = allStringGet.faceExp;
            string[] singleFaceExpression = fileBuffer.Split(';');
            int count = 0;
            foreach (string value in singleFaceExpression)
            {
                OVRFaceExpressions.FaceExpression faceExpression = interstedFaceExpressions[count++];
                float weight = float.Parse(value.Replace("(", "").Replace(")", "").Split(",")[1]);
                if ((int)faceExpression < _currentFaceState.ExpressionWeights.Length)
                {
                    _currentFaceState.ExpressionWeights[(int)faceExpression] = weight;
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 眼部追踪的改变
        /// </summary>
        /// <param name="indexOfFrame"></param>
        /// <param name="ApplyPosition"></param>
        /// <param name="ApplyRotation"></param>
        /// <param name="vector"></param>
        /// <param name="quaternion"></param>
        /// <param name="eye"></param>
        /// <returns></returns>
        public bool TrySetEyeGaze(bool ApplyPosition, bool ApplyRotation, ref Vector3 vector, ref Quaternion quaternion, OVREyeGaze.EyeId eye)
        {
            string fileBuffer = allStringGet.eyeGaze;
            if (applyResetPose)
            {
                fileBuffer = eyesArray[index];
            }
            string[] singleEyeState = fileBuffer.Split('/');
            //得到左/右眼睛的存储数据
            string thisEyeState = singleEyeState[(int)eye];
            string position = thisEyeState.Split(";")[0];
            string rotation = thisEyeState.Split(";")[1];
            float xPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[0]);
            float yPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[1]);
            float zPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[2]);
            float xRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[0]);
            float yRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[1]);
            float zRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[2]);
            float wRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[3]);
            //如果apply为真则更改
            if (ApplyPosition)
            {
                vector = new Vector3(xPosition, yPosition, zPosition);
            }
            if (ApplyRotation)
            {
                quaternion = new Quaternion(xRotation, yRotation, zRotation, wRotation);
            }
            return true;
        }

        /// <summary>
        /// 身体pos和rot的改变
        /// </summary>
        /// <param name="indexOfFrame"></param>
        /// <param name="_bodyState"></param>
        /// <returns></returns>
        public bool TrySetBody(ref OVRPlugin.BodyState _bodyState)
        {
            string fileBuffer = allStringGet.body;
            if (applyResetPose)
            {
                fileBuffer = bodyArray[index];
            }
            //Debug.Log("身体相关" + Time.time + fileBuffer);
            string[] singleBodyState = fileBuffer.Split('/');
            for (var i = 0; i < _bodyState.JointLocations.Length; i++)
            {
                //长度不等存储的数据数量与要分配的不同
                if (_bodyState.JointLocations.Length != singleBodyState.Length)
                    return false;
                else
                {
                    string position = singleBodyState[i].Split(";")[0];
                    string rotation = singleBodyState[i].Split(";")[1];
                    float xPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[0]);
                    float yPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[1]);
                    float zPosition = float.Parse(position.Replace("(", "").Replace(")", "").Split(",")[2]);
                    float xRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[0]);
                    float yRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[1]);
                    float zRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[2]);
                    float wRotation = float.Parse(rotation.Replace("(", "").Replace(")", "").Split(",")[3]);

                    if (_bodyState.JointLocations[i].OrientationValid)
                    {
                        OVRPlugin.Quatf quatf = new() { w = wRotation, x = xRotation, y = yRotation, z = zRotation };
                        _bodyState.JointLocations[i].Pose.Orientation = quatf;
                    }
                    if (_bodyState.JointLocations[i].PositionValid)
                    {
                        OVRPlugin.Vector3f vector3F = new() { x = xPosition, y = yPosition, z = zPosition };
                        _bodyState.JointLocations[i].Pose.Position = vector3F;
                    }
                }

            }
            return true;

        }

        /// <summary>
        /// 文件写入
        /// </summary>
        /// <param name="str"></param>
        /// <param name="filePath"></param>
        public void FileWriter(string str, string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            // 使用 StreamWriter 打开文件，并写入文本
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(str);
                //Debug.Log("OVRController" + String);
            }
        }

        /// <summary>
        /// .wav文件写入
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
        {
            int sampleCount = samples.Length;
            int subchunk2Size = sampleCount * channels * sizeof(short);
            int chunkSize = 36 + subchunk2Size;

            using (MemoryStream stream = new MemoryStream())
            {
                // 写入.wav文件头
                stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                stream.Write(System.BitConverter.GetBytes(chunkSize), 0, 4);
                stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
                stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
                stream.Write(System.BitConverter.GetBytes(16), 0, 4); // subchunk1Size
                stream.Write(System.BitConverter.GetBytes((short)1), 0, 2); // audioFormat (PCM)
                stream.Write(System.BitConverter.GetBytes((short)channels), 0, 2); // numChannels
                stream.Write(System.BitConverter.GetBytes(sampleRate), 0, 4); // sampleRate
                stream.Write(System.BitConverter.GetBytes(sampleRate * channels * sizeof(short)), 0, 4); // byteRate
                stream.Write(System.BitConverter.GetBytes((short)(channels * sizeof(short))), 0, 2); // blockAlign
                stream.Write(System.BitConverter.GetBytes((short)16), 0, 2); // bitsPerSample
                stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
                stream.Write(System.BitConverter.GetBytes(subchunk2Size), 0, 4);

                // 写入音频数据
                foreach (float sample in samples)
                {
                    short sampleValue = (short)(sample * 32767);
                    stream.Write(System.BitConverter.GetBytes(sampleValue), 0, 2);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 停止声音记录
        /// </summary>
        private void StopRecordVoice()
        {
            Microphone.End(null); // 停止录音

            // 将录音数据转换为字节数组
            float[] samples = new float[recordedClip.samples];
            recordedClip.GetData(samples, 0);
            // 创建一个字节数组用于存储.wav文件头和音频数据
            byte[] wavData = ConvertToWav(samples, recordedClip.channels, recordedClip.frequency);
            // 保存为.wav文件
            File.WriteAllBytes(filePathVoice, wavData);
            Debug.Log("Saved wav file: " + filePathVoice);
        }

        /// <summary>
        /// wav文件加载
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private AudioClip LoadWavFile(string filePath)
        {
            // 读取音频文件数据
            byte[] audioData = System.IO.File.ReadAllBytes(filePath);
            // 将字节数组转换为16位带符号整数数组
            short[] audioSamples = new short[audioData.Length / 2];
            Buffer.BlockCopy(audioData, 0, audioSamples, 0, audioData.Length);
            // 将带符号整数数组归一化为浮点数数组
            float[] normalizedSamples = new float[audioSamples.Length];
            const float conversionFactor = 1.0f / 32768.0f; // 归一化因子
            for (int i = 0; i < audioSamples.Length; i++)
            {
                normalizedSamples[i] = audioSamples[i] * conversionFactor;
            }
            // 创建一个新的AudioClip并加载音频数据
            //最后一个参数布尔值，指示是否将AudioClip设置为流式播放。如果设置为true，则可以在音频仍在加载时播放，适用于较大的音频文件。
            AudioClip audioClip = AudioClip.Create("wavClip", normalizedSamples.Length, 1, 44100, false);
            audioClip.SetData(normalizedSamples, 0);

            return audioClip;
        }

        /// <summary>
        /// wav文件播放
        /// </summary>
        /// <param name="audioClip"></param>
        public void PlayAudioClip()
        {
            Debug.Log("Video");
            // 设置AudioSource的音频剪辑
            audioSource.clip = playClip;
            // 播放音频
            audioSource.Play();
        }

    }


}