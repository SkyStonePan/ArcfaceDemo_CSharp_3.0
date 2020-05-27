namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 活体置信度
    /// </summary>
    public struct ASF_LivenessThreshold
    {
        /// <summary>
        /// RGB活体阈值 取值范围[0,1]
        /// </summary>
        public float thresholdmodel_BGR;

        /// <summary>
        /// IR活体阈值 取值范围[0,1]
        /// </summary>
        public float thresholdmodel_IR;
    }
}
