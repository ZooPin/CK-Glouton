﻿using System.Collections.Generic;

namespace CK.Glouton.Model.Lucene.Logs
{
    public interface IExceptionViewModel
    {
        IInnerExceptionViewModel InnerException { get; set; }
        List<IExceptionViewModel> AggregatedExceptions { get; set; }
        string Message { get; set; }
        string StackTrace { get; set; }
    }
}