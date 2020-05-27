using ArcFaceSDK.SDKModels;
using System;
using System.Collections.Generic;
namespace ArcFaceSDK.Entity
{
    /// <summary>
    /// 多人脸检测
    /// </summary>
    public class MultiFaceInfo
    {
        /// <summary>
        /// 人脸Rect结果集
        /// </summary>
        public MRECT[] faceRects { get; set; }

        /// <summary>
        /// 人脸角度结果集，与faceRects一一对应  对应ASF_OrientCode
        /// </summary>
        public int[] faceOrients { get; set; }

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int faceNum { get; set; }

        /// <summary>
        /// face ID，IMAGE模式下不返回FaceID
        /// </summary>
        public int[] faceID { get; set; }
    }
}
