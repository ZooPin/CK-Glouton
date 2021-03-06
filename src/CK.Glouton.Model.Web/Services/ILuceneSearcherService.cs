﻿using CK.Glouton.Model.Lucene.Logs;
using System;
using System.Collections.Generic;

namespace CK.Glouton.Model.Web.Services
{
    public interface ILuceneSearcherService
    {
        List<ILogViewModel> GetAll( params string[] appNames );
        List<string> GetAppNameList();
        List<ILogViewModel> GetLogWithFilters
        (
            string monitorId,
            DateTime start,
            DateTime end,
            string[] fields,
            string[] logLevel,
            string query,
            string[] appNames,
            int groupDepth = 0,
            int count = -1
        );
        List<ILogViewModel>[] LogsBeforeAndAfter
        (
            string monitorId,
            DateTime dateTime,
            string[] fields,
            string[] logLevel,
            string query,
            string[] appNames,
            int groupDepth,
            int count
        );
        List<string> GetMonitorIdList();
        List<ILogViewModel> Search( string query, params string[] appNames );
    }
}
