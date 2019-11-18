using UnityEngine;
using System;
using System.Runtime.InteropServices;
using StreamingServer;

public class CastExample : MonoBehaviour
{
    [SerializeField]
    uDesktopDuplication.Texture uddTexture;

    [SerializeField] private int port = 8080;
    
    Texture2D texture_;
    Color32[] pixels_;
    GCHandle handle_;
    IntPtr ptr_ = IntPtr.Zero;
    private ImageStreamingServer _server;
    
    void Start()
    {
        if (!uddTexture) return;
        _server = new ImageStreamingServer(port);
        uddTexture.monitor.useGetPixels = true;
        UpdateTexture();
    }

    void OnDestroy()
    {
        if (ptr_ != IntPtr.Zero) {
            handle_.Free();
        }
    }

    void Update()
    {
        if (!uddTexture) return;

        if (uddTexture.monitor.width != texture_.width || 
            uddTexture.monitor.height != texture_.height) {
            UpdateTexture();
        }

        CopyTexture();
    }

    void UpdateTexture()
    {
        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;

        // TextureFormat.BGRA32 should be set but it causes an error now.
        texture_ = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture_.filterMode = FilterMode.Bilinear;
        pixels_ = texture_.GetPixels32();
        handle_ = GCHandle.Alloc(pixels_, GCHandleType.Pinned);
        ptr_ = handle_.AddrOfPinnedObject();
    }

    void CopyTexture()
    {
        var buffer = uddTexture.monitor.buffer;
        if (buffer == IntPtr.Zero) return;

        var width = uddTexture.monitor.width;
        var height = uddTexture.monitor.height;
       // memcpy(ptr_, buffer, width * height * sizeof(Byte) * 4);
       if (uddTexture.monitor.GetPixels(pixels_, 0, 0, width, height))
       {
           texture_.SetPixels32(pixels_);
           texture_.Apply();
           _server.WriteToStream(texture_);
       }
    }
}
