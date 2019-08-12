/// <summary>
/// Image Exporter(序列图导出)
/// Version 1.0.9
/// created 2016/9/22
/// updated 2017/10/10
/// </summary>

namespace taecg.tools.ImageExporter
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.IO;

    public class ImageExporterEditorWindow : EditorWindow
    {
        #region 本地化语言
        private static string NAME_windowTitle{
            get
            {
                switch(Application.systemLanguage)
                {
                    case SystemLanguage.Chinese:
                        return "序列图导出";
                    default:
                        return "Image Exporter";
                }
            }
        }

        private string NAME_currectFrame;
        private string NAME_selectedCamera;
        private string NAME_setFormat;
        private string NAME_isEnableAlpha;
        private string NAME_setFormatDescribution;
        private string NAME_setResolution;
        private string NAME_setDefaultSize;
        private string NAME_setCurrectSize;
        private string NAME_setFileName;
        private string NAME_setSavePath;
        private string NAME_takeScreenShot;
        private string NAME_takeScreenShotDescribution;
        private string NAME_setEnableSequence;
        private string NAME_setFrame;
        private string NAME_setFrameRange;
        private string NAME_startExport;
        private string NAME_startExportDescribution;
        private string NAME_openFolder;
        private string NAME_credit;
        #endregion

        private string productName
        {
            get
            {
                return Application.productName;
            }
        }

        private bool isEnabled;
        private Camera cam;
        private Vector2 resolution;
        private int frameCount = 30;
        private string[] frameCountStrArry = new string[]{ "12", "30", "60" };
        private int[] frameCountIntArry = new int[]{ 12, 30, 60 };

        private enum ImageFormat
        {
            PNG,
            JPG
        }

        private ImageFormat theImageFormat = ImageFormat.PNG;

        private bool isEnabledAlpha;
        private string fileName="screenShot";
        private string filePath = "";
        private int rangeStart = 1;
        private int rangeEnd = 100;
        private float progress;
        private bool isEndStop;

        [MenuItem("Window/Taecg/Image Exporter(序列图导出) %M")]
        // Use this for initialization
        static void Open()
        {
            ImageExporterEditorWindow window = (ImageExporterEditorWindow)EditorWindow.GetWindow(typeof(ImageExporterEditorWindow),false);
            window.minSize = new Vector2(300, 480);
            window.titleContent = new GUIContent(NAME_windowTitle);
            window.Show();
        }

        void OnEnable()
        {
            InitLanguage();

            Application.runInBackground = true;
            Time.captureFramerate = frameCount;

            if (EditorPrefs.HasKey(productName + "_frameCount"))
                frameCount = EditorPrefs.GetInt(productName + "_frameCount");
            else
                frameCount = 30;

            if (EditorPrefs.HasKey(productName + "_resolutionX"))
                resolution.x = EditorPrefs.GetInt(productName + "_resolutionX");
            else
                resolution.x = 1280;

            if (EditorPrefs.HasKey(productName + "_resolutionY"))
                resolution.y = EditorPrefs.GetInt(productName + "_resolutionY");
            else
                resolution.y = 720;

            if (EditorPrefs.HasKey(productName + "_enabledAlpha"))
                isEnabledAlpha= EditorPrefs.GetBool(productName + "_enabledAlpha");
            else
                isEnabledAlpha = false;

            if (EditorPrefs.HasKey(productName + "_fileName"))
                fileName = EditorPrefs.GetString(productName + "_fileName");
            else
                fileName = "screenShot";

            if (EditorPrefs.HasKey(productName + "_filePath"))
                filePath = EditorPrefs.GetString(productName + "_filePath");
            else
                filePath = Application.dataPath;

            if (EditorPrefs.HasKey(productName + "_rangeStart"))
                rangeStart = EditorPrefs.GetInt(productName + "_rangeStart");
            else
                rangeStart = 0;

            if (EditorPrefs.HasKey(productName + "_rangeEnd"))
                rangeEnd = EditorPrefs.GetInt(productName + "_rangeEnd");
            else
                rangeEnd = 100;

        }

        void OnDisable()
        {
            EditorPrefs.SetString(productName + "_fileName", fileName);
            EditorPrefs.SetInt(productName + "_frameCount", frameCount);
            EditorPrefs.SetInt(productName + "_resolutionX", (int)resolution.x);
            EditorPrefs.SetInt(productName + "_resolutionY", (int)resolution.y);
            EditorPrefs.SetBool(productName + "_enabledAlpha", isEnabledAlpha);
            EditorPrefs.SetString(productName + "_filePath", filePath);
            EditorPrefs.SetInt(productName + "_rangeStart", rangeStart);
            EditorPrefs.SetInt(productName + "_rangeEnd", rangeEnd);
        }

        //界面语言本地化(中文和英文)
        void InitLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                    NAME_currectFrame = "当前帧";
                    NAME_selectedCamera = "选择相机";
                    NAME_setFormat = "设置格式";
                    NAME_isEnableAlpha = "是否启用透明通道";
                    NAME_setFormatDescribution = "特效材质注意2点:\n 1.只有Mobile/Particle中的Shader才能显示出来.\n 2.贴图导出如果有黑底，请将图片的Alpha is Transparent设置正确并勾选。";
                    NAME_setResolution = "设置分辨率";
                    NAME_setDefaultSize = "默认尺寸";
                    NAME_setCurrectSize = "当前屏幕尺寸";
                    NAME_setFileName = "文件名称";
                    NAME_setSavePath = "保存位置";
                    NAME_takeScreenShot = "截取当前面画";
                    NAME_takeScreenShotDescribution = "对当前画面进行单张截图,不在运行模式下也可以截图。";
                    NAME_setEnableSequence = "是否启用导出序列图";
                    NAME_setFrame = "设置帧率";
                    NAME_setFrameRange = "设置起始帧";
                    NAME_startExport = "开始导出序列图";
                    NAME_startExportDescribution = "如果想导序列图的话请勾选\"是否启用导出序列图\"按钮，然后点击\"开始导出序列图\"按钮或者直接运行即可进行序列图导出.";
                    NAME_openFolder = "打开导出文件夹";
                    NAME_credit = "如有任何疑问请联系99U:225367(李红伟) update:2017/10/11";
                    break;
                default:
                    NAME_currectFrame = "Currect Frame";
                    NAME_selectedCamera = "Select Camera";
                    NAME_setFormat = "Set Format";
                    NAME_isEnableAlpha = "Enabled Alpha";
                    NAME_setFormatDescribution = "Special material attention 2 points: \n 1., only the Shader in Mobile/Particle can show,.\n 2. map export, if there is black bottom, please put the pictures of Alpha, is, Transparent settings correctly, and tick.";
                    NAME_setResolution = "Resolution";
                    NAME_setDefaultSize = "Default Size";
                    NAME_setCurrectSize = "Set To Screen Size ";
                    NAME_setFileName = "File Name";
                    NAME_setSavePath = "Save Path";
                    NAME_takeScreenShot = "Take Screenshot";
                    NAME_takeScreenShotDescribution = "On the current screen for a single shot, not in the play mode can also be screenshots.";
                    NAME_setEnableSequence = "Enabled Sequence Export";
                    NAME_setFrame = "Set Frame";
                    NAME_setFrameRange = "Set Range";
                    NAME_startExport = "Start Export Sequence";
                    NAME_startExportDescribution = "If you want to export sequence image, please check the \"Enabled Sequence Export\" button, and then click the \"Start Export Sequence\" button or direct click run button.";
                    NAME_openFolder = "Open Folder";
                    NAME_credit = "by taecg@qq.com update:2017/10/11";
                    break;
            }
        }
			

        // Update is called once per frame
        void Update()
        {
            if (isEnabled)
            {
                if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                {
                    if (Time.frameCount >= rangeStart && Time.frameCount <= rangeEnd)
                    {
                        ImageExporterController sc = GameObject.FindObjectOfType<ImageExporterController>();
                        if (sc == null)
                        {
                            GameObject _obj = new GameObject("ScreenShotController");
                            sc = _obj.AddComponent<ImageExporterController>();
                        }
                        sc.cam = cam;
                        switch (theImageFormat)
                        {
                            case ImageFormat.JPG:
                                sc.imageFormat = ".jpg";
                                break;
                            case ImageFormat.PNG:
                                sc.imageFormat = ".png";
                                break;
                        }
                        sc.isEnabledAlpha = isEnabledAlpha;
                        sc.resolution = resolution;
                        sc.frameCount = frameCount;
                        sc.fileName = fileName;
                        sc.filePath = filePath;
                        sc.rangeStart = rangeStart;
                        sc.rangeEnd = rangeEnd;
                        sc.TakeSequenceScreenShot();
                        progress = (Time.frameCount - rangeStart) / (float)(rangeEnd - rangeStart);
                    }
                    else if (Time.frameCount > rangeEnd)
                    {
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    progress = 0f;
                }
            }
        }


        void OnGUI()
        {
            {
                EditorGUILayout.Space();
                GUIStyle style = new GUIStyle("label");
                style.fontSize = 12;
                EditorGUILayout.LabelField(NAME_currectFrame, Time.frameCount.ToString(), style);
            }

            //设置渲染相机
            {
                cam = EditorGUILayout.ObjectField(NAME_selectedCamera, cam, typeof(Camera),true)as Camera;
                if (cam == null)
                    cam = Camera.main;
            }

            //设置图片格式和透明通道
            {
                theImageFormat = (ImageFormat)EditorGUILayout.EnumPopup(NAME_setFormat, theImageFormat);
                switch (theImageFormat)
                {
                    case ImageFormat.JPG:
                        isEnabledAlpha = EditorGUILayout.Toggle(NAME_isEnableAlpha,false);;
                        break;
                    case ImageFormat.PNG:
                        isEnabledAlpha = EditorGUILayout.Toggle(NAME_isEnableAlpha,isEnabledAlpha);
                        if (isEnabledAlpha)
                        {
                            cam.clearFlags = CameraClearFlags.SolidColor;
                            cam.backgroundColor = new Color(0, 0, 0, 0);
                        }
                        break;
                }

                EditorGUILayout.HelpBox(NAME_setFormatDescribution, MessageType.None);
            }

            //设置辨率
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(NAME_setResolution);
                resolution = EditorGUILayout.Vector2Field(GUIContent.none, resolution);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("默认尺寸"))
                    resolution = new Vector2(1280, 720);
                if (GUILayout.Button("获取当前视图尺寸"))
                    resolution = Handles.GetMainGameViewSize();
            }

            {
                GUIStyle style = new GUIStyle("textfield");
                fileName = EditorGUILayout.TextField(NAME_setFileName, fileName, style);
            }

            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(NAME_setSavePath);
                GUIStyle style01 = new GUIStyle("button");
                style01.fixedWidth = 60;
                style01.fixedHeight = 15;
                if (GUILayout.Button("...", style01))
                {
                    filePath = EditorUtility.OpenFolderPanel("", "", "");
                }
                    
                GUIStyle style02 = new GUIStyle("label");
                style02.fontStyle = FontStyle.Italic;
                EditorGUILayout.LabelField(filePath, style02);

                EditorGUILayout.EndHorizontal();
            }

            //导出当前截图
            {
                GUILayout.Space(20);
                if (GUILayout.Button(NAME_takeScreenShot, GUILayout.Height(20)))
                {
                    TakeScreenShot();
                }
                EditorGUILayout.HelpBox(NAME_takeScreenShotDescribution, MessageType.None);
            }


            GUILayout.Space(20);

            //导出序列图
            {
                isEnabled = EditorGUILayout.Toggle(NAME_setEnableSequence, isEnabled);
                if (isEnabled)
                {
                    //设置帧率
                    frameCount = EditorGUILayout.IntPopup(NAME_setFrame, frameCount, frameCountStrArry, frameCountIntArry);

                    //帧数范围
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUIStyle style = new GUIStyle("textfield");
                        EditorGUILayout.PrefixLabel(NAME_setFrameRange);
                        rangeStart = EditorGUILayout.IntField(rangeStart, style);
                        rangeEnd = EditorGUILayout.IntField(rangeEnd, style);
                        EditorGUILayout.EndHorizontal();
                    }

                    //开始导出
                    if (GUILayout.Button(NAME_startExport, GUILayout.Height(30)))
                    {
                        EditorApplication.isPlaying = true;
                    }
//                    EditorGUI.ProgressBar(new Rect(5, posY, windowRect.xMax - 10, 15), progress, (int)(progress * 100) + "%");

                    //如果处于运行状态则显示进度条
                    if (EditorApplication.isPlaying)
                    {
                        if( EditorUtility.DisplayCancelableProgressBar(
                            "正在导出序列图...",
                            (progress * 100) + "%",
                            (float)progress ) )
                        {
                            EditorApplication.isPlaying = false;
                        }
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar( ); 
                    }
                }
                EditorGUILayout.HelpBox(NAME_startExportDescribution, MessageType.Warning);
            }

            //打开文件夹
            {
                if (GUILayout.Button("打开导出文件夹"))
                {
                    Application.OpenURL("file://" + filePath);
                }
            }
                
            //制作名单
            {
                GUILayout.Space(20);
                GUIStyle style = new GUIStyle("ProjectBrowserBottomBarBg");
                style.fixedHeight = 2.5f;
                EditorGUILayout.LabelField(GUIContent.none, style);
                GUILayout.Space(-10);
                EditorGUILayout.LabelField("如有任何疑问请联系99U:225367 Update:2017/10/11", EditorStyles.centeredGreyMiniLabel);
            }
        }

        void TakeScreenShot()
        {
            int resWidthN = (int)resolution.x;
            int resHeightN = (int)resolution.y;
            RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
            cam.targetTexture = rt;

            TextureFormat _texFormat;
            if (isEnabledAlpha)
                _texFormat = TextureFormat.ARGB32;
            else
                _texFormat = TextureFormat.RGB24;
            
            Texture2D tex = new Texture2D(resWidthN, resHeightN, _texFormat, false);

            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null; 
            byte[] bytes;
            string suffix;
            switch (theImageFormat)
            {
                case ImageFormat.PNG:
                    bytes = tex.EncodeToPNG();
                    suffix = ".png";
                    break;
                case ImageFormat.JPG:
                    bytes = tex.EncodeToJPG();
                    suffix = ".jpg";
                    break;
                default:
                    bytes = tex.EncodeToPNG();
                    suffix = ".png";
                    break;
            }
            File.WriteAllBytes(filePath + "/" + fileName + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + suffix, bytes);
            Debug.Log(string.Format("截图成功"));
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }


}