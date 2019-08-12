using UnityEngine;
using System.Collections;
using System.IO;

namespace taecg.tools.ImageExporter
{
    public class ImageExporterController : MonoBehaviour
    {

        [HideInInspector]public Camera cam;
        [HideInInspector]public string imageFormat;
        [HideInInspector]public bool isEnabledAlpha;
        [HideInInspector]public Vector2 resolution;
        [HideInInspector]public int frameCount;
        [HideInInspector]public string fileName;
        [HideInInspector]public string filePath;
        [HideInInspector]public int rangeStart;
        [HideInInspector]public int rangeEnd;
        [HideInInspector] public int currectFrame;

        void Awake()
        {
            if (cam = null)
                cam = Camera.main;
        }

        // Use this for initialization
        void Start()
        {
            Time.captureFramerate = frameCount;
        }
	
        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 生成序列图
        /// </summary>
        public void TakeSequenceScreenShot()
        {
            StartCoroutine(WaitTakeSequenceScreenShot());
        }

        IEnumerator WaitTakeSequenceScreenShot()
        {
            int resWidthN = (int)resolution.x;
            int resHeightN = (int)resolution.y;

            RenderTexture rt = new RenderTexture(resWidthN, resHeightN,24);
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
            tex.Apply();

            //清空rendertexture
            cam.targetTexture = null;
            RenderTexture.active = null; 
            if (!isEnabledAlpha)
                GameObject.Destroy(rt);

            byte[] bytes;
            switch(imageFormat)
            {
                case ".png":
                    bytes = tex.EncodeToPNG();
                    break;
                case ".jpg":
                    bytes = tex.EncodeToJPG();
                    break;
                default:
                    bytes = tex.EncodeToPNG();
                    break;
            }
            File.WriteAllBytes(filePath + "/" + fileName + "_" + Time.frameCount +imageFormat, bytes);

            yield return new WaitForEndOfFrame();
        }
    }
}