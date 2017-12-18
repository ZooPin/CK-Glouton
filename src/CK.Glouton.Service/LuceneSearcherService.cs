﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CK.Glouton.Lucene;
using CK.Glouton.Model.Logs;
using Microsoft.Extensions.Options;

namespace CK.Glouton.Service
{
    public class LuceneSearcherService : ILuceneSearcherService
    {
        private readonly LuceneConfiguration _configuration;
        private readonly LuceneSearcherManager _searcherManager;

        public LuceneSearcherService( IOptions<LuceneConfiguration> configuration )
        {
            _configuration = configuration.Value;
            _searcherManager = new LuceneSearcherManager(_configuration);
        }

        public List<ILogViewModel> Search(string query, params string[] appNames )
        {
            LuceneSearcherConfiguration configuration = new LuceneSearcherConfiguration
            {
                MaxResult = _configuration.MaxSearch,
                Fields = new[] { "LogLevel", "Exception" },
            };

            if (query == "*")
            {
                configuration.SearchAll(LuceneWantAll.Log);
                return _searcherManager.GetSearcher(appNames).Search(configuration);
            }

            configuration.SearchMethod = SearchMethod.FullText;
            return _searcherManager.GetSearcher(appNames).Search(configuration);
        }

        public List<ILogViewModel> GetAll(params string[] appNames)
        {
            LuceneSearcherConfiguration configuration = new LuceneSearcherConfiguration
            {
                MaxResult = _configuration.MaxSearch,
                Fields = new[] { "LogLevel" },
            };

            configuration.SearchAll(LuceneWantAll.Log);

            return _searcherManager.GetSearcher(appNames).Search(configuration);
        }

        /// <summary>
        /// Return the selected log.
        /// </summary>
        /// <param name="monitorId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="fields"></param>
        /// <param name="logLevel"></param>
        /// <param name="query"></param>
        /// <param name="appNames"></param>
        /// <returns></returns>
        public List<ILogViewModel> GetLogWithFilters( string monitorId, DateTime start, DateTime end, string[] fields, string[] logLevel, string query,  params string[] appNames)
        {
            LuceneSearcherConfiguration configuration = new LuceneSearcherConfiguration
            {
                MonitorId = monitorId,
                DateStart = start,
                DateEnd = end,
                Fields = fields,
                LogLevel = logLevel,
                Query = query,
                MaxResult = _configuration.MaxSearch
            };
            if (configuration.Fields == null)
                configuration.Fields = new[] { "LogLevel" };
            return LogsPrettifier(_searcherManager.GetSearcher(appNames)?.Search(configuration) ?? new List<ILogViewModel>(), 0).Item1;
        }

        public List<string> GetMonitorIdList()
        {
            LuceneSearcherConfiguration configuration = new LuceneSearcherConfiguration();
            return _searcherManager.GetSearcher(GetAppNameList().ToArray()).GetAllMonitorId().ToList();
        }

        /// <summary>
        /// Return of the App Name indexed by Lucene.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAppNameList()
        {
            return _searcherManager.AppName.ToList();
        }

        private (List<ILogViewModel> logs, int index) LogsPrettifier(List<ILogViewModel> logs, int index)
        {
            var indexSnapshot = index;
            for (; index < logs.Count; index += 1)
            {
                if (logs[index].LogType == ELogType.OpenGroup)
                {
                    if ( !(logs[index] is OpenGroupViewModel parent) )
                        throw new InvalidOperationException( nameof( parent ) );
                    var groupLogs = LogsPrettifier(logs, index + 1);
                    parent.GroupLogs = groupLogs.logs;
                    logs.RemoveRange(index + 1, groupLogs.index - index);
                }
                else if (logs[index].LogType == ELogType.CloseGroup)
                {
                    return (logs.GetRange(indexSnapshot, index - indexSnapshot + 1), index);
                }
            }
            return (logs, indexSnapshot);
        }
    }
}
