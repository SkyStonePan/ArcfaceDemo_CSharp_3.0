using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Utils;

namespace ArcSoftFace.Entity
{
    /// <summary>
    /// 视频检测缓存实体类
    /// </summary>
    public class FaceTrackUnit
    {
        /// <summary>
        /// 人脸ID
        /// </summary>
        public int FaceId { get; set; } 

        /// <summary>
        /// 人脸框
        /// </summary>
        public MRECT Rect { get; set; }

        /// <summary>
        /// 相似度结果
        /// </summary>
        public string Similarity { get; set; }

        /// <summary>
        /// 人脸定位
        /// </summary>
        public int FaceIndex = -1;

        /// <summary>
        /// Rgb活体
        /// </summary>
        public int RgbLiveness = -1;

        /// <summary>
        /// IR活体
        /// </summary>
        public int IrLiveness = -1;

        /// <summary>
        /// 人脸角度
        /// </summary>
        public int FaceOrient { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public FaceTrackUnit() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="faceId">人脸Faceid</param>
        /// <param name="rect">人脸框坐标</param>
        /// <param name="faceOrient">人脸角度</param>
        public FaceTrackUnit(int faceId, MRECT rect,int faceOrient)
        {
            FaceId = faceId;
            Rect = rect;
            FaceOrient = faceOrient;
        }

        /// <summary>
        /// 设置匹配人脸Index和相似度
        /// </summary>
        /// <param name="faceIndex">匹配人脸Index</param>
        /// <param name="similarity">相似度</param>
        public void SetFaceIndexAndSimilarity(int faceIndex, string similarity)
        {
            if (faceIndex >= 0)
            {
                FaceIndex = faceIndex;
                Similarity = similarity;
            }
        }

        /// <summary>
        /// 获取组合消息
        /// </summary>
        /// <returns></returns>
        public string GetCombineMessage()
        {
            return string.Format("{0}|{1}|{2}",FaceIndex>=0?string.Format("{0}号:{1}", FaceIndex, Similarity) :string.Empty,
               string.Format("RGB:{0}",CommonUtil.TransLivenessResult(RgbLiveness)),
               FaceId >= 0 ? string.Format("faceId:{0}", FaceId) : string.Empty).Trim();
        }

        /// <summary>
        /// 获取Ir活体检测文本
        /// </summary>
        /// <returns></returns>
        public string GetIrLivenessMessage()
        {
            return string.Format("IR:{0}", CommonUtil.TransLivenessResult(IrLiveness));
        }

        /// <summary>
        /// 是否成功匹配
        /// </summary>
        /// <returns></returns>
        public bool CertifySuccess()
        {
            return RgbLiveness.Equals(1) && FaceIndex >= 0;
        }
    }
}