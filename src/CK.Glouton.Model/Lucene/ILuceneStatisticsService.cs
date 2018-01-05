﻿using System.Collections.Generic;

namespace CK.Glouton.Model.Lucene
{
    public interface ILuceneStatisticsService
    {
        int AllExceptionCount { get; }
        int AppNameCount { get; }
        IEnumerable<string> GetAppNames { get; }

        int AllLogCount();
        Dictionary<string, int> GetExceptionByAppName();
        Dictionary<string, int> GetLogByAppName();
    }
}