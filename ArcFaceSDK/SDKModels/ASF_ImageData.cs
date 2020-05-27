using System;
using System.Runtime.InteropServices;

namespace ArcFaceSDK.SDKModels
{
    /// <summary>
    /// 图像数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ASF_ImageData
    {
        /// <summary>
        /// 图片格式
        /// </summary>
        public uint u32PixelArrayFormat;

        /// <summary>
        /// 宽
        /// </summary>
        public int i32Width;

        /// <summary>
        /// 高
        /// </summary>
        public int i32Height;

        /// <summary>
        /// 图像数据
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public IntPtr[] ppu8Plane;

        /// <summary>
        /// 步长
        /// </summary>       
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] pi32Pitch;
    }
}
