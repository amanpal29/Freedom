using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    public class CompositeExceptionHandler : List<IWebExceptionHandler>, IExceptionHandler
    {
        private readonly IExceptionHandler _defaultExceptionHandler;

        public CompositeExceptionHandler(IExceptionHandler defaultExceptionHandler)
        {
            if (defaultExceptionHandler == null)
                throw new ArgumentNullException(nameof(defaultExceptionHandler));

            _defaultExceptionHandler = defaultExceptionHandler;
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (IWebExceptionHandler webExceptionHandler in this)
                if (webExceptionHandler.ShouldHandle(context))
                    return webExceptionHandler.HandleAsync(context, cancellationToken);

            return _defaultExceptionHandler.HandleAsync(context, cancellationToken);
        }
    }
}
