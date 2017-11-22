﻿using CK.Core;
using NUnit.Framework;
using System.Threading;

namespace CK.Glouton.Tests
{
    [TestFixture]
    public class LuceneTests
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.Setup();
        }

        [Test]
        public void log_can_be_indexed()
        {
            using( var server = TestHelper.DefaultGloutonServer() )
            {
                server.Open();
                var g = GrandOutputHelper.GetNewGrandOutputClient();

                var m = new ActivityMonitor( false ) { MinimalFilter = LogFilter.Debug };
                g.EnsureGrandOutputClient( m );

                m.Info( "Hello world" );
                m.Error( "CriticalError" );
                Thread.Sleep(500);
                g.Dispose();
            }
        }
    }
}
