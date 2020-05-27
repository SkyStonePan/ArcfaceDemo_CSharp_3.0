using System;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 人脸框信息结构体
    /// </summary>
    public struct MRECT
    {
        /// <summary>
        /// 左侧坐标
        /// </summary>
        public int left;
        
        /// <summary>
        /// 上侧坐标
        /// </summary>
        public int top;
        
        /// <summary>
        /// 右侧坐标
        /// </summary>
        public int right;

        /// <summary>
        /// 下侧坐标
        /// </summary>
        public int bottom;
    }
}
