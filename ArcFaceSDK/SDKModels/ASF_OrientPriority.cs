
namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 人脸检测优先角度结构体，推荐ASF_OP_0_HIGHER_EXT
    /// </summary>
    public enum ASF_OrientPriority
    {
        /// <summary>
        /// 常规预览下正方向
        /// </summary>
        ASF_OP_0_ONLY = 0x1,

        /// <summary>
        /// 基于0°逆时针旋转90°的方向
        /// </summary>
        ASF_OP_90_ONLY = 0x2,

        /// <summary>
        /// 基于0°逆时针旋转270°的方向
        /// </summary>
        ASF_OP_270_ONLY = 0x3,

        /// <summary>
        /// 基于0°旋转180°的方向（逆时针、顺时针效果一样）
        /// </summary>
        ASF_OP_180_ONLY = 0x4,

        /// <summary>
        /// 全角度
        /// </summary>
        ASF_OP_ALL_OUT = 0x5
    }
}
