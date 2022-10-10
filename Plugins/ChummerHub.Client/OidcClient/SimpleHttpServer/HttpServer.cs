// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using NLog;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleHttpServer
{

    public class HttpServer
    {
        #region Fields

        private readonly int Port;
        private TcpListener Listener;
        private readonly HttpProcessor Processor;
        private readonly bool IsActive = true;

        #endregion

        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        #region Public Methods
        public HttpServer(int port, List<Route> routes)
        {
            this.Port = port;
            this.Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                this.Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            try
            {
                if (Listener == null)
                {
                    this.Listener = new TcpListener(IPAddress.Any, this.Port);
                    this.Listener.Start();
                }
                while (this.IsActive)
                {
                    TcpClient s = this.Listener.AcceptTcpClient();
                    Thread thread = new Thread(() =>
                    {
                        this.Processor.HandleClient(s);
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
            
        }

        #endregion

    }
}



