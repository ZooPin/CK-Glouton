﻿using CK.ControlChannel.Abstractions;
using CK.Core;
using CK.Glouton.Server;
using System.IO;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;

namespace CK.Glouton.Tests
{
    public class TestHelper
    {
        /// <summary>
        /// Represents 127.0.0.1.
        /// </summary>
        internal static string DefaultHost { get; } = "127.0.0.1";

        /// <summary>
        /// Represents 43712.
        /// </summary>
        internal static ushort DefaultPort { get; } = 43712;

        /// <summary>
        /// Represents 125 ms.
        /// </summary>
        internal static int DefaultSleepTime => 125;

        /// <summary>
        /// We want to initialize 
        /// </summary>
        private static bool _environmentInitialized;
        private static IAuthorizationHandler _defaultAuthHandler;

        /// <summary>
        /// Returns a new <see cref="IAuthorizationHandler"/> which is always true.
        /// </summary>
        internal static IAuthorizationHandler DefaultAuthHandler => _defaultAuthHandler ?? ( _defaultAuthHandler = new TestAuthHandler( s => true ) );

        /// <summary>
        /// Returns a new mock server.
        /// Host will be <see cref="DefaultAuthHandler"/> and Port will be <see cref="DefaultPort"/>.
        /// <para>Received logs will be stored in <see cref="GloutonServerMock.ListLog"/>.
        /// This server won't have handlers support.
        /// To get a server with handlers, please use <see cref="DefaultGloutonServer"/> instead.</para>
        /// </summary>
        /// <param name="authorizationHandler">The authorization handler. If null, <see cref="DefaultAuthHandler"/> will be used.</param>
        /// <param name="userCertificateValidationCallback">The user certification callback. Can be null.</param>
        /// <returns></returns>
        internal static GloutonServerMock DefaultMockServer
        (
            IAuthorizationHandler authorizationHandler = null,
            RemoteCertificateValidationCallback userCertificateValidationCallback = null
        )
        {
            return new GloutonServerMock
            (
                DefaultHost,
                DefaultPort,
                authorizationHandler ?? DefaultAuthHandler,
                null,
                userCertificateValidationCallback
            );
        }

        /// <summary>
        /// Returns a new Glouton Server.
        /// Host will be <see cref="DefaultHost"/> and port will be <see cref="DefaultPort"/>.
        /// </summary>
        /// <param name="activityMonitor"> The activity monitor used by server and its handler. If null, it will instantiate a new one.</param>
        /// <param name="authorizationHandler">The authorization handler. If null, <see cref="DefaultAuthHandler"/> will be used.</param>
        /// <param name="userCertificateValidationCallback">The user certification callback. Can be null.</param>
        /// <returns></returns>
        internal static GloutonServer DefaultGloutonServer
        (
            IActivityMonitor activityMonitor = null,
            IAuthorizationHandler authorizationHandler = null,
            RemoteCertificateValidationCallback userCertificateValidationCallback = null
        )
        {
            return new GloutonServer(
                DefaultHost,
                DefaultPort,
                activityMonitor ?? new ActivityMonitor(),
                authorizationHandler ?? DefaultAuthHandler,
                null,
                userCertificateValidationCallback
            );
        }

        /// <summary>
        /// Set console encoding to <see cref="Encoding.UTF8"/>, the root log path and register a new <see cref="ActivityMonitorConsoleClient"/>.
        /// </summary>
        internal static void Setup()
        {
            if( _environmentInitialized )
                return;

            _environmentInitialized = true;

            if( !System.Console.IsOutputRedirected )
                System.Console.OutputEncoding = Encoding.UTF8;

            LogFile.RootLogPath = GetTestLogDirectory();

            ActivityMonitor.DefaultFilter = LogFilter.Debug;
            ActivityMonitor.AutoConfiguration += monitor => monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
        }

        /// <summary>
        /// Get the log folder inside the current test directory.
        /// If none currently exits, one will be created.
        /// </summary>
        /// <returns></returns>
        internal static string GetTestLogDirectory()
        {
            var logPath = Path.Combine( Directory.GetParent( ProjectPath ).FullName, "Logs" );
            if( !Directory.Exists( logPath ) )
                Directory.CreateDirectory( logPath );
            return logPath;
        }

        private static string _projectPath;
        private static string ProjectPath => _projectPath ?? ( _projectPath = GetProjectPath() );
        private static string GetProjectPath( [CallerFilePath]string path = null ) => Path.GetDirectoryName( path );
    }
}