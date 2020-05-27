using ArcFaceSDK.SDKModels;

namespace ArcSoftFace.Entity
{
    /// <summary>
    /// 视频检测缓存实体类
    /// </summary>
    public class FaceTrackUnit
    {
        /// <summary>
        /// 人脸框
        /// </summary>
        public MRECT Rect { get; set; }

        /// <summary>
        /// 文本信息
        /// </summary>
        public string message = string.Empty;

        /// <summary>
        /// 活体
        /// </summary>
        public int liveness = -1;
    }
}
