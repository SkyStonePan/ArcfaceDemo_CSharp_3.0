using ArcFaceSDK.Entity;
using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Utils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ArcFaceSDK
{
    /// <summary>
    /// ArcFace 接口封装类
    /// </summary>
    public class FaceEngine
    {
        /// <summary>
        /// 引擎 handle
        /// </summary>
        private IntPtr pEngine;

        /// <summary>
        /// 判断引擎状态
        /// </summary>
        /// <returns>true:引擎已初始化；false:引擎未初始化</returns>
        public bool GetEngineStatus()
        {
            return !(pEngine.Equals(IntPtr.Zero));
        }
        
        /// <summary>
        /// 激活接口
        /// </summary>
        /// <param name="appId">appId</param>
        /// <param name="appKey">appKey</param>
        /// <returns>返回0表示正常;其他值请在官网-帮助中心查询</returns>
        public int ASFActivation(string appId, string appKey)
        {
            return ASFFunctions.ASFActivation(appId, appKey);
        }

        /// <summary>
        /// 激活接口
        /// </summary>
        /// <param name="appId">appId</param>
        /// <param name="appKey">appKey</param>
        /// <returns>返回0或90114表示正常；其他值请在官网-帮助中心查询</returns>
        public int ASFOnlineActivation(string appId, string appKey)
        {
            return ASFFunctions.ASFOnlineActivation(appId, appKey);
        }

        /// <summary>
        /// 引擎初始化
        /// </summary>
        /// <param name="detectMode">检测模式</param>
        /// <param name="detectFaceOrientPriority">检测脸部的角度优先值</param>
        /// <param name="detectFaceScaleVal">用于数值化表示的最小人脸尺寸</param>
        /// <param name="detectFaceMaxNum">最大需要检测的人脸个数</param>
        /// <param name="combinedMask">用户选择需要检测的功能组合，可单个或多个</param>
        /// <returns>返回0表示正常；其他值请在官网-帮助中心查询</returns>
        public int ASFInitEngine(DetectionMode detectMode, ASF_OrientPriority detectFaceOrientPriority, int detectFaceScaleVal, int detectFaceMaxNum, int combinedMask)
        {
            pEngine = IntPtr.Zero;
            if (detectFaceScaleVal < 2 || detectFaceScaleVal > 32)
            {
                detectFaceScaleVal = 16;
            }
            if (detectFaceMaxNum < 1 || detectFaceMaxNum > 50)
            {
                detectFaceMaxNum = 10;
            }
            int retCode = ASFFunctions.ASFInitEngine(detectMode, detectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pEngine);
            return retCode;
        }

        /// <summary>
        /// 人脸检测/人脸追踪
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">多人脸对象</param>
        /// <param name="detectModel">检测模式</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFDetectFaces(Image image, out MultiFaceInfo multiFaceInfo, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8, ASF_DetectModel detectModel = ASF_DetectModel.ASF_DETECT_MODEL_RGB)
        {
            multiFaceInfo = new MultiFaceInfo();
            //判断图像是否为空
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);

            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }

            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFDetectFaces(pEngine, imageInfo.width, imageInfo.height, imageInfo.format, imageInfo.imgData, pMultiFaceInfo);
            if (retCode != 0)
            {
                MemoryUtil.FreeArray(imageInfo.imgData, pMultiFaceInfo);
                return retCode;
            }
            ASF_MultiFaceInfo multiFaceInfoStruct = MemoryUtil.PtrToStructure<ASF_MultiFaceInfo>(pMultiFaceInfo);
            MemoryUtil.FreeArray(imageInfo.imgData, pMultiFaceInfo);

            //转化非托管内存到托管内存
            multiFaceInfo.faceNum = multiFaceInfoStruct.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfoStruct.faceID != IntPtr.Zero)
                {
                    multiFaceInfo.faceID = new int[multiFaceInfo.faceNum];
                    Marshal.Copy(multiFaceInfoStruct.faceID, multiFaceInfo.faceID, 0, multiFaceInfo.faceNum);
                }
                multiFaceInfo.faceOrients = new int[multiFaceInfo.faceNum];
                Marshal.Copy(multiFaceInfoStruct.faceOrients, multiFaceInfo.faceOrients, 0, multiFaceInfo.faceNum);
                multiFaceInfo.faceRects = new MRECT[multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    multiFaceInfo.faceRects[i] = MemoryUtil.PtrToStructure<MRECT>(multiFaceInfoStruct.faceRects + MemoryUtil.SizeOf<MRECT>() * i);
                }
            }
            return retCode;
        }

        /// <summary>
        /// 人脸信息检测（年龄/性别/人脸3D角度）
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">多人脸对象</param>
        /// <param name="combinedMask">检测属性</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFProcess(Image image, MultiFaceInfo multiFaceInfo, int combinedMask, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8)
        {
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //将多人脸对象的信息转化到结构体中
            ASF_MultiFaceInfo multiFaceInfoStruct = new ASF_MultiFaceInfo();
            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            multiFaceInfoStruct.faceNum = multiFaceInfo.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfo.faceID != null)
                {
                    multiFaceInfoStruct.faceID = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                    Marshal.Copy(multiFaceInfo.faceID, 0, multiFaceInfoStruct.faceID, multiFaceInfo.faceNum);
                }
                multiFaceInfoStruct.faceOrients = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                Marshal.Copy(multiFaceInfo.faceOrients, 0, multiFaceInfoStruct.faceOrients, multiFaceInfo.faceNum);
                multiFaceInfoStruct.faceRects = MemoryUtil.Malloc(MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum);
                byte[] allByte = new byte[MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    byte[] tempBytes = MemoryUtil.StructToBytes(multiFaceInfo.faceRects[i]);
                    tempBytes.CopyTo(allByte, MemoryUtil.SizeOf<MRECT>() * i);
                }
                Marshal.Copy(allByte, 0, multiFaceInfoStruct.faceRects, allByte.Length);
            }

            MemoryUtil.StructureToPtr(multiFaceInfoStruct, pMultiFaceInfo);
            //调用SDK接口
            int retCode = ASFFunctions.ASFProcess(pEngine, imageInfo.width, imageInfo.height, imageInfo.format, imageInfo.imgData, pMultiFaceInfo, combinedMask);
            //释放内存
            MemoryUtil.FreeArray(imageInfo.imgData, multiFaceInfoStruct.faceID, multiFaceInfoStruct.faceOrients, multiFaceInfoStruct.faceRects, pMultiFaceInfo);
            return retCode;
        }

        /// <summary>
        /// 单人脸特征提取
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">人脸框对象</param>
        /// <param name="faceIndex">人脸索引</param>
        /// <param name="faceFeature">[out]特征结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFFaceFeatureExtract(Image image, MultiFaceInfo multiFaceInfo, out FaceFeature faceFeature, int faceIndex = 0, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8)
        {
            faceFeature = new FaceFeature();
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (faceIndex >= multiFaceInfo.faceNum)
            {
                return ErrorCodeUtil.FACEINDEX_INVALID;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //获取某个单人脸信息
            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
            IntPtr pSIngleFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<SingleFaceInfo>());
            singleFaceInfo.faceRect = multiFaceInfo.faceRects[faceIndex];
            singleFaceInfo.faceOrient = multiFaceInfo.faceOrients[faceIndex];
            MemoryUtil.StructureToPtr(singleFaceInfo, pSIngleFaceInfo);
            IntPtr pAsfFaceFeature = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_FaceFeature>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFFaceFeatureExtract(pEngine, imageInfo.width, imageInfo.height, imageInfo.format, imageInfo.imgData, pSIngleFaceInfo, pAsfFaceFeature);
            if (retCode != 0)
            {
                MemoryUtil.FreeArray(pSIngleFaceInfo, pAsfFaceFeature, imageInfo.imgData);
                return retCode;
            }
            //获取特征结构体，并转化
            ASF_FaceFeature asfFeature = MemoryUtil.PtrToStructure<ASF_FaceFeature>(pAsfFaceFeature);
            byte[] feature = new byte[asfFeature.featureSize];
            MemoryUtil.Copy(asfFeature.feature, feature, 0, asfFeature.featureSize);
            faceFeature.featureSize = asfFeature.featureSize;
            faceFeature.feature = feature;
            MemoryUtil.FreeArray(pSIngleFaceInfo, pAsfFaceFeature, imageInfo.imgData);
            return retCode;
        }

        /// <summary>
        /// 人脸特征比对
        /// </summary>
        /// <param name="faceFeature1">特征1</param>
        /// <param name="faceFeature2">特征2</param>
        /// <param name="similarity">相似度</param>
        /// <param name="compareModel">ASF_LIFE_PHOTO：用于生活照之间的特征比对；ASF_ID_PHOTO：用于证件照或证件照和生活照之间的特征比对</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFFaceFeatureCompare(FaceFeature faceFeature1, FaceFeature faceFeature2, out float similarity, ASF_CompareModel compareModel = ASF_CompareModel.ASF_LIFE_PHOTO)
        {
            similarity = 0f;
            if (faceFeature1 == null || faceFeature2 == null)
            {
                return ErrorCodeUtil.FEATURE_IS_NULL;
            }
            #region 将特征对象转化为特征结构体，再转化为非托管内存
            ASF_FaceFeature asfFeatureStruct1 = new ASF_FaceFeature();
            asfFeatureStruct1.featureSize = faceFeature1.featureSize;
            asfFeatureStruct1.feature = MemoryUtil.Malloc(asfFeatureStruct1.featureSize);
            MemoryUtil.Copy(faceFeature1.feature, 0, asfFeatureStruct1.feature, asfFeatureStruct1.featureSize);
            IntPtr pFeature1 = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_FaceFeature>());
            MemoryUtil.StructureToPtr(asfFeatureStruct1, pFeature1);

            ASF_FaceFeature asfFeatureStruct2 = new ASF_FaceFeature();
            asfFeatureStruct2.featureSize = faceFeature2.featureSize;
            asfFeatureStruct2.feature = MemoryUtil.Malloc(asfFeatureStruct2.featureSize);
            MemoryUtil.Copy(faceFeature2.feature, 0, asfFeatureStruct2.feature, asfFeatureStruct2.featureSize);
            IntPtr pFeature2 = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_FaceFeature>());
            MemoryUtil.StructureToPtr(asfFeatureStruct2, pFeature2);
            #endregion
            //调用SDK接口
            int retCode = ASFFunctions.ASFFaceFeatureCompare(pEngine, pFeature1, pFeature2, ref similarity, compareModel);
            MemoryUtil.FreeArray(pFeature1, pFeature2, asfFeatureStruct1.feature, asfFeatureStruct2.feature);

            return retCode;
        }

        /// <summary>
        /// 获取年龄结果
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="ageInfo">out 年龄结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetAge(out AgeInfo ageInfo)
        {
            ageInfo = new AgeInfo();
            IntPtr pAgeInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_AgeInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetAge(pEngine, pAgeInfo);
            if (retCode != 0)
            {
                MemoryUtil.Free(pAgeInfo);
                return retCode;
            }
            //转化结果
            ASF_AgeInfo asfAgeInfo = MemoryUtil.PtrToStructure<ASF_AgeInfo>(pAgeInfo);
            ageInfo.num = asfAgeInfo.num;
            if (ageInfo.num > 0)
            {
                ageInfo.ageArray = new int[ageInfo.num];
                Marshal.Copy(asfAgeInfo.ageArray, ageInfo.ageArray, 0, ageInfo.num);
            }
            MemoryUtil.FreeArray(pAgeInfo);
            return retCode;
        }

        /// <summary>
        /// 获取性别结果
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="genderInfo">out 性别结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetGender(out GenderInfo genderInfo)
        {
            genderInfo = new GenderInfo();
            IntPtr pGenderInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_GenderInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetGender(pEngine, pGenderInfo);
            if (retCode != 0)
            {
                MemoryUtil.Free(pGenderInfo);
                return retCode;
            }
            //转化结果
            ASF_GenderInfo asfGenderInfo = MemoryUtil.PtrToStructure<ASF_GenderInfo>(pGenderInfo);
            genderInfo.num = asfGenderInfo.num;
            if (genderInfo.num > 0)
            {
                genderInfo.genderArray = new int[genderInfo.num];
                Marshal.Copy(asfGenderInfo.genderArray, genderInfo.genderArray, 0, genderInfo.num);
            }
            MemoryUtil.FreeArray(pGenderInfo);
            return retCode;
        }

        /// <summary>
        /// 获取3D角度信息
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="faceAngle">3D角度信息结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetFace3DAngle(out Face3DAngle faceAngle)
        {
            faceAngle = new Face3DAngle();
            IntPtr pFaceAngle = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_Face3DAngle>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetFace3DAngle(pEngine, pFaceAngle);
            if (retCode != 0)
            {
                MemoryUtil.Free(pFaceAngle);
                return retCode;
            }
            //转化结果
            ASF_Face3DAngle asfFaceAngle = MemoryUtil.PtrToStructure<ASF_Face3DAngle>(pFaceAngle);
            faceAngle.num = asfFaceAngle.num;
            if (faceAngle.num > 0)
            {
                faceAngle.pitch = new float[faceAngle.num];
                Marshal.Copy(asfFaceAngle.pitch, faceAngle.pitch, 0, faceAngle.num);
                faceAngle.roll = new float[faceAngle.num];
                Marshal.Copy(asfFaceAngle.roll, faceAngle.roll, 0, faceAngle.num);
                faceAngle.yaw = new float[faceAngle.num];
                Marshal.Copy(asfFaceAngle.yaw, faceAngle.yaw, 0, faceAngle.num);
                faceAngle.status = new int[faceAngle.num];
                Marshal.Copy(asfFaceAngle.status, faceAngle.status, 0, faceAngle.num);
            }
            MemoryUtil.FreeArray(pFaceAngle);
            return retCode;
        }

        /// <summary>
        /// 获取RGB活体结果
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="livenessInfo">RGB活体结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetLivenessScore(out LivenessInfo livenessInfo)
        {
            livenessInfo = new LivenessInfo();
            IntPtr pLiveness = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_LivenessInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetLivenessScore(pEngine, pLiveness);
            if (retCode != 0)
            {
                MemoryUtil.Free(pLiveness);
                return retCode;
            }
            //转化结果
            ASF_LivenessInfo asfLivenessInfo = MemoryUtil.PtrToStructure<ASF_LivenessInfo>(pLiveness);
            livenessInfo.num = asfLivenessInfo.num;
            if (asfLivenessInfo.num > 0)
            {
                livenessInfo.isLive = new int[asfLivenessInfo.num];
                Marshal.Copy(asfLivenessInfo.isLive, livenessInfo.isLive, 0, asfLivenessInfo.num);
            }
            MemoryUtil.FreeArray(pLiveness);
            return retCode;
        }

        /// <summary>
        /// 该接口目前仅支持单人脸IR活体检测（不支持年龄、性别、3D角度的检测），默认取第一张人脸
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <param name="imageFormat">图像格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">人脸框信息</param>
        /// <param name="combinedMask">检测属性</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFProcess_IR(Image image, MultiFaceInfo multiFaceInfo, int combinedMask, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_GRAY)
        {
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //转化多人脸信息
            ASF_MultiFaceInfo multiFaceInfoStruct = new ASF_MultiFaceInfo();
            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            multiFaceInfoStruct.faceNum = multiFaceInfo.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfo.faceID != null)
                {
                    multiFaceInfoStruct.faceID = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                    Marshal.Copy(multiFaceInfo.faceID, 0, multiFaceInfoStruct.faceID, multiFaceInfo.faceNum);
                }
                multiFaceInfoStruct.faceOrients = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                Marshal.Copy(multiFaceInfo.faceOrients, 0, multiFaceInfoStruct.faceOrients, multiFaceInfo.faceNum);
                multiFaceInfoStruct.faceRects = MemoryUtil.Malloc(MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum);
                byte[] allByte = new byte[MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    byte[] tempBytes = MemoryUtil.StructToBytes(multiFaceInfo.faceRects[i]);
                    tempBytes.CopyTo(allByte, MemoryUtil.SizeOf<MRECT>() * i);
                }
                Marshal.Copy(allByte, 0, multiFaceInfoStruct.faceRects, allByte.Length);
            }
            MemoryUtil.StructureToPtr(multiFaceInfoStruct, pMultiFaceInfo);
            //调用SDK接口
            int retCode = ASFFunctions.ASFProcess_IR(pEngine, imageInfo.width, imageInfo.height, imageInfo.format, imageInfo.imgData, pMultiFaceInfo, combinedMask);
            //释放内存
            MemoryUtil.FreeArray(multiFaceInfoStruct.faceID, multiFaceInfoStruct.faceOrients, multiFaceInfoStruct.faceRects, pMultiFaceInfo);

            return retCode;
        }

        /// <summary>
        /// 获取IR活体结果
        /// </summary>
        /// <param name="livenessInfo">IR活体结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetLivenessScore_IR(out LivenessInfo livenessInfo)
        {
            livenessInfo = new LivenessInfo();
            IntPtr pLiveness = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_LivenessInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetLivenessScore_IR(pEngine, pLiveness);
            if (retCode != 0)
            {
                MemoryUtil.Free(pLiveness);
                return retCode;
            }
            //转化结果
            ASF_LivenessInfo asfLivenessInfo = MemoryUtil.PtrToStructure<ASF_LivenessInfo>(pLiveness);
            livenessInfo.num = asfLivenessInfo.num;
            if (asfLivenessInfo.num > 0)
            {
                livenessInfo.isLive = new int[asfLivenessInfo.num];
                Marshal.Copy(asfLivenessInfo.isLive, livenessInfo.isLive, 0, asfLivenessInfo.num);
            }
            MemoryUtil.FreeArray(pLiveness);
            return retCode;
        }

        /// <summary>
        /// 销毁引擎
        /// </summary>
        /// <param name="pEngine">引擎handle</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public void ASFUninitEngine()
        {
            try
            {
                if (!pEngine.Equals(IntPtr.Zero))
                {
                    ASFFunctions.ASFUninitEngine(pEngine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <param name="version">版本信息</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetVersion(out SDKVersion version)
        {
            version = new SDKVersion();
            int retCode = -1;
            ASF_VERSION asfVersion = ASFFunctions.ASFGetVersion();
            version.version = Marshal.PtrToStringAnsi(asfVersion.Version);
            version.buildDate = Marshal.PtrToStringAnsi(asfVersion.BuildDate);
            version.copyRight = Marshal.PtrToStringAnsi(asfVersion.CopyRight);
            return retCode;
        }

        /// <summary>
        /// 获取激活文件信息
        /// </summary>
        /// <param name="activeFileInfo">激活文件信息</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFGetActiveFileInfo(out ActiveFileInfo activeFileInfo)
        {
            activeFileInfo = new ActiveFileInfo();
            IntPtr pASFActiveFileInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_ActiveFileInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFGetActiveFileInfo(pASFActiveFileInfo);
            if (retCode != 0)
            {
                MemoryUtil.Free(pASFActiveFileInfo);
                return retCode;
            }
            //转化结果
            ASF_ActiveFileInfo asfActiveFileInfo = MemoryUtil.PtrToStructure<ASF_ActiveFileInfo>(pASFActiveFileInfo);
            activeFileInfo.startTime = Marshal.PtrToStringAnsi(asfActiveFileInfo.startTime);
            activeFileInfo.endTime = Marshal.PtrToStringAnsi(asfActiveFileInfo.endTime);
            activeFileInfo.platform = Marshal.PtrToStringAnsi(asfActiveFileInfo.platform);
            activeFileInfo.sdkType = Marshal.PtrToStringAnsi(asfActiveFileInfo.sdkType);
            activeFileInfo.appId = Marshal.PtrToStringAnsi(asfActiveFileInfo.appId);
            activeFileInfo.sdkKey = Marshal.PtrToStringAnsi(asfActiveFileInfo.sdkKey);
            activeFileInfo.sdkVersion = Marshal.PtrToStringAnsi(asfActiveFileInfo.sdkVersion);
            activeFileInfo.fileVersion = Marshal.PtrToStringAnsi(asfActiveFileInfo.fileVersion);
            MemoryUtil.Free(pASFActiveFileInfo);
            return retCode;
        }

        /// <summary>
        /// 设置活体阈值：取值范围[0-1]内部默认数值RGB-0.5，IR-0.7， 用户可以根据实际需求，设置不同的阈值
        /// </summary>
        /// <param name="rgbThreshold">RGB活体阈值</param>
        /// <param name="irThreshole">IR活体阈值</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFSetLivenessParam(float rgbThreshold = 0.5f, float irThreshole = 0.7f)
        {
            ASF_LivenessThreshold livebessThreshold = new ASF_LivenessThreshold();
            //对应设置阈值
            livebessThreshold.thresholdmodel_BGR = (rgbThreshold >= 0 && rgbThreshold <= 1) ? rgbThreshold : 0.5f;
            livebessThreshold.thresholdmodel_IR = (irThreshole >= 0 && irThreshole <= 1) ? irThreshole : 0.7f;
            IntPtr pLivenessThreshold = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_LivenessThreshold>());
            MemoryUtil.StructureToPtr(livebessThreshold, pLivenessThreshold);
            //调用SDK接口
            int retCode = ASFFunctions.ASFSetLivenessParam(pEngine, pLivenessThreshold);
            MemoryUtil.Free(pLivenessThreshold);
            return retCode;
        }

        /// <summary>
        /// 人脸检测/人脸追踪
        /// 图像数据以结构体形式传入，对采用更高字节对齐方式的图像兼容性更好
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">多人脸对象</param>
        /// <param name="detectModel">检测模式</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFDetectFacesEx(Image image, out MultiFaceInfo multiFaceInfo, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8, ASF_DetectModel detectModel = ASF_DetectModel.ASF_DETECT_MODEL_RGB)
        {
            multiFaceInfo = new MultiFaceInfo();
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            ASF_ImageData asfInfoData = CommonUtil.TransImageDataStructByImageInfo(imageInfo);
            IntPtr pImageInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_ImageData>());
            MemoryUtil.StructureToPtr(asfInfoData, pImageInfo);

            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            //调用SDK接口
            int retCode = ASFFunctions.ASFDetectFacesEx(pEngine, pImageInfo, pMultiFaceInfo);
            if (retCode != 0)
            {
                MemoryUtil.FreeArray(imageInfo.imgData, pMultiFaceInfo, pImageInfo);
                return retCode;
            }
            ASF_MultiFaceInfo multiFaceInfoStruct = MemoryUtil.PtrToStructure<ASF_MultiFaceInfo>(pMultiFaceInfo);
            MemoryUtil.FreeArray(imageInfo.imgData, pMultiFaceInfo, pImageInfo);

            //转化非托管内存到托管内存
            multiFaceInfo.faceNum = multiFaceInfoStruct.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfoStruct.faceID != IntPtr.Zero)
                {
                    multiFaceInfo.faceID = new int[multiFaceInfo.faceNum];
                    Marshal.Copy(multiFaceInfoStruct.faceID, multiFaceInfo.faceID, 0, multiFaceInfo.faceNum);
                }
                multiFaceInfo.faceOrients = new int[multiFaceInfo.faceNum];
                Marshal.Copy(multiFaceInfoStruct.faceOrients, multiFaceInfo.faceOrients, 0, multiFaceInfo.faceNum);
                multiFaceInfo.faceRects = new MRECT[multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    multiFaceInfo.faceRects[i] = MemoryUtil.PtrToStructure<MRECT>(multiFaceInfoStruct.faceRects + MemoryUtil.SizeOf<MRECT>() * i);
                }
            }
            return retCode;
        }

        /// <summary>
        /// 年龄/性别/人脸3D角度（该接口仅支持RGB图像），最多支持4张人脸信息检测，超过部分返回未知
	    /// RGB活体仅支持单人脸检测，该接口不支持检测IR活体
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">多人脸对象</param>
        /// <param name="combinedMask">检测属性</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFProcessEx(Image image, MultiFaceInfo multiFaceInfo, int combinedMask, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8)
        {
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //转化人脸信息
            ASF_MultiFaceInfo multiFaceInfoStruct = new ASF_MultiFaceInfo();
            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            multiFaceInfoStruct.faceNum = multiFaceInfo.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfo.faceID != null)
                {
                    multiFaceInfoStruct.faceID = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                    Marshal.Copy(multiFaceInfo.faceID, 0, multiFaceInfoStruct.faceID, multiFaceInfo.faceNum);
                }
                multiFaceInfoStruct.faceOrients = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                Marshal.Copy(multiFaceInfo.faceOrients, 0, multiFaceInfoStruct.faceOrients, multiFaceInfo.faceNum);
                multiFaceInfoStruct.faceRects = MemoryUtil.Malloc(MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum);
                byte[] allByte = new byte[MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    byte[] tempBytes = MemoryUtil.StructToBytes(multiFaceInfo.faceRects[i]);
                    tempBytes.CopyTo(allByte, MemoryUtil.SizeOf<MRECT>() * i);
                }
                Marshal.Copy(allByte, 0, multiFaceInfoStruct.faceRects, allByte.Length);
            }
            MemoryUtil.StructureToPtr(multiFaceInfoStruct, pMultiFaceInfo);

            ASF_ImageData asfInfoData = CommonUtil.TransImageDataStructByImageInfo(imageInfo);
            IntPtr pImageInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_ImageData>());
            MemoryUtil.StructureToPtr(asfInfoData, pImageInfo);

            //调用SDK接口
            int retCode = ASFFunctions.ASFProcessEx(pEngine, pImageInfo, pMultiFaceInfo, combinedMask);
            //释放内存
            MemoryUtil.FreeArray(imageInfo.imgData, multiFaceInfoStruct.faceID, multiFaceInfoStruct.faceOrients, multiFaceInfoStruct.faceRects, pMultiFaceInfo, pImageInfo);

            return retCode;
        }

        /// <summary>
        /// 该接口目前仅支持单人脸IR活体检测（不支持年龄、性别、3D角度的检测），默认取第一张人脸
	    /// 图像数据以结构体形式传入，对采用更高字节对齐方式的图像兼容性更好
        /// </summary>
        /// <param name="imageFormat">图像格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">人脸框信息</param>
        /// <param name="combinedMask">检测属性</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFProcessEx_IR(Image image, MultiFaceInfo multiFaceInfo, int combinedMask, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_GRAY)
        {
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //转化人脸信息
            ASF_MultiFaceInfo multiFaceInfoStruct = new ASF_MultiFaceInfo();
            IntPtr pMultiFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_MultiFaceInfo>());
            multiFaceInfoStruct.faceNum = multiFaceInfo.faceNum;
            if (multiFaceInfo.faceNum > 0)
            {
                if (multiFaceInfo.faceID != null)
                {
                    multiFaceInfoStruct.faceID = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                    Marshal.Copy(multiFaceInfo.faceID, 0, multiFaceInfoStruct.faceID, multiFaceInfo.faceNum);
                }
                multiFaceInfoStruct.faceOrients = MemoryUtil.Malloc(multiFaceInfo.faceNum * MemoryUtil.SizeOf<int>());
                Marshal.Copy(multiFaceInfo.faceOrients, 0, multiFaceInfoStruct.faceOrients, multiFaceInfo.faceNum);
                multiFaceInfoStruct.faceRects = MemoryUtil.Malloc(MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum);
                byte[] allByte = new byte[MemoryUtil.SizeOf<MRECT>() * multiFaceInfo.faceNum];
                for (int i = 0; i < multiFaceInfo.faceNum; i++)
                {
                    byte[] tempBytes = MemoryUtil.StructToBytes(multiFaceInfo.faceRects[i]);
                    tempBytes.CopyTo(allByte, MemoryUtil.SizeOf<MRECT>() * i);
                }
                Marshal.Copy(allByte, 0, multiFaceInfoStruct.faceRects, allByte.Length);
            }
            MemoryUtil.StructureToPtr(multiFaceInfoStruct, pMultiFaceInfo);

            ASF_ImageData asfInfoData = CommonUtil.TransImageDataStructByImageInfo(imageInfo);
            IntPtr pImageInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_ImageData>());
            MemoryUtil.StructureToPtr(asfInfoData, pImageInfo);

            //调用SDK接口
            int retCode = ASFFunctions.ASFProcessEx_IR(pEngine, pImageInfo, pMultiFaceInfo, combinedMask);
            //释放内存
            MemoryUtil.FreeArray(imageInfo.imgData, multiFaceInfoStruct.faceID, multiFaceInfoStruct.faceOrients, multiFaceInfoStruct.faceRects, pMultiFaceInfo, pImageInfo);

            return retCode;
        }

        /// <summary>
        /// 单人脸特征提取
        /// </summary>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="image">图片</param>
        /// <param name="multiFaceInfo">人脸框对象</param>
        /// <param name="faceIndex">人脸索引</param>
        /// <param name="faceFeature">[out]特征结果</param>
        /// <returns>返回0表示正常；返回负数请根据ErrorCodeUtil类注释查看；其他值请在官网-帮助中心查询</returns>
        public int ASFFaceFeatureExtractEx(Image image, MultiFaceInfo multiFaceInfo, out FaceFeature faceFeature, int faceIndex = 0, ASF_ImagePixelFormat imageFormat = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8)
        {
            faceFeature = new FaceFeature();
            if (multiFaceInfo == null)
            {
                return ErrorCodeUtil.MULPTIFACEINFO_IS_NULL;
            }
            if (faceIndex >= multiFaceInfo.faceNum)
            {
                return ErrorCodeUtil.FACEINDEX_INVALID;
            }
            if (image == null)
            {
                return ErrorCodeUtil.IMAGE_IS_NULL;
            }
            ImageInfo imageInfo = ASF_ImagePixelFormat.ASVL_PAF_RGB24_B8G8R8.Equals(imageFormat) ? ImageUtil.ReadBMP(image) : ImageUtil.ReadBMP_IR(image);
            if (imageInfo == null)
            {
                return ErrorCodeUtil.IMAGE_DATA_READ_FAIL;
            }
            //转化单人脸信息
            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
            IntPtr pSIngleFaceInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<SingleFaceInfo>());
            singleFaceInfo.faceRect = multiFaceInfo.faceRects[faceIndex];
            singleFaceInfo.faceOrient = multiFaceInfo.faceOrients[faceIndex];
            MemoryUtil.StructureToPtr(singleFaceInfo, pSIngleFaceInfo);
            IntPtr pAsfFaceFeature = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_FaceFeature>());

            ASF_ImageData asfInfoData = CommonUtil.TransImageDataStructByImageInfo(imageInfo);
            IntPtr pImageInfo = MemoryUtil.Malloc(MemoryUtil.SizeOf<ASF_ImageData>());
            MemoryUtil.StructureToPtr(asfInfoData, pImageInfo);

            //调用SDK接口
            int retCode = ASFFunctions.ASFFaceFeatureExtractEx(pEngine, pImageInfo, pSIngleFaceInfo, pAsfFaceFeature);
            if (retCode != 0)
            {
                MemoryUtil.FreeArray(pSIngleFaceInfo, pAsfFaceFeature, imageInfo.imgData, pImageInfo);
                return retCode;
            }
            ASF_FaceFeature asfFeature = MemoryUtil.PtrToStructure<ASF_FaceFeature>(pAsfFaceFeature);
            byte[] feature = new byte[asfFeature.featureSize];
            MemoryUtil.Copy(asfFeature.feature, feature, 0, asfFeature.featureSize);
            faceFeature.featureSize = asfFeature.featureSize;
            faceFeature.feature = feature;
            MemoryUtil.FreeArray(pSIngleFaceInfo, pAsfFaceFeature, imageInfo.imgData, pImageInfo);
            return retCode;
        }

        /// <summary>
        /// 析构函数-注销引擎
        /// </summary>
        ~FaceEngine()
        {
            ASFUninitEngine();
        }
    }
}