﻿using CK.Glouton.Model.Lucene;
using Lucene.Net.Documents;

namespace CK.Glouton.Model.Logs
{
    public class LineViewModel : ILogViewModel
    {
        public ELogType LogType => ELogType.Line;
        public string LogTime { get; set; }
        public string Text { get; set; }
        public string Tags { get; set; }
        public string SourceFileName { get; set; }
        public string LineNumber { get; set; }
        public IExceptionViewModel Exception { get; set; }
        public string LogLevel { get; set; }
        public string MonitorId { get; set; }
        public string GroupDepth { get; set; }
        public string PreviousEntryType { get; set; }
        public string PreviousLogTime { get; set; }
        public string AppId { get; set; }

        public static LineViewModel Get ( ILuceneSearcher luceneSearcher, Document document )
        {
            return new LineViewModel
            {
                MonitorId = document.Get( LogField.MONITOR_ID ),
                GroupDepth = document.Get( LogField.GROUP_DEPTH ),
                PreviousEntryType = document.Get( LogField.PREVIOUS_ENTRY_TYPE ),
                PreviousLogTime = document.Get( LogField.PREVIOUS_LOG_TIME ),
                LogLevel = document.Get( LogField.LOG_LEVEL ),
                Text = document.Get( LogField.TEXT ),
                Tags = document.Get( LogField.TAGS ),
                SourceFileName = document.Get( LogField.SOURCE_FILE_NAME ),
                LineNumber = document.Get( LogField.LINE_NUMBER ),
                LogTime = DateTools.StringToDate( document.Get( LogField.LOG_TIME ) ).ToString( "dd/MM/yyyy HH:mm:ss.fff" ),
                Exception = ExceptionViewModel.Get( luceneSearcher, document ),
                AppId = document.Get( LogField.APP_NAME )
            };
        }
    }
}
