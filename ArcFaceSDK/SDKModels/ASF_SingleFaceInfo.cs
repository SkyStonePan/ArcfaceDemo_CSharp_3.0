using System;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 单人脸检测结构体
    /// </summary>
    public struct SingleFaceInfo
    {
        /// <summary>
        /// 人脸坐标Rect结果
        /// </summary>
        public MRECT faceRect;

        /// <summary>
        /// 人脸角度
        /// </summary>
        public int faceOrient;

        /// <summary>
        /// face ID，IMAGE模式下不返回FaceID
        /// </summary>
        public int faceID;
    }
}
