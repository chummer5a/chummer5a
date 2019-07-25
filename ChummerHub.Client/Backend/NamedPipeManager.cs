using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    /// <summary>
    /// A very simple Named Pipe Server implementation that makes it 
    /// easy to pass string messages between two applications.
    /// </summary>
    public class NamedPipeManager 
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public string NamedPipeName = "Chummer";
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        private Thread Thread;

        public NamedPipeManager(string name)
        {
            NamedPipeName = name;
        }

        /// <summary>
        /// Starts a new Pipe server on a new thread
        /// </summary>
        public void StartServer()
        {
            StopServer();
            Thread = new Thread((pipeName) =>
            {
                
                _isRunning = true;
                while (true)
                {
                    string text;
                    try
                    {
                        using (var server = new NamedPipeServerStream(pipeName as string))
                        {
                            server.WaitForConnection();

                            using (StreamReader reader = new StreamReader(server))
                            {
                                text = reader.ReadToEnd();
                            }
                        }

                        if (text == EXIT_STRING)
                            break;

                        OnReceiveString(text);
                    }
                    catch(IOException e)
                    {
                        
                        Log.Warn(e);
                    }
                    

                    if (_isRunning == false)
                        break;
                }
            });
            Thread.Start(NamedPipeName);
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);


        /// <summary>
        /// Shuts down the pipe server
        /// </summary>
        public void StopServer()
        {
            _isRunning = false;
            Write(EXIT_STRING);
            //this.Thread.Sleep(30); // give time for thread shutdown
        }

        /// <summary>
        /// Write a client message to the pipe
        /// </summary>
        /// <param name="text"></param>
        /// <param name="connectTimeout"></param>
        public bool Write(string text, int connectTimeout = 300)
        {
            try
            {
                using (var client = new NamedPipeClientStream(NamedPipeName))
                {
                    try
                    {
                        client.Connect(connectTimeout);
                    }
                    catch (Exception e)
                    {
                        Log.Trace(e);
                        return false;
                    }

                    if (!client.IsConnected)
                        return false;

                    using (StreamWriter writer = new StreamWriter(client))
                    {
                        writer.Write(text);
                        writer.Flush();
                    }
                }
            }
            catch(Exception e)
            {
                Log.Warn(e);
                return false;
            }
            return true;
        }
    }

}
