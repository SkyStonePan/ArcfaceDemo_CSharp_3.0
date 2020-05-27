namespace ArcFaceSDK.Utils
{
    /// <summary>
    /// 接口封装时定义的错误码
    /// </summary>
    public class ErrorCodeUtil
    {
        /// <summary>
        /// image不能为空
        /// </summary>
        public const int IMAGE_IS_NULL = -1001;

        /// <summary>
        /// 图像数据读取失败
        /// </summary>
        public const int IMAGE_DATA_READ_FAIL = -1002;

        /// <summary>
        /// multiFaceInfo 不能为空
        /// </summary>
        public const int MULPTIFACEINFO_IS_NULL = -1101;

        /// <summary>
        /// faceIndex 不合法
        /// </summary>
        public const int FACEINDEX_INVALID = -1102;

        /// <summary>
        /// faceFeature1或faceFeature2 不能为空
        /// </summary>
        public const int FEATURE_IS_NULL = -1103;
    }
}
