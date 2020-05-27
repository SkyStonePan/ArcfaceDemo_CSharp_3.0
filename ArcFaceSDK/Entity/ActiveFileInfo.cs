namespace ArcFaceSDK.Entity
{
    /// <summary>
    /// 激活文件信息
    /// </summary>
    public class ActiveFileInfo
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public string startTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string endTime { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public string platform { get; set; }

        /// <summary>
        /// sdk类型
        /// </summary>
        public string sdkType { get; set; }

        /// <summary>
        /// APPID
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// SDKKEY
        /// </summary>
        public string sdkKey { get; set; }

        /// <summary>
        /// SDK版本号
        /// </summary>
        public string sdkVersion { get; set; }

        /// <summary>
        /// 激活文件版本号
        /// </summary>
        public string fileVersion { get; set; }
    }
}
