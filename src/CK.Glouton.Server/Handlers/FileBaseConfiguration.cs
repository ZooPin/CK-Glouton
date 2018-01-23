﻿using CK.Glouton.Model.Server.Handlers;

namespace CK.Glouton.Server.Handlers
{
    /// <summary>
    /// Configuration base object for files.
    /// </summary>
    public abstract class FileConfigurationBase : IGloutonHandlerConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="FileConfigurationBase"/>.
        /// </summary>
        protected FileConfigurationBase()
        {
            MaxCountPerFile = 20000;
        }

        /// <summary>
        /// Gets or sets the path of the file. When not rooted (see <see cref="System.IO.Path.IsPathRooted"/>),
        /// it is a sub path in <see cref="LogFile.RootLogPath"/>.
        /// It defaults to null: it must be specified.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the maximal count of entries per file.
        /// Defaults to 20000.
        /// </summary>
        public int MaxCountPerFile { get; set; }

        /// <summary>
        /// Clones this configuration.
        /// </summary>
        public abstract IGloutonHandlerConfiguration Clone();
    }
}
