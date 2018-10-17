using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Freedom.Extensions;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Freedom.WebApi.Results
{
    public class RawTableResult : IHttpActionResult
    {
        public static readonly string[] SupportedMediaTypes =
        {
            "application/json",
            "application/xml",
            "text/json",
            "text/xml"
        };

        private string _mediaType = SupportedMediaTypes[0];
        private Encoding _encoding = Encoding.UTF8;
        private CancellationToken _cancellationToken = CancellationToken.None;

        public RawTableResult(HttpRequestMessage request, string sql, params KeyValuePair<string, object>[] parameters)
            : this(request, sql, (IEnumerable<KeyValuePair<string, object>>) parameters)
        {
        }

        public RawTableResult(HttpRequestMessage request, string sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql));

            Sql = sql;
            Parameters = new Dictionary<string, object>();

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                    Parameters.Add(parameter);
            }

            RootElementName = "Table";
            ItemElementName = "Row";

            NegotiateMediaType(request);
        }

        private void NegotiateMediaType(HttpRequestMessage request)
        {
            foreach (string mediaType in SupportedMediaTypes)
            {
                MediaTypeWithQualityHeaderValue matchingHeader = request.Headers.Accept
                    .FirstOrDefault(a => string.Equals(a.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));

                if (matchingHeader == null) continue;

                _mediaType = mediaType;

                try
                {
                    _encoding = Encoding.GetEncoding(matchingHeader.CharSet);
                }
                catch (ArithmeticException)
                {
                    _encoding = Encoding.UTF8;
                }
            }
        }

        public string Sql { get; }

        public IDictionary<string, object> Parameters { get; }

        public string Namespace { get; set; }

        public string RootElementName { get; set; }

        public string ItemElementName { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue(_mediaType);

            mediaType.CharSet = _encoding.WebName;

            switch (_mediaType)
            {
                case "application/json":
                case "text/json":
                    response.Content = new PushStreamContent(OnStreamAvailableJson, mediaType);
                    break;

                case "application/xml":
                case "text/xml":
                    response.Content = new PushStreamContent(OnStreamAvailableXml, mediaType);
                    break;

                default:
                    throw new InvalidOperationException("Internal Error: MediaType is not supported.");
            }

            return Task.FromResult(response);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private DbCommand BuildCommand(DbConnection connection)
        {
            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = Sql;

            foreach (KeyValuePair<string, object> parameter in Parameters)
                command.CreateParameter(parameter.Key, parameter.Value);

            return command;
        }

        private async Task OnStreamAvailableXml(Stream stream, HttpContent content, TransportContext context)
        {
            try
            {
                using (DbConnection connection = IoC.Get<DbConnection>())
                {
                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync(_cancellationToken);

                    DbCommand command = BuildCommand(connection);

                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Async = true,
                        Encoding = _encoding,
                        Indent = _mediaType == "text/xml"
                    };

                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    using (DbDataReader reader = await command.ExecuteReaderAsync(_cancellationToken))
                    {
                        await writer.WriteStartDocumentAsync();

                        await writer.WriteStartElementAsync(null, RootElementName, null);

                        if (!string.IsNullOrWhiteSpace(Namespace))
                            await writer.WriteAttributeStringAsync(null, "xmlns", null, Namespace);

                        while (await reader.ReadAsync(_cancellationToken))
                        {
                            await writer.WriteStartElementAsync(null, ItemElementName, null);

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (await reader.IsDBNullAsync(i, _cancellationToken))
                                    continue;

                                string value;

                                switch (reader.GetDataTypeName(i))
                                {
                                    case "date":
                                    case "datetime":
                                    case "datetime2":
                                        value = reader.GetDateTime(i).ToString("o");
                                        break;

                                    default:
                                        value = reader.GetValue(i).ToString();
                                        break;
                                }


                                await writer.WriteElementStringAsync(null, reader.GetName(i), null, value);
                            }

                            await writer.WriteEndElementAsync();
                        }

                        await writer.WriteEndElementAsync();
                        await writer.WriteEndDocumentAsync();
                    }
                }
            }
            finally
            {
                stream?.Close();
            }
        }

        private async Task OnStreamAvailableJson(Stream stream, HttpContent content, TransportContext context)
        {
            try
            {
                using (DbConnection connection = IoC.Get<DbConnection>())
                {
                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync(_cancellationToken);

                    DbCommand command = BuildCommand(connection);

                    using (TextWriter textWriter = new StreamWriter(stream, Encoding.UTF8))
                    using (JsonTextWriter writer = new JsonTextWriter(textWriter))
                    using (DbDataReader reader = await command.ExecuteReaderAsync(_cancellationToken))
                    {
                        writer.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                        writer.Formatting = _mediaType == "text/json" ? Formatting.Indented : Formatting.None;

                        writer.WriteStartArray();

                        while (await reader.ReadAsync(_cancellationToken))
                        {
                            writer.WriteStartObject();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (await reader.IsDBNullAsync(i, _cancellationToken))
                                    continue;

                                writer.WritePropertyName(reader.GetName(i));
                                writer.WriteValue(await reader.GetFieldValueAsync<object>(i, _cancellationToken));
                            }

                            writer.WriteEndObject();
                        }

                        writer.WriteEnd();
                    }
                }
            }
            finally
            {
                stream?.Close();
            }
        }
    }
}