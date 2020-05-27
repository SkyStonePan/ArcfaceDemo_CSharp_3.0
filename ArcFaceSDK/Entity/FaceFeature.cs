
namespace ArcFaceSDK.Entity
{
    /// <summary>
    /// 人脸特征
    /// </summary>
    public class FaceFeature
    {
        /// <summary>
        /// 特征值 byte[]
        /// </summary>
        public byte[] feature { get; set; }

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int featureSize { get; set; }
    }
}
