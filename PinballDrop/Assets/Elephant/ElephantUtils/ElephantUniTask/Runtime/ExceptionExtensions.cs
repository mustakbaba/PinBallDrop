
using System;

namespace ElephantUniTask.Threading.Tasks
{
    public static class ExceptionExtensions
    {
        public static bool IsOperationCanceledException(this Exception exception)
        {
            return exception is OperationCanceledException;
        }
    }
}

