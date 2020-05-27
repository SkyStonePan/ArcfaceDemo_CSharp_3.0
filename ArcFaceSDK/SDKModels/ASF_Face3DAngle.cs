using System;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 3D人脸角度检测结构体，可参考https://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=1459&page=1&extra=&_dsign=fd9e1a7a
    /// </summary>
    public struct ASF_Face3DAngle
    {
        /// <summary>
        /// 横滚角
        /// </summary>
        public IntPtr roll;

        /// <summary>
        /// 偏航角
        /// </summary>
        public IntPtr yaw;

        /// <summary>
        /// 俯仰角
        /// </summary>
        public IntPtr pitch;

        /// <summary>
        /// 是否检测成功，0成功，其他为失败
        /// </summary>
        public IntPtr status;

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int num;
    }
}
