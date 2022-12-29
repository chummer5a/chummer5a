/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
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
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                Log.Trace("Sending Exit to PipeServer...");
                Write(EXIT_STRING);
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            try
            {
                if (_objRunningTask?.IsCompleted == false) // Wait for existing thread to shut down
                    await _objRunningTask;
            }
            catch (TaskCanceledException)
            {
                // Swallow and continue
            }
            catch
            {
                Interlocked.CompareExchange(ref _objCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }

            CancellationToken objToken = objNewSource.Token;
            _objRunningTask = Task.Run(() => RunChummerFilePipeThread(objToken), objToken);
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);

        /// <summary>
        /// Shuts down the pipe server
        /// </summary>
        public void StopServer(bool blnSendExitToPipeServer)
        {
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                if (blnSendExitToPipeServer)
                {
                    Log.Trace("Sending Exit to PipeServer...");
                    Write(EXIT_STRING);
                }
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
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

        private async Task RunChummerFilePipeThread(CancellationToken token = default)
        {
            if (!(NamedPipeName is string pipeNameString))
                throw new ArgumentNullException(nameof(NamedPipeName));
            PipeSecurity ps = new PipeSecurity();
            System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            //PipeAccessRule psRule = new PipeAccessRule(@"Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    string text;
                    using (NamedPipeServerStream server = new NamedPipeServerStream(pipeNameString,
                        PipeDirection.InOut, 1,
                        PipeTransmissionMode.Message, PipeOptions.None,
                        4028, 4028, ps))
                    {
                        await server.WaitForConnectionAsync(token);

                        token.ThrowIfCancellationRequested();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            text = await reader.ReadToEndAsync();
                        }
                    }

                    if (text == EXIT_STRING)
                        return;

                    token.ThrowIfCancellationRequested();

                    OnReceiveString(text);
                }
                catch (IOException e)
                {
                    Log.Warn(e);
                    await Chummer.Utils.SafeSleepAsync(token);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    await Chummer.Utils.SafeSleepAsync(token);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopServer(false);
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
