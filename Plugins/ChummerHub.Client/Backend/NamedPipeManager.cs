using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ChummerHub.Client.Backend
{
    /// <summary>
    /// A very simple Named Pipe Server implementation that makes it
    /// easy to pass string messages between two applications.
    /// </summary>
    public class NamedPipeManager : IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public string NamedPipeName { get; }
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private CancellationTokenSource _objCancellationTokenSource;
        private Task _objRunningTask;

        public NamedPipeManager(string name = "Chummer")
        {
            NamedPipeName = name;
        }

        /// <summary>
        /// Starts a new Pipe server on a new thread
        /// </summary>
        public async Task StartServer()
        {
            StopServer();
            try
            {
                if (_objRunningTask?.IsCompleted == false) // Wait for existing thread to shut down
                    await _objRunningTask;
            }
            catch (TaskCanceledException)
            {
                // Swallow and continue
            }
            _objCancellationTokenSource = new CancellationTokenSource();
            _objRunningTask = Task.Run(RunChummerFilePipeThread, _objCancellationTokenSource.Token);
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
            _objCancellationTokenSource?.Cancel(false);
            Log.Trace("Sending Exit to PipeServer...");
            Write(EXIT_STRING);
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
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", NamedPipeName, PipeDirection.InOut))
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

        private async Task RunChummerFilePipeThread()
        {
            if (!(NamedPipeName is string pipeNameString))
                throw new ArgumentNullException(nameof(NamedPipeName));
            PipeSecurity ps = new PipeSecurity();
            System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            //PipeAccessRule psRule = new PipeAccessRule(@"Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            while (!_objCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    string text;
                    using (NamedPipeServerStream server = new NamedPipeServerStream(pipeNameString,
                        PipeDirection.InOut, 1,
                        PipeTransmissionMode.Message, PipeOptions.None,
                        4028, 4028, ps))
                    {
                        await server.WaitForConnectionAsync(_objCancellationTokenSource.Token);

                        if (_objCancellationTokenSource.IsCancellationRequested)
                            throw new OperationCanceledException();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            text = await reader.ReadToEndAsync();
                        }
                    }

                    if (text == EXIT_STRING)
                        return;

                    if (_objCancellationTokenSource.IsCancellationRequested)
                        throw new OperationCanceledException();

                    OnReceiveString(text);
                }
                catch (IOException e)
                {
                    Log.Warn(e);
                    await Chummer.Utils.SafeSleepAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    await Chummer.Utils.SafeSleepAsync();
                }
            }
            throw new OperationCanceledException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _objCancellationTokenSource?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
