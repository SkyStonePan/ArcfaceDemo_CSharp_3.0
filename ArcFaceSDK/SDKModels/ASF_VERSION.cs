using System;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// SDK版本信息结构体
    /// </summary>
    public struct ASF_VERSION
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public IntPtr Version;

        /// <summary>
        /// 构建日期
        /// </summary>
        public IntPtr BuildDate;

        /// <summary>
        /// Copyright
        /// </summary>
        public IntPtr CopyRight;
    }
}
