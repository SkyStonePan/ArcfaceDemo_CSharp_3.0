using System;
using System.Drawing;
using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Entity;
using ArcFaceSDK;

namespace ArcSoftFace.Utils
{
    public static class FaceUtil
    {
        /// <summary>
        /// 人脸检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="image">图像</param>
        /// <returns></returns>
        public static MultiFaceInfo DetectFace(FaceEngine faceEngine, Image image)
        {
            MultiFaceInfo multiFaceInfo = null;
            try
            {
                faceEngine.ASFDetectFacesEx(image, out multiFaceInfo);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return multiFaceInfo;
        }

        /// <summary>
        /// IR图片人脸检测
        /// </summary>
        /// <param name="pEngine"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static MultiFaceInfo DetectFaceIR(FaceEngine faceEngine, Bitmap image)
        {
            MultiFaceInfo multiFaceInfo = null;
            try
            {
                faceEngine.ASFDetectFacesEx(image,out multiFaceInfo,ASF_ImagePixelFormat.ASVL_PAF_GRAY);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return multiFaceInfo;
        }

        /// <summary>
        /// 提取人脸特征，多人脸默认取第一个人脸特征
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="image">图像</param>
        /// <returns>保存人脸特征结构体指针</returns>
        public static FaceFeature ExtractFeature(FaceEngine faceEngine, Image image,out SingleFaceInfo singleFaceInfo,ref int retCode,int faceIndex = 0)
        {
            FaceFeature faceFeature = new FaceFeature();
            singleFaceInfo = new SingleFaceInfo();
            try
            {
                if (image == null)
                {
                    return faceFeature;
                }
                MultiFaceInfo multiFaceInfo;
                //人脸检测
                retCode = faceEngine.ASFDetectFacesEx(image,out multiFaceInfo);
                if(retCode != 0 || multiFaceInfo.faceNum <=0)
                {
                    return faceFeature;
                }
                singleFaceInfo.faceOrient = multiFaceInfo.faceOrients[faceIndex];
                singleFaceInfo.faceRect = multiFaceInfo.faceRects[faceIndex];
                retCode = faceEngine.ASFFaceFeatureExtractEx(image, multiFaceInfo, out faceFeature, faceIndex);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return faceFeature;
        }

        /// <summary>
        /// 提取单人脸特征
        /// </summary>
        /// <param name="pEngine">人脸识别引擎</param>
        /// <param name="image">图片</param>
        /// <param name="singleFaceInfo">单人脸信息</param>
        /// <returns>单人脸特征</returns>
        public static FaceFeature ExtractFeature(FaceEngine faceEngine, Image image, SingleFaceInfo singleFaceInfo)
        {
            FaceFeature faceFeature = null;
            try
            {
                MultiFaceInfo multiFaceInfo = new MultiFaceInfo();
                multiFaceInfo.faceNum = 1;
                multiFaceInfo.faceOrients = new int[1];
                multiFaceInfo.faceOrients[0] = singleFaceInfo.faceOrient;
                multiFaceInfo.faceRects = new MRECT[1];
                multiFaceInfo.faceRects[0] = singleFaceInfo.faceRect;

                faceEngine.ASFFaceFeatureExtractEx(image, multiFaceInfo, out faceFeature);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return faceFeature;
        }
        
        /// <summary>
        /// 年龄检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="imageInfo">图像数据</param>
        /// <param name="multiFaceInfo">人脸检测结果</param>
        /// <returns>年龄检测结构体</returns>
        public static AgeInfo AgeEstimation(FaceEngine faceEngine, Image image, MultiFaceInfo multiFaceInfo, out int retCode)
        {
            retCode = -1;
            AgeInfo ageInfo = new AgeInfo();
            try
            {
                if (multiFaceInfo.faceNum <= 0)
                {
                    return ageInfo;
                }
                //人脸信息检测
                retCode = faceEngine.ASFProcessEx(image, multiFaceInfo, FaceEngineMask.ASF_AGE);
                if (retCode == 0)
                {
                    //获取年龄信息
                    retCode = faceEngine.ASFGetAge(out ageInfo);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return ageInfo;
        }

        /// <summary>
        /// 性别检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="imageInfo">图像数据</param>
        /// <param name="multiFaceInfo">检测的人脸信息</param>
        /// <returns>性别检测结果结构体</returns>
        public static GenderInfo GenderEstimation(FaceEngine faceEngine, Image image, MultiFaceInfo multiFaceInfo, out int retCode)
        {
            retCode = -1;
            GenderInfo genderInfo = new GenderInfo();
            try
            {
                if (multiFaceInfo.faceNum <= 0)
                {
                    return genderInfo;
                }

                //人脸信息检测
                retCode = faceEngine.ASFProcessEx(image, multiFaceInfo, FaceEngineMask.ASF_GENDER);
                if (retCode == 0)
                {
                    //获取性别信息
                    retCode = faceEngine.ASFGetGender(out genderInfo);   
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return genderInfo;
        }

        /// <summary>
        /// 人脸3D角度检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="imageInfo">图像数据</param>
        /// <param name="multiFaceInfo">检测的人脸信息</param>
        /// <returns>人脸3D角度检测结果结构体</returns>
        public static Face3DAngle Face3DAngleDetection(FaceEngine faceEngine, Image image, MultiFaceInfo multiFaceInfo, out int retCode)
        {
            retCode = -1;
            Face3DAngle face3DAngle = new Face3DAngle();
            try
            {
                if (multiFaceInfo.faceNum <= 0)
                {
                    return face3DAngle;
                }

                //人脸信息检测
                retCode = faceEngine.ASFProcessEx(image, multiFaceInfo, FaceEngineMask.ASF_FACE3DANGLE);
                if (retCode == 0)
                {
                    //获取人脸3D角度
                    retCode = faceEngine.ASFGetFace3DAngle(out face3DAngle);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return face3DAngle;
        }

        /// <summary>
        /// RGB活体检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="imageInfo">图像数据</param>
        /// <param name="singleFaceInfo">单人脸信息</param>
        /// <returns>活体检测结果</returns>
        public static LivenessInfo LivenessInfo_RGB(FaceEngine faceEngine, Image image, SingleFaceInfo singleFaceInfo, out int retCode)
        {
            LivenessInfo livenessInfo = new LivenessInfo();
            retCode = -1;
            try
            {
                MultiFaceInfo multiFaceInfo = new MultiFaceInfo();
                multiFaceInfo.faceNum = 1;
                multiFaceInfo.faceOrients = new int[1];
                multiFaceInfo.faceOrients[0] = singleFaceInfo.faceOrient;
                multiFaceInfo.faceRects = new MRECT[1];
                multiFaceInfo.faceRects[0] = singleFaceInfo.faceRect;
                //人脸信息检测
                retCode = faceEngine.ASFProcessEx(image, multiFaceInfo, FaceEngineMask.ASF_LIVENESS);
                if (retCode == 0)
                {
                    //获取活体检测结果
                    retCode = faceEngine.ASFGetLivenessScore(out livenessInfo);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return livenessInfo;
        }

        /// <summary>
        /// 红外活体检测
        /// </summary>
        /// <param name="pEngine">引擎Handle</param>
        /// <param name="imageInfo">图像数据</param>
        /// <param name="singleFaceInfo">单人脸信息</param>
        /// <returns>活体检测结果</returns>
        public static LivenessInfo LivenessInfo_IR(FaceEngine faceEngine, Image image, SingleFaceInfo singleFaceInfo, out int retCode)
        {
            LivenessInfo livenessInfo = new LivenessInfo();
            retCode = -1;
            try
            {
                MultiFaceInfo multiFaceInfo = new MultiFaceInfo();
                multiFaceInfo.faceNum = 1;
                multiFaceInfo.faceOrients = new int[1];
                multiFaceInfo.faceOrients[0] = singleFaceInfo.faceOrient;
                multiFaceInfo.faceRects = new MRECT[1];
                multiFaceInfo.faceRects[0] = singleFaceInfo.faceRect;
                //人脸信息处理
                retCode = faceEngine.ASFProcessEx_IR(image, multiFaceInfo, FaceEngineMask.ASF_IR_LIVENESS);
                if (retCode == 0)
                {
                    //获取IR活体检测结果
                    retCode = faceEngine.ASFGetLivenessScore_IR(out livenessInfo);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(typeof(FaceUtil), ex);
            }
            return livenessInfo;
        }

        /// <summary>
        /// 获取多个人脸检测结果中面积最大的人脸
        /// </summary>
        /// <param name="multiFaceInfo">人脸检测结果</param>
        /// <returns>面积最大的人脸信息</returns>
        public static SingleFaceInfo GetMaxFace(MultiFaceInfo multiFaceInfo)
        {
            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
            singleFaceInfo.faceRect = new MRECT();
            singleFaceInfo.faceOrient = 1;

            int maxArea = 0;
            int index = -1;
            //遍历查找最大人脸索引
            for (int i = 0; i < multiFaceInfo.faceNum; i++)
            {
                try
                {
                    MRECT rect = multiFaceInfo.faceRects[i];
                    int area = (rect.right - rect.left) * (rect.bottom - rect.top);
                    if (maxArea <= area)
                    {
                        maxArea = area;
                        index = i;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo(typeof(FaceUtil), ex);
                }
            }
            //获取对应的人脸信息
            if (index != -1) 
            {
                singleFaceInfo.faceRect = multiFaceInfo.faceRects[index];
                singleFaceInfo.faceOrient = multiFaceInfo.faceOrients[index];
                if(multiFaceInfo.faceID != null && multiFaceInfo.faceID.Length > index)
                {
                    singleFaceInfo.faceID = multiFaceInfo.faceID[index];
                }
            }
            return singleFaceInfo;
        }
    }
}
