using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace FileDB
{
    public class FileCaster
    {
        private static CastHttpServer server;
        private static Thread serverThread;
        private static string currentFilePath;

        public static bool IsRunning()
        {
            return server != null && serverThread.IsAlive;
        }

        public static void RunServer(int port)
        {
            if (IsRunning())
            {
                return;
            }

            server = new CastHttpServer(port);
            serverThread = new Thread(Start)
            {
                IsBackground = true,
            };
            serverThread.Start();
        }

        private static void Start()
        {
            server.Run();
        }

        public static void CastFile(string filePath)
        {
            if (IsRunning() &&
                currentFilePath != filePath)
            {
                // TODO: handle more files and MIME types
                if (filePath.EndsWith(".jpg"))
                {
                    try
                    {
                        var fileContent = File.ReadAllBytes(filePath);
                        currentFilePath = filePath;
                        server.NextFile = new FileToCast(fileContent, "image/jpg");
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
            // TODO: access denied when specifying * instead of localhost
            var prefix = $"http://localhost:{port}/filedb/";

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

                switch (context.Request.Url.Segments.Last())
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
