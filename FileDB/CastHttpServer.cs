using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace FileDB
{
    public static class MimeTypeCreator
    {
        public static string? CreateMimeTypeFor(string filePath)
        {
            // See https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types

            var ext = Path.GetExtension(filePath);
            
            // Images
            if (string.Compare(ext, ".jpg", true) == 0 ||
                string.Compare(ext, ".jpeg", true) == 0)
            {
                return "image/jpeg";
            }
            if (string.Compare(ext, ".png", true) == 0)
            {
                return "image/png";
            }
            if (string.Compare(ext, ".gif", true) == 0)
            {
                return "image/gif";
            }
            if (string.Compare(ext, ".bmp", true) == 0)
            {
                return "image/bmp";
            }
            if (string.Compare(ext, ".svg", true) == 0)
            {
                return "image/svg+xml";
            }

            // Videos
            if (string.Compare(ext, ".mpg", true) == 0 ||
                string.Compare(ext, ".mpeg", true) == 0)
            {
                return "video/mpeg";
            }
            if (string.Compare(ext, ".mp4", true) == 0)
            {
                return "video/mp4";
            }
            if (string.Compare(ext, ".avi", true) == 0)
            {
                return "video/x-msvideo";
            }

            // Audio
            if (string.Compare(ext, ".mp3", true) == 0)
            {
                return "audio/mpeg";
            }
            if (string.Compare(ext, ".wav", true) == 0)
            {
                return "audio/wav";
            }

            // Documents
            if (string.Compare(ext, ".txt", true) == 0)
            {
                return "text/plain";
            }
            if (string.Compare(ext, ".html", true) == 0 ||
                string.Compare(ext, ".htm", true) == 0)
            {
                return "text/html";
            }
            if (string.Compare(ext, ".pdf", true) == 0)
            {
                return "application/pdf";
            }

            return null;
        }
    }

    public class FileCaster
    {
        private static CastHttpServer? server;
        private static Thread? serverThread;
        private static string? currentFilePath;

        public static bool Started => server != null && serverThread != null && serverThread.IsAlive;

        public static void StartServer(int port)
        {
            if (Started)
            {
                return;
            }

            server = new CastHttpServer(port);
            serverThread = new Thread(() => server.Run())
            {
                IsBackground = true,
            };
            serverThread.Start();
        }

        public static void LoadFile(string filePath)
        {
            if (Started &&
                currentFilePath != filePath)
            {
                var mimeType = MimeTypeCreator.CreateMimeTypeFor(filePath);
                if (mimeType != null)
                {
                    try
                    {
                        var fileContent = File.ReadAllBytes(filePath);
                        currentFilePath = filePath;
                        server!.NextFile = new FileToCast(fileContent, mimeType);
                    }
                    catch (IOException)
                    {
                        // Ignore
                    }
                }
            }
        }
    }

    public record FileToCast(byte[] Content, string ContentType);

    public class CastHttpServer
    {
        private HttpListener httpListener;

        public FileToCast NextFile
        {
            get => nextFile;
            set
            {
                lock (fileRequestLock)
                {
                    nextFile = value;
                }
            }
        }
        private FileToCast nextFile = new FileToCast(Encoding.UTF8.GetBytes("No file loaded"), "text/plain");

        private readonly object fileRequestLock = new();

        public CastHttpServer(int port)
        {
            var prefix = $"http://+:{port}/";

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
        }

        public void Run()
        {
            httpListener.Start();

            bool run = true;
            while (run)
            {
                var context = httpListener.GetContext();
                var result = false;

                switch (context.Request.Url!.Segments.Last())
                {
                    case "stop":
                        run = false;
                        break;

                    default:
                        result = HandleFileRequest(context);
                        break;
                }

                if (!result && run)
                {
                    WriteErrorResponse(context, "Not found");
                }
            }

            httpListener.Stop();
        }

        private bool HandleFileRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                lock (fileRequestLock)
                {
                    if (NextFile != null)
                    {
                        WriteBinaryResponse(context, NextFile.Content, NextFile.ContentType);
                    }
                    else
                    {
                        WriteErrorResponse(context, "No file prepared");
                    }
                }
                return true;
            }
            return false;
        }

        private static void WriteBinaryResponse(HttpListenerContext context, byte[] data, string contentType)
        {
            WriteResponse(context.Response, 200, data, contentType);
        }

        private static void WriteErrorResponse(HttpListenerContext context, string message)
        {
            WriteTextResponse(context, message, 404);
        }

        private static void WriteTextResponse(HttpListenerContext context, string text, int statusCode = 200)
        {
            WriteResponse(context.Response, statusCode, Encoding.UTF8.GetBytes(text), "text/plain");
        }

        private static void WriteResponse(HttpListenerResponse response, int statusCode, byte[] content, string contentType)
        {
            response.ContentLength64 = content.Length;
            response.ContentType = contentType;
            response.StatusCode = statusCode;

            var output = response.OutputStream;
            output.Write(content, 0, content.Length);
            output.Close();
        }
    }
}
