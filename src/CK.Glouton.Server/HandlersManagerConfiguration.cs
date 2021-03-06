﻿using CK.Glouton.Model.Server;
using CK.Glouton.Model.Server.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK.Glouton.Server
{
    public sealed class HandlersManagerConfiguration : IHandlersManagerConfiguration
    {
        internal bool InternalClone;

        public List<IGloutonHandlerConfiguration> GloutonHandlers { get; } = new List<IGloutonHandlerConfiguration>();
        public TimeSpan TimerDuration { get; set; } = TimeSpan.FromMilliseconds( 500 );

        public IHandlersManagerConfiguration AddGloutonHandler( IGloutonHandlerConfiguration configuration )
        {
            GloutonHandlers.Add( configuration );
            return this;
        }

        public IHandlersManagerConfiguration Clone()
        {
            var configuration = new HandlersManagerConfiguration();
            configuration.GloutonHandlers.AddRange( GloutonHandlers.Select( h => h.Clone() ) );
            return configuration;
        }
    }
}