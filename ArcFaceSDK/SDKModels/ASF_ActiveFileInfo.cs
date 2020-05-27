using System;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 激活文件信息
    /// </summary>
    public struct ASF_ActiveFileInfo
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public IntPtr startTime;

        /// <summary>
        /// 截止时间
        /// </summary>
        public IntPtr endTime;

        /// <summary>
        /// 平台
        /// </summary>
        public IntPtr platform;

        /// <summary>
        /// sdk类型
        /// </summary>
        public IntPtr sdkType;

        /// <summary>
        /// APPID
        /// </summary>
        public IntPtr appId;

        /// <summary>
        /// SDKKEY
        /// </summary>
        public IntPtr sdkKey;

        /// <summary>
        /// SDK版本号
        /// </summary>
        public IntPtr sdkVersion;

        /// <summary>
        /// 激活文件版本号
        /// </summary>
        public IntPtr fileVersion;
    }
}
