
using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using System;
using System.Runtime.InteropServices;

namespace ArcFaceSDK.Utils
{
    /// <summary>
    /// 公用方法
    /// </summary>
    public class CommonUtil
    {
        /// <summary>
        /// 转化活体检测结果
        /// </summary>
        /// <param name="liveness">活体检测值</param>
        /// <returns></returns>
        public  static string TransLivenessResult(int liveness)
        {
            string rel = "不确定";
            switch (liveness)
            {
                case 0: rel = "非真人"; break;
                case 1: rel = "真人"; break;
                case -2: rel = "传入人脸数>1"; break;
                case -3: rel = "人脸过小"; break;
                case -4: rel = "角度过大"; break;
                case -5: rel = "人脸超出边界"; break;
                case -1:
                default:
                    rel = "不确定"; break;
            }
            return rel;
        }

        /// <summary>
        /// 将ImageInfo对象转化为ASF_ImageData结构体
        /// </summary>
        /// <param name="imageInfo">ImageInfo对象</param>
        /// <returns>ASF_ImageData结构体</returns>
        public static ASF_ImageData TransImageDataStructByImageInfo(ImageInfo imageInfo)
        {
            ASF_ImageData asfImageData = new ASF_ImageData();
            asfImageData.i32Width = imageInfo.width;
            asfImageData.i32Height = imageInfo.height;
            asfImageData.u32PixelArrayFormat = (uint)imageInfo.format;
            asfImageData.pi32Pitch = new int[4];
            asfImageData.ppu8Plane = new IntPtr[4];
            asfImageData.pi32Pitch[0] = imageInfo.widthStep;
            asfImageData.ppu8Plane[0] = imageInfo.imgData;
            return asfImageData;
        }
    }
}
