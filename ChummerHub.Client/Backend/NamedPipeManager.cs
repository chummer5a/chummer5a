using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChummerHub.Client.Backend
{
    /// <summary>
    /// A very simple Named Pipe Server implementation that makes it
    /// easy to pass string messages between two applications.
    /// </summary>
    public class NamedPipeManager
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public string NamedPipeName { get; }
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        private Thread Thread;

        public NamedPipeManager(string name = "Chummer")
        {
            NamedPipeName = name;
        }

        /// <summary>
        /// Starts a new Pipe server on a new thread
        /// </summary>
        public void StartServer()
        {
            StopServer();
            Thread = new Thread(pipeName =>
            {
                if (!(pipeName is string pipeNameString))
                    throw new ArgumentNullException(nameof(pipeName));
                PipeSecurity ps = new PipeSecurity();
                System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
                PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                //PipeAccessRule psRule = new PipeAccessRule(@"Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                ps.AddAccessRule(par);
                _isRunning = true;
                while (_isRunning)
                {
                    try
                    {
                        string text;
                        using (var server = new NamedPipeServerStream(pipeNameString,
                            PipeDirection.InOut, 1,
                            PipeTransmissionMode.Message, PipeOptions.None,
                            4028, 4028, ps))
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
                    catch (IOException e)
                    {
                        Log.Warn(e);
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Thread.Sleep(50);
                    }
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
            Log.Trace("Sending Exit to PipeServer...");
            Write(EXIT_STRING);
            Thread.Sleep(60); // give time for thread shutdown
        }

        /// <summary>
        /// Write a client message to the pipe
        /// </summary>
        /// <param name="text"></param>
        /// <param name="connectTimeout"></param>
        public bool Write(string text, int connectTimeout = 3000)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", NamedPipeName, PipeDirection.InOut))
                {
                    try
                    {
                        Log.Debug("Connection with pipe " + NamedPipeName + "...");
                        client.Connect(connectTimeout);
                        Log.Debug("Pipe connection established.");
                    }
                    catch (TimeoutException e)
                    {
                        if (text != EXIT_STRING)
                        {
                            Log.Warn(e);
                            return false;
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        return false;
                    }

                    if (!client.IsConnected)
                        return false;

                    using (StreamWriter writer = new StreamWriter(client))
                    {
                        writer.Write(text);
                        writer.Flush();
                        Log.Debug("Written to pipe " + NamedPipeName + ": " + text);
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
