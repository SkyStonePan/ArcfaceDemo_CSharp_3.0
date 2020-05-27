namespace ArcFaceSDK.Entity
{
    public class LivenessInfo
    {
        /// <summary>
        /// 是否是真人 
        /// 0：非真人；1：真人；-1：不确定；-2:传入人脸数>1；
        /// </summary>
        public int[] isLive { get; set; }

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int num { get; set; }
    }
}
