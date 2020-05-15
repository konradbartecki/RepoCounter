using Me.Bartecki.RepoCounter.Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Me.Bartecki.RepoCounter.Api.Services
{
    public class ErrorCodeMapper
    {
        private readonly bool _isInDevelopment;

        public ErrorCodeMapper(bool isInDevelopment)
        {
            _isInDevelopment = isInDevelopment;
        }

        private HttpStatusCode GetHttpStatusCode(ErrorCodes errorCode)
        {
            switch (errorCode)
            {
                case ErrorCodes.UserNotFound:
                case ErrorCodes.UserHasNoRepositories:
                    return HttpStatusCode.NotFound;
                case ErrorCodes.RepositorySource_UnableToReach:
                    return HttpStatusCode.FailedDependency;
                default:
                    return HttpStatusCode.InternalServerError;
            }
        }

        public ObjectResult GetUserFriendlyError(RepoCounterApiException exception)
        {
            //TODO: Log the exception
            //Alternatively when isInDevelopment then we could actually throw this exception.

            var errorCode = exception.ErrorCode;
            var httpStatusCode = (int)GetHttpStatusCode(errorCode);
            var message = new ErrorMessage()
            {
                ErrorCode = errorCode,
                Message = _isInDevelopment ?
                    exception.ToString() :
                    $"{errorCode.ToString()}: {exception.Message}"
            };
            return new ObjectResult(message) { StatusCode = httpStatusCode };
        }
    }
}
