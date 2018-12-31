using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Services.Command;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Command
{
    public class CommandServiceProxy : ICommandService
    {
        private const string MediaType = "application/xml";

        private readonly IHttpClientErrorHandler _errorHandler = IoC.Get<IHttpClientErrorHandler>();

        public async Task<CommandResult> ExecuteAsync(CommandBase command, CancellationToken cancellationToken)
        {
            HttpClient httpClient = IoC.Get<HttpClient>();

            try
            {
                const string uri = "command/execute";

                XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

                using (HttpContent content = new ObjectContent<CommandBase>(command, formatter, MediaType))
                using (HttpResponseMessage response = await httpClient.PostAsync(uri, content, cancellationToken))
                {
                    await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                    return await response.Content.ReadAsAsync<CommandResult>(cancellationToken);
                }
            }
            catch (HttpStatusCommunicationException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new InvalidCommandException(command,
                            "The command could not be executed because it's invalid.", ex);

                    case HttpStatusCode.Conflict:
                        throw new ConstraintViolatedException(ex.Content as IDictionary<string, object>,
                            "The command could not be executed due to a database constraint violation.", ex);

                    case HttpStatusCode.NotImplemented:
                        throw new NotImplementedException(
                            $"The {App.Name} server does not support commands of the type '{command.GetType().Name}'.");
                }

                throw;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(httpClient, ex, cancellationToken);

                throw;
            }
        }
    }
}
