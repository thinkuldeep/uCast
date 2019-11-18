using System;
using System.IO;
using System.Text;

namespace StreamingServer
{
    public class MJpegWriter : IDisposable
    {
        private const string CarriageReturnLineFeed = "\r\n";
        private string _boundaryString;

        private string Boundary { get; set; }
        private Stream Stream { get; set; }

        public MJpegWriter(Stream stream, string boundary = "--boundary")
        {
            Stream = stream;
            Boundary = boundary;
        }

        public void WriteHeader()
        {
            var multipartXMixedReplaceBoundary =
                "HTTP/1.1 200 OK" + CarriageReturnLineFeed + "Content-Type: multipart/x-mixed-replace; boundary=";
            Write(multipartXMixedReplaceBoundary + Boundary + CarriageReturnLineFeed);

            Stream.Flush();
        }

        public void Write(MemoryStream imageStream)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine(Boundary);
            sb.AppendLine("Content-Type: image/jpeg");
            sb.AppendLine("Content-Length: " + imageStream.Length.ToString());
            sb.AppendLine();

            Write(sb.ToString());
            imageStream.WriteTo(Stream);
            Write(CarriageReturnLineFeed);

            Stream.Flush();
        }

        private void Write(string text)
        {
            byte[] data = BytesOf(text);
            Stream.Write(data, 0, data.Length);
        }

        private static byte[] BytesOf(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Stream?.Dispose();
            }
            finally
            {
                Stream = null;
            }
        }

        #endregion
    }
}