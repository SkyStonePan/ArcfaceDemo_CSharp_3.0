namespace ArcFaceSDK.Entity
{
    /// <summary>
    /// 年龄结果对象
    /// </summary>
    public class AgeInfo
    {
        /// <summary>
        /// 年龄检测结果集合
        /// </summary>
        public int[] ageArray { get; set; }

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int num { get; set; }
    }
}
