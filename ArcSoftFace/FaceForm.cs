using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArcSoftFace.Utils;
using ArcSoftFace.Entity;
using System.IO;
using System.Configuration;
using System.Threading;
using AForge.Video.DirectShow;
using ArcFaceSDK;
using ArcFaceSDK.SDKModels;
using ArcFaceSDK.Entity;
using ArcFaceSDK.Utils;

namespace ArcSoftFace
{
    public partial class FaceForm : Form
    {
        #region 参数定义
        /// <summary>
        /// 图像处理引擎对象
        /// </summary>
        private FaceEngine imageEngine = new FaceEngine();

        /// <summary>
        /// 保存右侧图片路径
        /// </summary>
        private string image1Path;

        /// <summary>
        /// 图片最大大小限制
        /// </summary>
        private long maxSize = 1024 * 1024 * 2;

        /// <summary>
        /// 最大宽度
        /// </summary>
        private int maxWidth = 1536;

        /// <summary>
        /// 最大高度
        /// </summary>
        private int maxHeight = 1536;

        /// <summary>
        /// 比对人脸图片人脸特征
        /// </summary>
        private List<FaceFeature> rightImageFeatureList = new List<FaceFeature>();

        /// <summary>
        /// 保存对比图片的列表
        /// </summary>
        private List<string> imagePathList = new List<string>();

        /// <summary>
        /// 人脸库人脸特征列表
        /// </summary>
        private List<FaceFeature> leftImageFeatureList = new List<FaceFeature>();

        /// <summary>
        /// 相似度
        /// </summary>
        private float threshold = 0.8f;

        /// <summary>
        /// 用于标记是否需要清除比对结果
        /// </summary>
        private bool isCompare = false;

        #region 视频模式下相关
        /// <summary>
        /// 视频引擎对象
        /// </summary>
        private FaceEngine videoEngine = new FaceEngine();

        /// <summary>
        /// RGB视频引擎对象
        /// </summary>
        private FaceEngine videoRGBImageEngine = new FaceEngine();

        /// <summary>
        /// IR视频引擎对象
        /// </summary>
        private FaceEngine videoIRImageEngine = new FaceEngine();

        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        private FilterInfoCollection filterInfoCollection;

        /// <summary>
        /// RGB摄像头设备
        /// </summary>
        private VideoCaptureDevice rgbDeviceVideo;

        /// <summary>
        /// IR摄像头设备
        /// </summary>
        private VideoCaptureDevice irDeviceVideo;
        
        /// <summary>
        /// 是否是双目摄像
        /// </summary>
        private bool isDoubleShot = false;

        /// <summary>
        /// RGB 摄像头索引
        /// </summary>
        private int rgbCameraIndex = 0;

        /// <summary>
        /// IR 摄像头索引
        /// </summary>
        private int irCameraIndex = 0;

        /// <summary>
        /// 人员库图片选择 锁对象
        /// </summary>
        private object chooseImgLocker = new object();

        /// <summary>
        /// RGB视频帧图像使用锁
        /// </summary>
        private object rgbVideoImageLocker = new object();
        /// <summary>
        /// IR视频帧图像使用锁
        /// </summary>
        private object irVideoImageLocker = new object();
        /// <summary>
        /// RGB视频帧图像
        /// </summary>
        private Bitmap rgbVideoBitmap = null;
        /// <summary>
        /// IR视频帧图像
        /// </summary>
        private Bitmap irVideoBitmap = null;
        /// <summary>
        /// RGB 摄像头视频人脸追踪检测结果
        /// </summary>
        private DictionaryUnit<int,FaceTrackUnit> trackRGBUnitDict = new DictionaryUnit<int, FaceTrackUnit>();

        /// <summary>
        /// RGB 特征搜索尝试次数字典
        /// </summary>
        private DictionaryUnit<int, int> rgbFeatureTryDict = new DictionaryUnit<int, int>();

        /// <summary>
        /// RGB 活体检测尝试次数字典
        /// </summary>
        private DictionaryUnit<int, int> rgbLivenessTryDict = new DictionaryUnit<int, int>();

        /// <summary>
        /// IR 视频最大人脸追踪检测结果
        /// </summary>
        private FaceTrackUnit trackIRUnit = new FaceTrackUnit();

        /// <summary>
        /// VideoPlayer 框的字体
        /// </summary>
        private Font font = new Font(FontFamily.GenericSerif, 10f, FontStyle.Bold);

        /// <summary>
        /// 红色画笔
        /// </summary>
        private SolidBrush redBrush = new SolidBrush(Color.Red);

        /// <summary>
        /// 绿色画笔
        /// </summary>
        private SolidBrush greenBrush = new SolidBrush(Color.Green);
        
        /// <summary>
        /// 关闭FR线程开关
        /// </summary>
        private bool exitVideoRGBFR = false;

        /// <summary>
        /// 关闭活体线程开关
        /// </summary>
        private bool exitVideoRGBLiveness = false;
        /// <summary>
        /// 关闭IR活体和FR线程线程开关
        /// </summary>
        private bool exitVideoIRFRLiveness = false;
        /// <summary>
        /// FR失败重试次数
        /// </summary>
        private int frMatchTime = 30;

        /// <summary>
        /// 活体检测失败重试次数
        /// </summary>
        private int liveMatchTime = 30;
        #endregion
        #endregion

        #region 初始化
        public FaceForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            //初始化引擎
            InitEngines();
            //隐藏摄像头图像窗口
            rgbVideoSource.Hide();
            irVideoSource.Hide();
            //阈值控件不可用
            txtThreshold.Enabled = false;
        }

