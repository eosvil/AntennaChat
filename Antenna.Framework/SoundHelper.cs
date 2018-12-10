using Istrib.Sound;
using Istrib.Sound.Formats;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SDK.AntSdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Framework
{
    /// <summary>
    /// 语音帮助类
    /// </summary>
    public class SoundHelper
    {
        #region 字段
        /// <summary>
        /// 音频文件路径
        /// </summary>
        private static string _soundFilePath;

        /// <summary>
        /// 录音
        /// </summary>
        private static Mp3SoundCapture SoundCapture;
        #endregion

        /// <summary>
        /// 录音设备检查
        /// </summary>
        /// <param name="errMsg">错误信息</param>
        /// <returns></returns>
        public static bool CheckDeviceAvailable(ref string errMsg)
        {
            var getDivice = new MMDeviceEnumerator();
            var devices = getDivice.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            if (devices == null)
            {
                errMsg = "检测不到麦克风";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        public static void StartRecord(bool isGroup)
        {
            var filePath = isGroup
                ? publicMethod.localDataPath() + AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode +
                  "\\" + AntSdkService.AntSdkLoginOutput.userId + "\\group\\RecordSound\\"
                : publicMethod.localDataPath() + AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode +
                  "\\" + AntSdkService.AntSdkLoginOutput.userId + "\\personal\\RecordSound\\";
            _soundFilePath = filePath + Guid.NewGuid() + ".mp3";
            //检查文件夹是否存在
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                if(SoundCapture==null) SoundCapture=new Mp3SoundCapture();
                foreach (var device in SoundCaptureDevice.AllAvailable)
                {
                    SoundCapture.CaptureDevice = device;
                }
                SoundCapture.OutputType = Mp3SoundCapture.Outputs.Mp3;
                SoundCapture.WaveFormat = PcmSoundFormat.Pcm44kHz16bitStereo;
                SoundCapture.Mp3BitRate = Mp3BitRate.BitRate32;
                SoundCapture.Start(_soundFilePath); 
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[语音录制失败]："+ex.Message+","+ex.StackTrace);
            }
        }

        /// <summary>
        /// 停止录音并返回音频文件
        /// </summary>
        public static string StopRecord()
        {
            SoundCapture.Stop();
            SoundCapture.Dispose();
            SoundCapture = null;
            if (!File.Exists(_soundFilePath))
            {
                return null;
            }
            var file = new FileInfo(_soundFilePath);
            var size = file.Length;
            return size == 0 ? null : _soundFilePath;
        }
        /// <summary>
        /// 获取mp3时长 整数秒
        /// </summary>
        /// <param name="filePath">mp3路径</param>
        /// <returns></returns>
        public static string GetMp3Time(string filePath)
        {
            AudioFileReader autoFile = new AudioFileReader(filePath);
            autoFile.Dispose();
            return Math.Round(autoFile.TotalTime.TotalSeconds, 0).ToString();
        }
    }
}
