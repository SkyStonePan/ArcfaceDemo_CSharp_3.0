namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 人脸比对可选的模型
    /// </summary>
    public enum ASF_CompareModel
    {
        /// <summary>
        /// 用于生活照之间的特征比对，推荐阈值0.80
        /// </summary>
        ASF_LIFE_PHOTO = 0x1,

        /// <summary>
        /// 用于证件照或生活照与证件照之间的特征比对，推荐阈值0.82
        /// </summary>
        ASF_ID_PHOTO = 0x2
    }
}
