namespace ArcFaceSDK.Entity
{
    /// <summary>
    /// 性别结果
    /// </summary>
    public class GenderInfo
    {
        /// <summary>
        /// 性别检测结果集合
        /// </summary>
        public int[] genderArray { get; set; }

        /// <summary>
        /// 结果集大小
        /// </summary>
        public int num { get; set; }
    }
}
