using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using StreamingServer;

namespace StreamingServer
{
    public class StreamingExample : MonoBehaviour
    {
        private Texture2D _texture ;
        private RenderTexture _bufferTexture;
        private Color32[] _pixels;
        private ImageStreamingServer _server;
        
        [SerializeField] private int port = 8080;
        
        [SerializeField]
        private int bufferWidth = 950;

        [SerializeField]
        private int bufferHeight = 500;
        [SerializeField]
        private Camera StreamingCamera;
        private Texture2D _jpgTexture;
       
        private void Start()
        {
            _server = new ImageStreamingServer(port);
            _bufferTexture = new RenderTexture(bufferWidth, bufferHeight, 24, RenderTextureFormat.ARGB32);
            _jpgTexture = new Texture2D(bufferWidth, bufferHeight, TextureFormat.ARGB32, false);
            var mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            StreamingCamera.transform.position = mainCamera.transform.position;
            StreamingCamera.targetTexture = _bufferTexture;
        }
        
        void Update()
        {
            
            transform.Rotate(Vector3.down, 100f* Time.deltaTime);
            RenderTexture.active = _bufferTexture;
            _jpgTexture.ReadPixels(new Rect(0, 0, bufferWidth, bufferHeight), 0, 0);
            _jpgTexture.Apply();
            
            _server.WriteToStream(_jpgTexture);
        }
        
    }
}