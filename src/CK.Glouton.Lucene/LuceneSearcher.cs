﻿using CK.Glouton.Model.Logs;
using CK.Glouton.Model.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CK.Glouton.Lucene
{
    public class LuceneSearcher : ILuceneSearcher
    {
        private readonly IndexSearcher _indexSearcher;

        /// <summary>
        /// Basic Searcher in a Lucene index for CK.Monitoring log.
        /// </summary>
        /// <param name="luceneConfiguration"></param>
        /// <param name="fields"></param>
        public LuceneSearcher( MultiReader multiReader )
        {
            _indexSearcher = new IndexSearcher( multiReader );
        }

        internal MultiFieldQueryParser QueryParser { get; }

        /// <summary>
        /// Search into Lucene index.
        /// If the <see cref="LuceneSearcherConfiguration"/> is not correct return null.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        public List<ILogViewModel> Search( LuceneSearcherConfiguration searchConfiguration )
        {
            if( !CheckSearchConfiguration( searchConfiguration ) )
                return null;

            if( searchConfiguration.SearchMethod == SearchMethod.FullText )
            {
                return Search( searchConfiguration, new MultiFieldQueryParser( LuceneVersion.LUCENE_48,
                    searchConfiguration.Fields,
                    new StandardAnalyzer( LuceneVersion.LUCENE_48 ) ).Parse( searchConfiguration.Query ) );
            }

            if( searchConfiguration.WantAll )
                return Search( searchConfiguration, GetAll( searchConfiguration.All ) );

            return CreateLogsResult( _indexSearcher?.Search( ( CreateQuery( searchConfiguration ) ), (int)searchConfiguration.MaxResult ) );
        }

        private Query GetAll( LuceneWantAll all )
        {
            Query query = null;
            switch( all )
            {
                case LuceneWantAll.Exception:
                    query = new QueryParser( LuceneVersion.LUCENE_48,
                                                    LogField.LOG_LEVEL,
                                                    new StandardAnalyzer( LuceneVersion.LUCENE_48 ) )
                                                        .Parse( "Fatal" );
                    break;
                case LuceneWantAll.Log:
                    query = new WildcardQuery( new Term( LogField.LOG_LEVEL, "*" ) );
                    break;
            }
            return query;
        }

        /// <summary>
        /// Create a query to search into the logs.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private Query CreateQuery( ILuceneSearcherConfiguration configuration )
        {
            var bQuery = new BooleanQuery();

            if( configuration.MonitorId != null ) bQuery.Add( CreateMonitorIdQuery( configuration ), Occur.MUST );
            if( configuration.Fields != null ) bQuery.Add( CreateFieldQuery( configuration ), Occur.MUST );
            if( configuration.DateEnd.Year != 1 && configuration.DateStart.Year != 1 ) bQuery.Add( CreateTimeQuery( configuration ), Occur.MUST );
            if( configuration.LogLevel != null ) bQuery.Add( CreateLogLevelQuery( configuration ), Occur.MUST );

            return bQuery;
        }

        private Query CreateTimeQuery( ILuceneSearcherConfiguration configuration )
        {
            return new TermRangeQuery( LogField.LOG_TIME,
                new BytesRef( DateTools.DateToString( configuration.DateStart, DateTools.Resolution.MILLISECOND ) ),
                new BytesRef( DateTools.DateToString( configuration.DateEnd, DateTools.Resolution.MILLISECOND ) ),
                includeLower: true,
                includeUpper: true );
        }

        private Query CreateLogLevelQuery( ILuceneSearcherConfiguration configuration )
        {
            var levelParser = new QueryParser( LuceneVersion.LUCENE_48,
               LogField.LOG_LEVEL,
               new StandardAnalyzer( LuceneVersion.LUCENE_48 ) );

            var bLevelQuery = new BooleanQuery();
            foreach( var level in configuration.LogLevel )
            {
                bLevelQuery.Add( levelParser.Parse( level ), Occur.SHOULD );
            }
            return bLevelQuery;
        }

        private Query CreateMonitorIdQuery( ILuceneSearcherConfiguration configuration )
        {
            return new TermQuery( new Term( LogField.MONITOR_ID, configuration.MonitorId ) );
        }

        private Query CreateFieldQuery( ILuceneSearcherConfiguration configuration )
        {
            var bFieldQuery = new BooleanQuery();
            foreach( var field in configuration.Fields )
            {
                if( field == LogField.TEXT && configuration.Query != null )
                    bFieldQuery.Add( new QueryParser( LuceneVersion.LUCENE_48, field, new StandardAnalyzer( LuceneVersion.LUCENE_48 ) ).Parse( configuration.Query ), Occur.SHOULD );
                else
                    bFieldQuery.Add( new WildcardQuery( new Term( field, configuration.Query ) ), Occur.SHOULD );
            }
            return bFieldQuery;
        }

        private List<ILogViewModel> Search( LuceneSearcherConfiguration configuration, Query searchQuery )
        {
            return CreateLogsResult( _indexSearcher?.Search( searchQuery, (int)configuration.MaxResult ) );
        }
        private List<ILogViewModel> Search( Query searchQuery, int maxResult )
        {
            return CreateLogsResult( _indexSearcher?.Search( searchQuery, maxResult ) );
        }

        public Document GetDocument( ScoreDoc scoreDoc )
        {
            return _indexSearcher?.Doc( scoreDoc.Doc );
        }

        public Document GetDocument( Query query, int maxResult )
        {
            return GetDocument( _indexSearcher?.Search( query, maxResult ).ScoreDocs.First() );
        }

        public Document GetDocument( string key, string value, int maxResult )
        {
            return GetDocument( _indexSearcher?.Search( new TermQuery( new Term( key, value ) ), maxResult ).ScoreDocs.First() );
        }

        private bool CheckSearchConfiguration( LuceneSearcherConfiguration configuration ) // TODO: Check if the check is good.
        {
            if( configuration == null )
                throw new ArgumentNullException( nameof( configuration ) );

            if( configuration.AppName == null ||
                configuration.Fields == null ||
                configuration.MaxResult == 0 )
                throw new ArgumentException( nameof( configuration ) );
            return true;
        }

        /// <summary>
        /// Get all monitor id in all AppName.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllMonitorID()
        {
            var hits = _indexSearcher.Search( new WildcardQuery( new Term( LogField.MONITOR_ID_LIST, "*" ) ), 999 );
            var monitorIds = new List<string>();
            foreach( var doc in hits.ScoreDocs )
            {
                var document = GetDocument( doc );
                var monitorId = document.Get( LogField.MONITOR_ID_LIST ).Split( ' ' );
                foreach( var id in monitorIds )
                    monitorIds.Add( id );
            }

            return monitorIds;
        }

        /// <summary>
        /// Get the <see cref="TopDocs"/> of a search.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public TopDocs QuerySearch( Query query, int maxResult ) // TODO: Get a good name
        {
            return _indexSearcher?.Search( query, maxResult );
        }

        private List<ILogViewModel> CreateLogsResult( TopDocs topDocs )
        {
            var result = new List<ILogViewModel>();
            foreach( var scoreDoc in topDocs.ScoreDocs )
            {
                var document = GetDocument( scoreDoc );
                switch( document.Get( LogField.LOG_TYPE ) )
                {
                    case "Line":
                        result.Add( LineViewModel.Get( this, document ) );
                        break;
                    case "CloseGroup":
                        result.Add( CloseGroupViewModel.Get( this, document ) );
                        break;
                    case "OpenGroup":
                        result.Add( OpenGroupViewModel.Get( this, document ) );
                        break;
                    default:
                        throw new ArgumentException( nameof( document ) );
                }
            }
            return result;
        }
    }
}