        /// <summary>
        /// 初始化引擎
        /// </summary>
        private void InitEngines()
        {
            try
            {
                //读取配置文件
                AppSettingsReader reader = new AppSettingsReader();
                string appId = (string)reader.GetValue("APPID", typeof(string));
                string sdkKey64 = (string)reader.GetValue("SDKKEY64", typeof(string));
                string sdkKey32 = (string)reader.GetValue("SDKKEY32", typeof(string));
                rgbCameraIndex = (int)reader.GetValue("RGB_CAMERA_INDEX", typeof(int));
                irCameraIndex = (int)reader.GetValue("IR_CAMERA_INDEX", typeof(int));
                frMatchTime= (int)reader.GetValue("FR_MATCH_TIME", typeof(int));
                liveMatchTime = (int)reader.GetValue("LIVENESS_MATCH_TIME", typeof(int));
                //判断CPU位数
                var is64CPU = Environment.Is64BitProcess;
                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(is64CPU ? sdkKey64 : sdkKey32))
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                    MessageBox.Show(string.Format("请在App.config配置文件中先配置APP_ID和SDKKEY{0}!", is64CPU ? "64" : "32"));
                    System.Environment.Exit(0);
                }
                //在线激活引擎    如出现错误，1.请先确认从官网下载的sdk库已放到对应的bin中，2.当前选择的CPU为x86或者x64
                int retCode = 0;
                try
                {
                    retCode = imageEngine.ASFOnlineActivation(appId, is64CPU ? sdkKey64 : sdkKey32);
                    if (retCode != 0 && retCode != 90114)
                    {
                        MessageBox.Show("激活SDK失败,错误码:" + retCode);
                        System.Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                    if (ex.Message.Contains("无法加载 DLL"))
                    {
                        MessageBox.Show("请将SDK相关DLL放入bin对应的x86或x64下的文件夹中!");
                    }
                    else
                    {
                        MessageBox.Show("激活SDK失败,请先检查依赖环境及SDK的平台、版本是否正确!");
                    }
                    System.Environment.Exit(0);
                }

                //初始化引擎
                DetectionMode detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
                //Video模式下检测脸部的角度优先值
                ASF_OrientPriority videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
                //Image模式下检测脸部的角度优先值
                ASF_OrientPriority imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_ALL_OUT;
                //人脸在图片中所占比例，如果需要调整检测人脸尺寸请修改此值，有效数值为2-32
                int detectFaceScaleVal = 16;
                //最大需要检测的人脸个数
                int detectFaceMaxNum = 15;
                //引擎初始化时需要初始化的检测功能组合
                int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE;
                //初始化引擎，正常值为0，其他返回值请参考http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
                retCode = imageEngine.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask);
                Console.WriteLine("InitEngine Result:" + retCode);
                AppendText((retCode == 0) ? "图片引擎初始化成功!" : string.Format("图片引擎初始化失败!错误码为:{0}", retCode));
                if (retCode != 0)
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                }

                //初始化视频模式下人脸检测引擎
                DetectionMode detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
                int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION;
                retCode = videoEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMaskVideo);
                AppendText((retCode == 0) ? "视频引擎初始化成功!" : string.Format("视频引擎初始化失败!错误码为:{0}", retCode));
                if (retCode != 0)
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                }

                //RGB视频专用FR引擎
                combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS;
                retCode = videoRGBImageEngine.ASFInitEngine(detectMode, videoDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask);
                AppendText((retCode == 0) ? "RGB处理引擎初始化成功!" : string.Format("RGB处理引擎初始化失败!错误码为:{0}", retCode));
                if (retCode != 0)
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                }
                //设置活体阈值
                videoRGBImageEngine.ASFSetLivenessParam(0.5f);

                //IR视频专用FR引擎
                combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_IR_LIVENESS;
                retCode = videoIRImageEngine.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask);
                AppendText((retCode == 0) ? "IR处理引擎初始化成功!\r\n" : string.Format("IR处理引擎初始化失败!错误码为:{0}\r\n", retCode));
                if (retCode != 0)
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                }
                //设置活体阈值
                videoIRImageEngine.ASFSetLivenessParam(0.5f, 0.7f);

                initVideo();
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
                MessageBox.Show("程序初始化异常,请在App.config中修改日志配置,根据日志查找原因!");
                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// 摄像头初始化
        /// </summary>
        private void initVideo()
        {
            try
            {
                filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                //如果没有可用摄像头，“启用摄像头”按钮禁用，否则使可用
                btnStartVideo.Enabled = filterInfoCollection.Count != 0;
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion
        
        #region 注册人脸按钮事件        
        /// <summary>
        /// 人脸库图片选择按钮事件
        /// </summary>
        private void ChooseMultiImg(object sender, EventArgs e)
        {
            try
            {
                lock (chooseImgLocker)
                {                    
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Title = "选择图片";
                    openFileDialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png";
                    openFileDialog.Multiselect = true;
                    openFileDialog.FileName = string.Empty;
                    imageList.Refresh();
                    if (openFileDialog.ShowDialog().Equals(DialogResult.OK))
                    {

                        List<string> imagePathListTemp = new List<string>();
                        var numStart = imagePathList.Count;
                        int isGoodImage = 0;                       

                        //人脸检测以及提取人脸特征
                        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                        {
                            //禁止点击按钮
                            Invoke(new Action(delegate
                            {
                                ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn, btnStartVideo);
                            }));

                            //保存图片路径并显示
                            string[] fileNames = openFileDialog.FileNames;
                            for (int i = 0; i < fileNames.Length; i++)
                            {
                                //图片格式判断
                                if (CheckImage(fileNames[i]))
                                {
                                    imagePathListTemp.Add(fileNames[i]);
                                }
                            }
                            //人脸检测和剪裁
                            for (int i = 0; i < imagePathListTemp.Count; i++)
                            {
                                Image image = ImageUtil.ReadFromFile(imagePathListTemp[i]);
                                //校验图片宽高
                                CheckImageWidthAndHeight(ref image);
                                if (image == null)
                                {
                                    continue;
                                }
                                //调整图像宽度，需要宽度为4的倍数
                                if (image.Width % 4 != 0)
                                {
                                    image = ImageUtil.ScaleImage(image, image.Width - (image.Width % 4), image.Height);
                                }
                                //提取特征判断
                                int featureCode = -1;
                                SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                                FaceFeature feature = FaceUtil.ExtractFeature(imageEngine, image, out singleFaceInfo, ref featureCode);
                                if (featureCode != 0)
                                {
                                    this.Invoke(new Action(delegate
                                    {
                                        AppendText("未检测到人脸");
                                    }));
                                    if (image != null)
                                    {
                                        image.Dispose();
                                    }
                                    continue;
                                }
                                //人脸检测
                                MultiFaceInfo multiFaceInfo;
                                int retCode = imageEngine.ASFDetectFacesEx(image, out multiFaceInfo);
                                //判断检测结果
                                if (retCode == 0 && multiFaceInfo.faceNum > 0)
                                {
                                    //多人脸时，默认裁剪第一个人脸
                                    imagePathList.Add(imagePathListTemp[i]);
                                    MRECT rect = multiFaceInfo.faceRects[0];
                                    image = ImageUtil.CutImage(image, rect.left, rect.top, rect.right, rect.bottom);
                                }
                                else
                                {
                                    this.Invoke(new Action(delegate
                                    {
                                        AppendText("未检测到人脸");
                                    }));
                                    if (image != null)
                                    {
                                        image.Dispose();
                                    }
                                    continue;
                                }
                                
                                //显示人脸
                                this.Invoke(new Action(delegate
                                {
                                    if (image == null)
                                    {
                                        image = ImageUtil.ReadFromFile(imagePathListTemp[i]);
                                        //校验图片宽高
                                        CheckImageWidthAndHeight(ref image);
                                    }
                                    imageLists.Images.Add(imagePathListTemp[i], image);
                                    imageList.Items.Add((numStart + isGoodImage) + "号", imagePathListTemp[i]);
                                    imageList.Refresh();
                                    AppendText(string.Format("已提取{0}号人脸特征值，[left:{1},right:{2},top:{3},bottom:{4},orient:{5}]", (numStart + isGoodImage), singleFaceInfo.faceRect.left, singleFaceInfo.faceRect.right, singleFaceInfo.faceRect.top, singleFaceInfo.faceRect.bottom, singleFaceInfo.faceOrient));
                                    leftImageFeatureList.Add(feature);
                                    isGoodImage++;
                                    if (image != null)
                                    {
                                        image.Dispose();
                                    }
                                }));
                            }
                            //允许点击按钮
                            Invoke(new Action(delegate
                            {
                                ControlsEnable(true, chooseMultiImgBtn, btnClearFaceList, btnStartVideo);
                                ControlsEnable(("启用摄像头".Equals(btnStartVideo.Text)), chooseImgBtn, matchBtn);
                            }));
                        }));

                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion

        #region 清空人脸库按钮事件
        /// <summary>
        /// 清除人脸库事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearFaceList_Click(object sender, EventArgs e)
        {
            try
            {
                //清除数据
                imageLists.Images.Clear();
                imageList.Items.Clear();
                leftImageFeatureList.Clear();
                imagePathList.Clear();
                GC.Collect();
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion

        #region 选择识别图按钮事件
        /// <summary>
        /// “选择识别图片”按钮事件
        /// </summary>
        private void ChooseImg(object sender, EventArgs e)
        {
            try
            {
                lblCompareInfo.Text = string.Empty;
                //判断引擎是否初始化成功
                if (!imageEngine.GetEngineStatus())
                {
                    //禁用相关功能按钮
                    ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                    MessageBox.Show("请先初始化引擎!");
                    return;
                }
                //选择图片
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择图片";
                openFileDialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png";
                openFileDialog.Multiselect = false;
                openFileDialog.FileName = string.Empty;
                if (openFileDialog.ShowDialog().Equals(DialogResult.OK))
                {

                    image1Path = openFileDialog.FileName;
                    //检测图片格式
                    if (!CheckImage(image1Path))
                    {
                        return;
                    }
                    DateTime detectStartTime = DateTime.Now;
                    AppendText(string.Format("------------------------------开始检测，时间:{0}------------------------------", detectStartTime.ToString("yyyy-MM-dd HH:mm:ss:ms")));

                    //获取文件，拒绝过大的图片
                    FileInfo fileInfo = new FileInfo(image1Path);
                    if (fileInfo.Length > maxSize)
                    {
                        MessageBox.Show("图像文件最大为2MB，请压缩后再导入!");
                        AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                        AppendText("");
                        return;
                    }

                    Image srcImage = ImageUtil.ReadFromFile(image1Path);
                    //校验图片宽高
                    CheckImageWidthAndHeight(ref srcImage);
                    if (srcImage == null)
                    {
                        MessageBox.Show("图像数据获取失败，请稍后重试!");
                        AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                        AppendText("");
                        return;
                    }
                    //调整图像宽度，需要宽度为4的倍数
                    if (srcImage.Width % 4 != 0)
                    {
                        srcImage = ImageUtil.ScaleImage(srcImage, srcImage.Width - (srcImage.Width % 4), srcImage.Height);
                    }
                    //人脸检测
                    MultiFaceInfo multiFaceInfo;
                    int retCode = imageEngine.ASFDetectFacesEx(srcImage, out multiFaceInfo);
                    if (retCode != 0)
                    {
                        MessageBox.Show("图像人脸检测失败，请稍后重试!");
                        AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                        AppendText("");
                        return;
                    }
                    if (multiFaceInfo.faceNum < 1)
                    {
                        srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                        rightImageFeatureList.Clear();
                        picImageCompare.Image = srcImage;
                        AppendText(string.Format("{0} - 未检测出人脸!\r\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                        AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                        AppendText("");
                        return;
                    }

                    //年龄检测
                    int retCode_Age = -1;
                    AgeInfo ageInfo = FaceUtil.AgeEstimation(imageEngine, srcImage, multiFaceInfo, out retCode_Age);
                    //性别检测
                    int retCode_Gender = -1;
                    GenderInfo genderInfo = FaceUtil.GenderEstimation(imageEngine, srcImage, multiFaceInfo, out retCode_Gender);
                    //3DAngle检测
                    int retCode_3DAngle = -1;
                    Face3DAngle face3DAngleInfo = FaceUtil.Face3DAngleDetection(imageEngine, srcImage, multiFaceInfo, out retCode_3DAngle);

                    AppendText(string.Format("{0} - 人脸数量:{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), multiFaceInfo.faceNum));

                    MRECT[] mrectTemp = new MRECT[multiFaceInfo.faceNum];
                    int[] ageTemp = new int[multiFaceInfo.faceNum];
                    int[] genderTemp = new int[multiFaceInfo.faceNum];
                    SingleFaceInfo singleFaceInfo;

                    //标记出检测到的人脸
                    for (int i = 0; i < multiFaceInfo.faceNum; i++)
                    {
                        MRECT rect = multiFaceInfo.faceRects[i];
                        int orient = multiFaceInfo.faceOrients[i];
                        int age = 0;
                        //年龄检测
                        if (retCode_Age != 0)
                        {
                            AppendText(string.Format("年龄检测失败，返回{0}!", retCode_Age));
                        }
                        else
                        {
                            age = ageInfo.ageArray[i];
                        }
                        //性别检测
                        int gender = -1;
                        if (retCode_Gender != 0)
                        {
                            AppendText(string.Format("性别检测失败，返回{0}!", retCode_Gender));
                        }
                        else
                        {
                            gender = genderInfo.genderArray[i];
                        }
                        //3DAngle检测
                        int face3DStatus = -1;
                        float roll = 0f;
                        float pitch = 0f;
                        float yaw = 0f;
                        if (retCode_3DAngle != 0)
                        {
                            AppendText(string.Format("3DAngle检测失败，返回{0}!", retCode_3DAngle));
                        }
                        else
                        {
                            //角度状态 非0表示人脸不可信
                            face3DStatus = face3DAngleInfo.status[i];
                            //roll为侧倾角，pitch为俯仰角，yaw为偏航角
                            roll = face3DAngleInfo.roll[i];
                            pitch = face3DAngleInfo.pitch[i];
                            yaw = face3DAngleInfo.yaw[i];
                        }
                        //提取人脸特征
                        rightImageFeatureList.Add(FaceUtil.ExtractFeature(imageEngine, srcImage, out singleFaceInfo, ref retCode, i));

                        mrectTemp[i] = rect;
                        ageTemp[i] = age;
                        genderTemp[i] = gender;
                        AppendText(string.Format("{0} - 第{12}人脸坐标:[left:{1},top:{2},right:{3},bottom:{4},orient:{5},roll:{6},pitch:{7},yaw:{8},status:{11}] Age:{9} Gender:{10}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), rect.left, rect.top, rect.right, rect.bottom, orient, roll, pitch, yaw, age, (gender >= 0 ? gender.ToString() : ""), face3DStatus, i));
                    }
                    AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    AppendText("");

                    //清空上次的匹配结果
                    for (int i = 0; i < leftImageFeatureList.Count; i++)
                    {
                        imageList.Items[i].Text = string.Format("{0}号", i);
                    }
                    //获取缩放比例
                    float scaleRate = ImageUtil.GetWidthAndHeight(srcImage.Width, srcImage.Height, picImageCompare.Width, picImageCompare.Height);
                    //缩放图片
                    srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                    //添加标记
                    srcImage = ImageUtil.MarkRectAndString(srcImage, mrectTemp, ageTemp, genderTemp, picImageCompare.Width, scaleRate, multiFaceInfo.faceNum);

                    //显示标记后的图像
                    picImageCompare.Image = srcImage;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion

        #region 开始匹配按钮事件
        /// <summary>
        /// 匹配事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void matchBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (leftImageFeatureList.Count == 0)
                {
                    MessageBox.Show("请注册人脸!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (rightImageFeatureList == null || rightImageFeatureList.Count <= 0)
                {
                    if (picImageCompare.Image == null)
                    {
                        MessageBox.Show("请选择识别图!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("比对失败，识别图未提取到特征值!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
                //标记已经做了匹配比对，在开启视频的时候要清除比对结果
                isCompare = true;
                AppendText(string.Format("------------------------------开始比对，时间:{0}------------------------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                for (int faceIndex = 0; faceIndex < rightImageFeatureList.Count; faceIndex++)
                {
                    float compareSimilarity = 0f;
                    int compareNum = 0;
                    FaceFeature tempFaceFeature = rightImageFeatureList[faceIndex];
                    if (tempFaceFeature.featureSize <= 0)
                    {
                        AppendText(string.Format("比对人脸第{0}张人脸特征提取失败", faceIndex));
                        continue;
                    }
                    AppendText(string.Format("---------开始匹配比对人脸第{0}张人脸---------", faceIndex));
                    for (int i = 0; i < leftImageFeatureList.Count; i++)
                    {
                        FaceFeature feature = leftImageFeatureList[i];
                        float similarity = 0f;
                        imageEngine.ASFFaceFeatureCompare(tempFaceFeature, feature, out similarity);
                        //增加异常值处理
                        if (similarity.ToString().IndexOf("E") > -1)
                        {
                            similarity = 0f;
                        }
                        AppendText(string.Format("与人脸库{0}号比对结果:{1}", i, similarity));
                        if (similarity > compareSimilarity)
                        {
                            compareSimilarity = similarity;
                            compareNum = i;
                        }
                    }
                    if (compareSimilarity > 0)
                    {
                        AppendText(string.Format("匹配结果:{0}号,比对结果:{1}", compareNum, compareSimilarity));
                    }
                }
                AppendText(string.Format("------------------------------比对结束，时间:{0}------------------------------", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion

        #region 视频检测相关(<摄像头按钮点击事件、摄像头Paint事件、特征比对、摄像头播放完成事件>)

        /// <summary>
        /// 摄像头按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartVideo_Click(object sender, EventArgs e)
        {
            try
            {
                //在点击开始的时候再坐下初始化检测，防止程序启动时有摄像头，在点击摄像头按钮之前将摄像头拔掉的情况
                initVideo();
                //必须保证有可用摄像头
                if (filterInfoCollection.Count == 0)
                {
                    MessageBox.Show("未检测到摄像头，请确保已安装摄像头或驱动!");
                    return;
                }
                if (rgbVideoSource.IsRunning || irVideoSource.IsRunning)
                {
                    btnStartVideo.Text = "启用摄像头";
                    //关闭摄像头
                    if (irVideoSource.IsRunning)
                    {
                        irVideoSource.SignalToStop();
                        irVideoSource.Hide();
                    }
                    if (rgbVideoSource.IsRunning)
                    {
                        rgbVideoSource.SignalToStop();
                        rgbVideoSource.Hide();
                    }
                    //“选择识别图”、“开始匹配”按钮可用，阈值控件禁用
                    ControlsEnable(true, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
                    txtThreshold.Enabled = false;
                    exitVideoRGBFR = true;
                    exitVideoRGBLiveness = true;
                    exitVideoIRFRLiveness = true;
                }
                else
                {
                    if (isCompare)
                    {
                        //比对结果清除
                        for (int i = 0; i < leftImageFeatureList.Count; i++)
                        {
                            imageList.Items[i].Text = string.Format("{0}号", i);
                        }
                        lblCompareInfo.Text = string.Empty;
                        isCompare = false;
                    }
                    //“选择识别图”、“开始匹配”按钮禁用，阈值控件可用，显示摄像头控件
                    txtThreshold.Enabled = true;
                    rgbVideoSource.Show();
                    irVideoSource.Show();
                    ControlsEnable(false, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
                    btnStartVideo.Text = "关闭摄像头";
                    //获取filterInfoCollection的总数
                    int maxCameraCount = filterInfoCollection.Count;
                    //如果配置了两个不同的摄像头索引
                    if (rgbCameraIndex != irCameraIndex && maxCameraCount >= 2)
                    {
                        //RGB摄像头加载
                        rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex < maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                        rgbVideoSource.VideoSource = rgbDeviceVideo;
                        rgbVideoSource.Start();

                        //IR摄像头
                        irDeviceVideo = new VideoCaptureDevice(filterInfoCollection[irCameraIndex < maxCameraCount ? irCameraIndex : 0].MonikerString);
                        irVideoSource.VideoSource = irDeviceVideo;
                        irVideoSource.Start();
                        //双摄标志设为true
                        isDoubleShot = true;
                        //启动检测线程
                        exitVideoIRFRLiveness = false;
                        videoIRLiveness();
                    }
                    else
                    {
                        //仅打开RGB摄像头，IR摄像头控件隐藏
                        rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex <= maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                        rgbVideoSource.VideoSource = rgbDeviceVideo;
                        rgbVideoSource.Start();
                        irVideoSource.Hide();
                    }
                    //启动两个检测线程
                    exitVideoRGBFR = false;
                    exitVideoRGBLiveness = false;
                    videoRGBLiveness();
                    videoRGBFR();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// RGB摄像头Paint事件，图像显示到窗体上，得到每一帧图像，并进行处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoSource_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (!rgbVideoSource.IsRunning)
                {
                    return;
                }
                //得到当前RGB摄像头下的图片
                lock (rgbVideoImageLocker)
                {
                    rgbVideoBitmap = rgbVideoSource.GetCurrentVideoFrame();
                }
                Bitmap bitmapClone = null;
                try
                {
                    lock (rgbVideoImageLocker)
                    {
                        if (rgbVideoBitmap == null)
                        {
                            return;
                        }
                        bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                    }
                    if(bitmapClone == null)
                    {
                        return;
                    }
                    //检测人脸，得到Rect框
                    MultiFaceInfo multiFaceInfo = FaceUtil.DetectFace(videoEngine, bitmapClone);
                    //未检测到人脸
                    if (multiFaceInfo.faceNum <= 0)
                    {
                        trackRGBUnitDict.ClearAllElement();
                        return;
                    }
                    Graphics g = e.Graphics;
                    float offsetX = rgbVideoSource.Width * 1f / bitmapClone.Width;
                    float offsetY = rgbVideoSource.Height * 1f / bitmapClone.Height;
                    List<int> tempIdList = new List<int>();
                    for (int faceIndex = 0; faceIndex < multiFaceInfo.faceNum; faceIndex++)
                    {
                        MRECT mrect = multiFaceInfo.faceRects[faceIndex];
                        float x = mrect.left * offsetX;
                        float width = mrect.right * offsetX - x;
                        float y = mrect.top * offsetY;
                        float height = mrect.bottom * offsetY - y;
                        int faceId = multiFaceInfo.faceID[faceIndex];
                        FaceTrackUnit currentFaceTrack = trackRGBUnitDict.GetElementByKey(faceId);
                        //根据Rect进行画框
                        //将上一帧检测结果显示到页面上
                        lock (rgbVideoImageLocker)
                        {
                            if (currentFaceTrack != null)
                            {
                                g.DrawRectangle(currentFaceTrack.CertifySuccess() ? Pens.Green : Pens.Red, x, y, width, height);
                                if (!string.IsNullOrWhiteSpace(currentFaceTrack.GetCombineMessage()) && x > 0 && y > 0)
                                {
                                    g.DrawString(currentFaceTrack.GetCombineMessage(), font, currentFaceTrack.CertifySuccess() ? greenBrush : redBrush, x, y - 15);
                                }
                            }
                            else
                            {
                                g.DrawRectangle(Pens.Red, x, y, width, height);
                            }
                        }
                        if (faceId >= 0)
                        {
                            //判断faceid是否加入待处理队列
                            if (!rgbFeatureTryDict.ContainsKey(faceId))
                            {
                                rgbFeatureTryDict.AddDictionaryElement(faceId, 0);
                            }
                            if (!rgbLivenessTryDict.ContainsKey(faceId))
                            {
                                rgbLivenessTryDict.AddDictionaryElement(faceId, 0);
                            }
                            if (trackRGBUnitDict.ContainsKey(faceId))
                            {
                                trackRGBUnitDict.GetElementByKey(faceId).Rect = mrect;
                                trackRGBUnitDict.GetElementByKey(faceId).FaceOrient = multiFaceInfo.faceOrients[faceIndex];
                            }
                            else
                            {
                                trackRGBUnitDict.AddDictionaryElement(faceId, new FaceTrackUnit(faceId, mrect, multiFaceInfo.faceOrients[faceIndex]));
                            }
                            tempIdList.Add(faceId);
                        }
                        
                    }
                    //初始化及刷新待处理队列,移除出框的人脸
                    rgbFeatureTryDict.RefershElements(tempIdList);
                    rgbLivenessTryDict.RefershElements(tempIdList);
                    trackRGBUnitDict.RefershElements(tempIdList);
                }
                catch (Exception ee)
                {
                    LogUtil.LogInfo(GetType(), ee);
                }
                finally
                {
                    if (bitmapClone != null)
                    {
                        bitmapClone.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// 活体检测线程
        /// </summary>
        private void videoRGBLiveness()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                while (true)
                {
                    if (exitVideoRGBLiveness)
                    {
                        return;
                    }
                    if (rgbLivenessTryDict.GetDictCount() <= 0)
                    {
                        continue;
                    }
                    try
                    {
                        if (rgbVideoBitmap == null)
                        {
                            continue;
                        }
                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbLivenessTryDict.GetAllElement().Keys);
                        //遍历人脸Id，进行活体检测
                        foreach (int tempFaceId in faceIdList)
                        {
                            //待处理队列中不存在，移除
                            if (!rgbLivenessTryDict.ContainsKey(tempFaceId))
                            {
                                continue;
                            }
                            //大于尝试次数，移除
                            int tryTime = rgbLivenessTryDict.GetElementByKey(tempFaceId);
                            if (tryTime >= liveMatchTime)
                            {
                                continue;
                            }
                            tryTime += 1;
                            //无对应的人脸框信息
                            if (!trackRGBUnitDict.ContainsKey(tempFaceId))
                            {
                                continue;
                            }
                            FaceTrackUnit tempFaceTrack = trackRGBUnitDict.GetElementByKey(tempFaceId);

                            //RGB活体检测
                            Console.WriteLine(string.Format("faceId:{0},活体检测第{1}次\r\n", tempFaceId, tryTime));
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                            singleFaceInfo.faceID = tempFaceId;
                            singleFaceInfo.faceOrient = tempFaceTrack.FaceOrient;
                            singleFaceInfo.faceRect = tempFaceTrack.Rect;
                            Bitmap bitmapClone = null;
                            try
                            {
                                lock (rgbVideoImageLocker)
                                {
                                    if (rgbVideoBitmap == null)
                                    {
                                        break;
                                    }
                                    bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                                }
                                int retCodeLiveness = -1;
                                LivenessInfo liveInfo = FaceUtil.LivenessInfo_RGB(videoRGBImageEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                                //更新活体检测结果
                                if (retCodeLiveness.Equals(0) && liveInfo.num > 0 && trackRGBUnitDict.ContainsKey(tempFaceId))
                                {
                                    trackRGBUnitDict.GetElementByKey(tempFaceId).RgbLiveness = liveInfo.isLive[0];
                                    if (liveInfo.isLive[0].Equals(1))
                                    {
                                        tryTime = liveMatchTime;
                                    }
                                }
                            }
                            catch (Exception ee)
                            {
                                LogUtil.LogInfo(GetType(), ee);
                            }
                            finally
                            {
                                if (bitmapClone != null)
                                {
                                    bitmapClone.Dispose();
                                }
                            }
                            rgbLivenessTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogInfo(GetType(), ex);
                    }
                }
            }));
        }

        /// <summary>
        /// 特征提取和搜索线程
        /// </summary>
        private void videoRGBFR()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                while (true)
                {
                    if (exitVideoRGBFR)
                    {
                        return;
                    }
                    if (rgbFeatureTryDict.GetDictCount() <= 0)
                    {
                        continue;
                    }
                    //左侧人脸库为空时，不用进行特征搜索
                    if (leftImageFeatureList.Count <= 0)
                    {
                        continue;
                    }
                    try
                    {
                        if (rgbVideoBitmap == null)
                        {
                            continue;
                        }
                        List<int> faceIdList = new List<int>();
                        faceIdList.AddRange(rgbFeatureTryDict.GetAllElement().Keys);
                        foreach (int tempFaceId in faceIdList)
                        {
                            //待处理队列中不存在，移除
                            if (!rgbFeatureTryDict.ContainsKey(tempFaceId))
                            {
                                continue;
                            }
                            //大于尝试次数，移除
                            int tryTime = rgbFeatureTryDict.GetElementByKey(tempFaceId);
                            if (tryTime >= frMatchTime)
                            {
                                continue;
                            }
                            //无对应的人脸框信息
                            if (!trackRGBUnitDict.ContainsKey(tempFaceId))
                            {
                                continue;
                            }
                            FaceTrackUnit tempFaceTrack = trackRGBUnitDict.GetElementByKey(tempFaceId);
                            tryTime += 1;
                            //特征搜索
                            int faceIndex = -1;
                            float similarity = 0f;
                            Console.WriteLine(string.Format("faceId:{0},特征搜索第{1}次\r\n", tempFaceId, tryTime));
                            //提取人脸特征
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                            singleFaceInfo.faceID = tempFaceId;
                            singleFaceInfo.faceOrient = tempFaceTrack.FaceOrient;
                            singleFaceInfo.faceRect = tempFaceTrack.Rect;
                            Bitmap bitmapClone = null;
                            try
                            {
                                lock (rgbVideoImageLocker)
                                {
                                    if (rgbVideoBitmap == null)
                                    {
                                        break;
                                    }
                                    bitmapClone = (Bitmap)rgbVideoBitmap.Clone();
                                }
                                FaceFeature feature = FaceUtil.ExtractFeature(videoRGBImageEngine, bitmapClone, singleFaceInfo);
                                if (feature.featureSize <= 0)
                                {
                                    break;
                                }
                                //特征搜索
                                faceIndex = compareFeature(feature, out similarity);
                                //更新比对结果
                                if (trackRGBUnitDict.ContainsKey(tempFaceId))
                                {
                                    trackRGBUnitDict.GetElementByKey(tempFaceId).SetFaceIndexAndSimilarity(faceIndex, similarity.ToString("#0.00"));
                                    if (faceIndex > -1)
                                    {
                                        tryTime = frMatchTime;
                                    }
                                }
                            }
                            catch (Exception ee)
                            {
                                LogUtil.LogInfo(GetType(), ee);
                            }
                            finally
                            {
                                if (bitmapClone != null)
                                {
                                    bitmapClone.Dispose();
                                }
                            }
                            rgbFeatureTryDict.UpdateDictionaryElement(tempFaceId, tryTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogInfo(GetType(), ex);
                    }
                }
            }));
        }

        /// <summary>
        /// IR摄像头Paint事件,同步RGB人脸框，对比人脸框后进行IR活体检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void irVideoSource_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (!isDoubleShot || !irVideoSource.IsRunning)
                {
                    return;
                }
                //如果双摄，且IR摄像头工作，获取IR摄像头图片
                lock (irVideoImageLocker)
                {
                    irVideoBitmap = irVideoSource.GetCurrentVideoFrame();
                }
                Bitmap irBmpClone = null;
                try
                {
                    lock (irVideoImageLocker)
                    {
                        if (irVideoBitmap == null)
                        {
                            return;
                        }
                        irBmpClone = (Bitmap)irVideoBitmap.Clone();
                    }
                    if (irBmpClone == null)
                    {
                        return;
                    }
                    //校验图片宽高
                    CheckBitmapWidthAndHeight(ref irBmpClone);
                    //检测人脸，得到Rect框
                    MultiFaceInfo multiFaceInfo = FaceUtil.DetectFaceIR(videoIRImageEngine, irBmpClone);
                    if (multiFaceInfo.faceNum <= 0)
                    {
                        trackIRUnit.FaceId = -1;
                        return;
                    }
                    //得到最大人脸
                    SingleFaceInfo irMaxFace = FaceUtil.GetMaxFace(multiFaceInfo);
                    //得到Rect
                    MRECT rect = irMaxFace.faceRect;
                    //检测RGB摄像头下最大人脸
                    Graphics g = e.Graphics;
                    float offsetX = irVideoSource.Width * 1f / irBmpClone.Width;
                    float offsetY = irVideoSource.Height * 1f / irBmpClone.Height;
                    float x = rect.left * offsetX;
                    float width = rect.right * offsetX - x;
                    float y = rect.top * offsetY;
                    float height = rect.bottom * offsetY - y;
                    //根据Rect进行画框
                    lock (irVideoImageLocker)
                    {
                        g.DrawRectangle(trackIRUnit.IrLiveness.Equals(1) ? Pens.Green : Pens.Red, x, y, width, height);
                        if (!string.IsNullOrWhiteSpace(trackIRUnit.GetIrLivenessMessage()) && x > 0 && y > 0)
                        {
                            //将上一帧检测结果显示到页面上
                            g.DrawString(trackIRUnit.GetIrLivenessMessage(), font, trackIRUnit.IrLiveness.Equals(1) ? greenBrush : redBrush, x, y - 15);
                        }
                    }
                    trackIRUnit.Rect = irMaxFace.faceRect;
                    trackIRUnit.FaceId = irMaxFace.faceID;
                    trackIRUnit.FaceOrient = irMaxFace.faceOrient;
                }
                catch (Exception ee)
                {
                    LogUtil.LogInfo(GetType(), ee);
                }
                finally
                {
                    if (irBmpClone != null)
                    {
                        irBmpClone.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// IR活体检测线程
        /// </summary>
        private void videoIRLiveness()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                while (true)
                {
                    if (exitVideoIRFRLiveness)
                    {
                        return;
                    }
                    try
                    {                        
                        Bitmap bitmapClone = null;
                        try
                        {
                            lock (irVideoImageLocker)
                            {
                                if (irVideoBitmap == null)
                                {
                                    continue;
                                }
                                bitmapClone = (Bitmap)irVideoBitmap.Clone();
                                if (bitmapClone == null)
                                {
                                    continue;
                                }
                            }
                            SingleFaceInfo singleFaceInfo = new SingleFaceInfo();
                            singleFaceInfo.faceID = trackIRUnit.FaceId;
                            singleFaceInfo.faceOrient = trackIRUnit.FaceOrient;
                            singleFaceInfo.faceRect = trackIRUnit.Rect;
                            int retCodeLiveness = -1;
                            LivenessInfo liveInfo = FaceUtil.LivenessInfo_IR(videoIRImageEngine, bitmapClone, singleFaceInfo, out retCodeLiveness);
                            if (retCodeLiveness.Equals(0) && liveInfo.num > 0)
                            {
                                trackIRUnit.IrLiveness = liveInfo.isLive[0];
                            }
                        }
                        catch (Exception ee)
                        {
                            LogUtil.LogInfo(GetType(), ee);
                        }
                        finally
                        {
                            if (bitmapClone != null)
                            {
                                bitmapClone.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogInfo(GetType(), ex);
                    }
                }
            }));
        }

        /// <summary>
        /// 得到feature比较结果
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private int compareFeature(FaceFeature feature, out float similarity)
        {
            int result = -1;
            similarity = 0f;
            try
            {
                //如果人脸库不为空，则进行人脸匹配
                if (leftImageFeatureList != null && leftImageFeatureList.Count > 0)
                {
                    for (int i = 0; i < leftImageFeatureList.Count; i++)
                    {
                        //调用人脸匹配方法，进行匹配
                        videoRGBImageEngine.ASFFaceFeatureCompare(feature, leftImageFeatureList[i], out similarity);
                        if (similarity >= threshold)
                        {
                            result = i;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
            return result;
        }

        /// <summary>
        /// 摄像头播放完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        private void videoSource_PlayingFinished(object sender, AForge.Video.ReasonToFinishPlaying reason)
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;
                ControlsEnable(true, chooseImgBtn, matchBtn, chooseMultiImgBtn, btnClearFaceList);
                exitVideoRGBFR = true;
                exitVideoRGBLiveness = true;
                exitVideoIRFRLiveness = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        #endregion

        #region 界面阈值相关
        /// <summary>
        /// 阈值文本框键按下事件，检测输入内容是否正确，不正确不能输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtThreshold_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                //阻止从键盘输入键
                e.Handled = true;
                //是数值键，回退键，.能输入，其他不能输入
                if (char.IsDigit(e.KeyChar) || e.KeyChar == 8 || e.KeyChar == '.')
                {
                    //渠道当前文本框的内容
                    string thresholdStr = txtThreshold.Text.Trim();
                    int countStr = 0;
                    int startIndex = 0;
                    //如果当前输入的内容是否是“.”
                    if (e.KeyChar == '.')
                    {
                        countStr = 1;
                    }
                    //检测当前内容是否含有.的个数
                    if (thresholdStr.IndexOf('.', startIndex) > -1)
                    {
                        countStr += 1;
                    }
                    //如果输入的内容已经超过12个字符，
                    if (e.KeyChar != 8 && (thresholdStr.Length > 12 || countStr > 1))
                    {
                        return;
                    }
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// 阈值文本框键抬起事件，检测阈值是否正确，不正确改为0.8f
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtThreshold_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                //如果输入的内容不正确改为默认值
                if (!float.TryParse(txtThreshold.Text.Trim(), out threshold))
                {
                    threshold = 0.8f;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        #endregion

        #region 窗体关闭
        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        private void Form_Closed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (rgbVideoSource.IsRunning)
                {
                    btnStartVideo_Click(sender, e); //关闭摄像头
                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 恢复使用/禁用控件列表控件
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="controls">控件列表</param>
        private void ControlsEnable(bool isEnable, params Control[] controls)
        {
            try
            {
                if (controls == null || controls.Length <= 0)
                {
                    return;
                }
                foreach (Control control in controls)
                {
                    control.Enabled = isEnable;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// 校验图片
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private bool CheckImage(string imagePath)
        {
            try
            {
                if (imagePath == null)
                {
                    AppendText("图片不存在，请确认后再导入");
                    return false;
                }
                try
                {
                    //判断图片是否正常，如将其他文件把后缀改为.jpg，这样就会报错
                    Image image = ImageUtil.ReadFromFile(imagePath);
                    if (image == null)
                    {
                        throw new ArgumentException(" image is null");
                    }
                    else
                    {
                        image.Dispose();
                    }
                }
                catch
                {
                    AppendText(string.Format("{0} 图片格式有问题，请确认后再导入", imagePath));
                    return false;
                }
                FileInfo fileCheck = new FileInfo(imagePath);
                if (!fileCheck.Exists)
                {
                    AppendText(string.Format("{0} 不存在", fileCheck.Name));
                    return false;
                }
                else if (fileCheck.Length > maxSize)
                {
                    AppendText(string.Format("{0} 图片大小超过2M，请压缩后再导入", fileCheck.Name));
                    return false;
                }
                else if (fileCheck.Length < 2)
                {
                    AppendText(string.Format("{0} 图像质量太小，请重新选择", fileCheck.Name));
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
            return true;
        }

        /// <summary>
        /// 追加公用方法
        /// </summary>
        /// <param name="message"></param>
        private void AppendText(string message)
        {
            logBox.AppendText(message+"\r\n");
        }

        /// <summary>
        /// 检查图片宽高
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private void CheckImageWidthAndHeight(ref Image image)
        {
            if (image == null)
            {
                return;
            }
            try
            {
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image = ImageUtil.ScaleImage(image, maxWidth, maxHeight);
                }
            }
            catch(Exception ex) {
                LogUtil.LogInfo(GetType(), ex);
            }
        }

        /// <summary>
        /// 检查图片宽高
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private void CheckBitmapWidthAndHeight(ref Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return;
            }
            try
            {
                if (bitmap.Width > maxWidth || bitmap.Height > maxHeight)
                {
                    bitmap = (Bitmap)ImageUtil.ScaleImage(bitmap, maxWidth, maxHeight);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo(GetType(), ex);
            }
        }
        #endregion
        
    }
}
