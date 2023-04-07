using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseProxyEg
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    public class SimpleFileLogger
    {
        private readonly string logFile;

        public SimpleFileLogger(string logFile)
        {
            this.logFile = logFile;
        }

        public void Log(string message)
        {
            string logMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}{Environment.NewLine}";
            File.AppendAllText(logFile,  logMessage);
        }
    }

    public class AuthenticatedProxyServer
    {
        private readonly int port;
        private readonly string proxyUsername;
        private readonly string proxyPassword;

        public AuthenticatedProxyServer(int port, string proxyUsername, string proxyPassword)
        {
            this.port = port;
            this.proxyUsername = proxyUsername;
            this.proxyPassword = proxyPassword;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Proxy server started on port " + port);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected from " + ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                // Authenticate the client before forwarding the request.
              HandleRequest(client);
            }
        }

        public void HandleRequest(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (!AuthenticateRequest(request))
            {
                byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 401 Unauthorized\r\n\r\n");
                stream.Write(response, 0, response.Length);
                stream.Close();
                client.Close();
                Console.WriteLine("Authentication failed, connection closed");
                return;
            }

            // Forward the request to the destination server.
            string[] requestLines = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] firstLineParts = requestLines[0].Split(' ');
            string destinationHost = firstLineParts[1];
            TcpClient destinationClient = new TcpClient(destinationHost, 80);
            NetworkStream destinationStream = destinationClient.GetStream();

            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            destinationStream.Write(requestBytes, 0, requestBytes.Length);

            // Forward the response back to the client.
            buffer = new byte[1024];
            bytesRead = destinationStream.Read(buffer, 0, buffer.Length);
            while (bytesRead > 0)
            {
                stream.Write(buffer, 0, bytesRead);
                bytesRead = destinationStream.Read(buffer, 0, buffer.Length);
            }

            destinationStream.Close();
            destinationClient.Close();
            stream.Close();
            client.Close();
            try
            {
                Console.WriteLine($"client request. received from {client.Client.RemoteEndPoint.Serialize()}");
            }
            catch (Exception e) { }
        }

        public bool AuthenticateRequest(string request)
        {
            string[] requestLines = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in requestLines)
            {
                if (line.StartsWith("MilkyServerAuthorization"))
                {
                    string encodedCredentials = line.Substring("MilkyServerAuthorization".Length);
                    // string decodedCredentials = Encoding.ASCII.GetString(Convert.FromBase64String(encodedCredentials));
                    //string[] credentials = decodedCredentials.Split(':');
                    string[] credentials = encodedCredentials.Replace(":","").Split('/');
                    string username = credentials[0];
                    string password = credentials[1];
                    return (username == proxyUsername && password == proxyPassword);
                }
            }

            return true; // should return here true
        }
    }
    public class SimpleProxy
    {
        private int localPort;
        private string remoteHost;
        private int remotePort;
        private readonly SimpleFileLogger logger;
        private readonly string proxyPassword;

        public readonly string proxyUsername;

        public SimpleProxy(int localPort, string remoteHost, int remotePort, SimpleFileLogger logger)
        {
            this.localPort = localPort;
            this.remoteHost = remoteHost;
            this.remotePort = remotePort;
            this.logger = logger;
            proxyUsername = "saha";
            proxyPassword = "sarvani";
        }
        public bool AuthenticateRequest(string request)
        {
            string[] requestLines = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in requestLines)
            {
                if (line.StartsWith("MilkyServerAuthorization"))
                {
                    string encodedCredentials = line.Substring("MilkyServerAuthorization".Length);
                    // string decodedCredentials = Encoding.ASCII.GetString(Convert.FromBase64String(encodedCredentials));
                    //string[] credentials = decodedCredentials.Split(':');
                    string[] credentials = encodedCredentials.Replace(":", "").Split('/');
                    string username = credentials[0].Trim();
                    string password = credentials[1].Trim();
                    return (username == proxyUsername && password == proxyPassword);
                }
            }

            return false; // should return here true
        }
        public void Start()
        {
            logger.Log("Proxy started on port " + localPort);

            TcpListener listener = new TcpListener(IPAddress.Any, localPort);
            listener.Start();
            logger.Log("reversy proxy started..");
            while (true)
            {
                logger.Log("waiting for client request.");
                TcpClient client = listener.AcceptTcpClient();
                try {
                    logger.Log("Client connected from " + ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    logger.Log($"client request. received from {client.Client.RemoteEndPoint.Serialize()}"); 
                }
                catch(Exception e)
                {
                    logger.Log($"ERROR: while getting client ip {e.ToString()}" );
                }
                
                HandleClient(client);
                logger.Log("client request. finished");
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                
                //auh.HandleRequest(client);
                TcpClient remoteClient = new TcpClient(remoteHost, remotePort);
                Stream clientStream = client.GetStream();
                Stream remoteStream = remoteClient.GetStream();



               
                byte[] buffer = new byte[1024];
                int bytesRead = clientStream.Read(buffer, 0, buffer.Length);
                string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (!AuthenticateRequest(request))
                {
                    byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 401 Unauthorized\r\n\r\n");
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Close();
                    client.Close();
                    Console.WriteLine("Authentication failed, connection closed");
                    return;
                }

                // Create a buffer to hold data read from the client.

                while ((bytesRead) > 0)
                {
                    remoteStream.Write(buffer, 0, bytesRead);
                    byte[] responseBuffer = new byte[4096];
                    int responseBytesRead = remoteStream.Read(responseBuffer, 0, responseBuffer.Length);
                    clientStream.Write(responseBuffer, 0, responseBytesRead);
                }

                clientStream.Close();
                remoteStream.Close();
                client.Close();
                remoteClient.Close();
            }
            catch (Exception ex)
            {
                logger.Log("Error handling connection: " + ex.Message);
                client.Close();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SimpleFileLogger logger = new SimpleFileLogger("mylog.txt");
            Console.WriteLine("proxyport  applicationaddress(localhost/127.0.0.1/0.0.0.0 applicaitonPort");
            //SimpleProxy proxy = new SimpleProxy(8888, "www.google.com", 80);
            SimpleProxy proxy = new SimpleProxy(Convert.ToInt32( args[0].Trim()), args[1].Trim(), Convert.ToInt32(args[2].Trim()),logger);
            proxy.Start();
        }
    }

}
