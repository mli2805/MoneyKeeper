using System;

namespace KeeperDomain
{
    public class LibResult
    {
        public readonly bool IsSuccess;
        public readonly object Payload;
        public readonly Exception Exception;

        public LibResult()
        {
            IsSuccess = true;
        }

        public LibResult(bool isSuccess, object payload)
        {
            IsSuccess = isSuccess;
            Payload = payload;
        }

        public LibResult(Exception exception)
        {
            Exception = exception;
        }
    }
}