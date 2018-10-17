using System;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Freedom.Domain.Exceptions;
using Freedom.Extensions;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    /// <summary>
    /// Handles when a sql insert/update/delete fails because of Primary Key, Foreign Key, Unique Index or other constraint.
    /// When this happens the server will return a HTTP 409 Conflit error response with some information about the SqlException
    /// </summary>
    public class SqlConstraintViolationExceptionHandler : WebExceptionHandler
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Conflict;

        public override HttpError CreateHttpError(Exception exception, bool includeErrorDetail)
        {
            HttpError result = base.CreateHttpError(exception, includeErrorDetail);

            SqlException sqlException = exception.Find<SqlException>();

            if (sqlException == null)
                return result;  // Should never happen

            SqlConstraintType sqlConstraintType = (SqlConstraintType) sqlException.Number;

            result.Add(nameof(SqlConstraintType), sqlConstraintType.ToString());
            result.Add(nameof(sqlException.Class), sqlException.Class);
            result.Add(nameof(sqlException.LineNumber), sqlException.LineNumber);
            result.Add(nameof(sqlException.Number), sqlException.Number);
            result.Add(nameof(sqlException.Procedure), sqlException.Procedure);
            result.Add(nameof(sqlException.Server), sqlException.Server);
            result.Add(nameof(sqlException.Source), sqlException.Source);
            result.Add(nameof(sqlException.State), sqlException.State);

            return result;
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            SqlException sqlException = context.Exception.Find<SqlException>();

            if (sqlException == null)
                return false;

            switch (sqlException.Number)
            {
                case 515: // Not-Null Constraint Violated
                case 547: // Foreign Key or other Check Constraint Violated
                case 2601: // Unique Index Constraint Violated
                case 2627: // Primary Key Constraint Violated
                    return true;

                default:
                    return false;
            }
        }
    }
}