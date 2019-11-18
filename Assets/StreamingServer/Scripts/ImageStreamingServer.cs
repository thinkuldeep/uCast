using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace StreamingServer
{
    public class ImageStreamingServer : IDisposable
    {
        private List<Socket> _clients;
        private Thread _thread;
        private IEnumerable<MemoryStream> imagesSource { get; }

        public IEnumerable<Socket> Clients => _clients;
        
        private byte[] _frameBufferBytes = new byte[0];
        public bool IsRunning => (_thread != null && _thread.IsAlive);

        public ImageStreamingServer(int port)
        {
            _clients = new List<Socket>();
            _thread = null;
            this.imagesSource = ImageStream();
            Start(port);
        }

        private void Start(int port)
        {
            lock (this)
            {
                Debug.Log($"ImageServer::Starting Sever {port}");
                _thread = new Thread(new ParameterizedThreadStart(ServerThread));
                _thread.IsBackground = true;
                _thread.Start(port);
                Debug.Log($"ImageServer::Started Sever ");
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                try
                {
                    _thread.Join();
                    _thread.Abort();
                }
                finally
                {
                    lock (_clients)
                    {
                        foreach (var s in _clients)
                        {
                            try
                            {
                                s.Close();
                            }
                            catch (Exception exception)
                            {
                                Debug.LogError(exception);
                            }
                        }

                        _clients.Clear();
                    }

                    _thread = null;
                }
            }
        }

        private void ServerThread(object state)
        {
            try
            {
                Socket streamingServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                streamingServer.Bind(new IPEndPoint(IPAddress.Any, (int) state));
                streamingServer.Listen(10);

                Debug.Log($"ImageServer::Server started on port {state}.");

                foreach (Socket client in streamingServer.AcceptIncomingConnections())
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }

            Stop();
        }

        private void ClientThread(object client)
        {
            Socket socket = (Socket) client;

            Debug.Log($"ImageServer::New client from {socket.RemoteEndPoint.ToString()}");

            lock (_clients)
                _clients.Add(socket);

            try
            {
                using (MJpegWriter wr = new MJpegWriter(new NetworkStream(socket, true)))
                {
                    wr.WriteHeader();
                    foreach (var imgStream in imagesSource)
                    {
                        wr.Write(imgStream);
                    }
                }
            }
            catch(Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                lock (_clients)
                    _clients.Remove(socket);
            }
        }

        public void WriteToStream(byte[] frameBuffer)
        {
            _frameBufferBytes = frameBuffer;
        }
        
        public void WriteToStream(Texture2D texture2D)
        {
            _frameBufferBytes = texture2D.EncodeToJPG(70);
        }
        
        private IEnumerable<MemoryStream> ImageStream()
        {
            while (true)
            {
                if(_frameBufferBytes?.Length>0)
                yield return new MemoryStream(_frameBufferBytes);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}