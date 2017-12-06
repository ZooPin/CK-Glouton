﻿using CK.Glouton.Model.Logs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CK.Glouton.Web.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the controller for log related requests.
    /// </summary>
    [Route( "api/log" )]
    public class LogController : Controller
    {
        private readonly ILuceneSearcherService _luceneSearcherService;

        public LogController( ILuceneSearcherService luceneSearcherService )
        {
            _luceneSearcherService = luceneSearcherService;
        }

        /// <summary>
        /// Returns <paramref name="max"/> logs. <paramref name="max"/> is 500 by default.
        /// Match the following: <code>api/log?max=[max] -- GET</code>.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="max">The number of logs to return.</param>
        /// <returns></returns>
        [HttpGet( "all/{appName}" )]
        public List<ILogViewModel> GetAll( [FromRoute] string appName, [FromQuery] int max = 0 )
        {
            return _luceneSearcherService.GetAll( appName, max == 0 ? 10 : max ) ?? new List<ILogViewModel>();
        }

        /// <summary>
        /// Returns logs matching <paramref name="query"/>. Lucene is doing the processing.
        /// Match the following: <code>api/log/search?query=[query] -- GET</code>.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="query">The query which will be processed by lucene.</param>
        /// <returns></returns>
        [HttpGet( "search/{appName}" )]
        public List<ILogViewModel> Search( [FromRoute] string appName, [FromQuery] string query = "" )
        {
            return string.IsNullOrEmpty( query ) ? GetAll( appName ) : _luceneSearcherService.Search( appName, query );
        }

        /// <summary>
        /// Returns logs matching given parameters. When not specified, a parameter will be treated as <value>"All"</value>
        /// Match the following: <code>api/log/search/date?from=[date]&to=[date]&monitor=[monitor]&fields=[fields]&keywords=[keywords] -- GET</code>.
        /// </summary>
        /// <param name="monitorId">The monitorId to get logs from. By default: <code>"All"</code>.</param>
        /// <param name="appName">The application name to get logs from. By default: <code>"All"</code>.</param>
        /// <param name="from">The lower date for the time span range..</param>
        /// <param name="to">The superior date for the time span range.</param>
        /// <param name="fields">Fields to look for. By default: <code>{ "Tags", "FileName", "Text" }</code>.</param>
        /// <param name="keyword">Which keywords to consider. By default: <code>"*"</code>.</param>
        /// <param name="logLevel">Log levels to consider during the search. By default: <code>{ "Debug", "Trace", "Info", "Warn", "Error", "Fatal" }</code>.</param>
        /// <returns></returns>
        [HttpGet( "filter" )]
        public List<ILogViewModel> Filter
        (
            [FromQuery] string monitorId, [FromQuery] string appName,
            [FromQuery] DateTime from, [FromQuery] DateTime to,
            [FromQuery] string[] fields, [FromQuery] string keyword,
            [FromQuery] string[] logLevel
        )
        {
            if( monitorId == null || monitorId == "*" )
                monitorId = "All";
            if( appName == null || appName == "*" )
                appName = "All";
            if( fields.Length == 0 || fields[ 0 ] == "*" )
                fields = new[] { "Tags", "FileName", "Text" };
            if( logLevel.Length == 0 || logLevel[ 0 ] == "*" )
                logLevel = new[] { "Debug", "Trace", "Info", "Warn", "Error", "Fatal" };
            if( keyword == null )
                keyword = "*";

            return _luceneSearcherService.GetLogWithFilters( monitorId, appName, from, to, fields, logLevel, keyword );
        }

        /// <summary>
        /// Returns the list of all monitors id.
        /// Match the following: <code>api/log/monitorId -- GET</code>.
        /// </summary>
        /// <returns></returns>
        [HttpGet( "monitorId" )]
        public ISet<string> GetAllMonitorId()
        {
            return _luceneSearcherService.GetMonitorIdList() ?? new HashSet<string>();
        }

        /// <summary>
        /// Returns the list of all application name.
        /// Match the following: <code>api/log/appName -- GET</code>.
        /// </summary>
        /// <returns></returns>
        [HttpGet( "appName" )]
        public ISet<string> GetAllAppName()
        {
            return _luceneSearcherService.GetAppNameList() ?? new HashSet<string>();
        }
    }
}
