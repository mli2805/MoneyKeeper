using System;

namespace KeeperDomain
{
    public class LibResult
    {
        public readonly bool IsSuccess;
        public readonly object Payload;
        public readonly Exception Exception;
        public readonly string Where;

        public LibResult()
        {
            IsSuccess = true;
        }

        public LibResult(bool isSuccess, object payload)
        {
            IsSuccess = isSuccess;
            Payload = payload;
        }

        public LibResult(Exception exception, string where = null)
        {
            Exception = exception;
            Where = where;
        }
    }
}