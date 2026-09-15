using System;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using Contoso.LegacyBank.Portal.Domain;

namespace Contoso.LegacyBank.Portal.Services
{
    public sealed class StatementApiClient : IStatementGateway
    {
        private static readonly HttpClient HttpClient = CreateClient();
        private readonly Uri _endpoint;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public StatementApiClient(Uri endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException("endpoint");
        }

        public StatementResult RequestStatement(StatementRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var json = _serializer.Serialize(new
            {
                customerNumber = request.CustomerNumber,
                accountNumber = request.AccountNumber,
                fromDate = request.FromDate.ToString("yyyy-MM-dd"),
                toDate = request.ToDate.ToString("yyyy-MM-dd")
            });

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = HttpClient.PostAsync(_endpoint, content).GetAwaiter().GetResult())
            {
                return ReadResponse(response, "request");
            }
        }

        public StatementResult GetStatement(string statementId)
        {
            if (string.IsNullOrWhiteSpace(statementId))
            {
                throw new ArgumentException("A statement ID is required.", "statementId");
            }

            var uri = new Uri(_endpoint.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(statementId));
            using (var response = HttpClient.GetAsync(uri).GetAwaiter().GetResult())
            {
                return ReadResponse(response, "status");
            }
        }

        private StatementResult ReadResponse(HttpResponseMessage response, string operation)
        {
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                throw new StatementApiException(
                    string.Format("Statement {0} failed ({1} {2}). {3}", operation, (int)response.StatusCode,
                        response.ReasonPhrase, ExtractMessage(json)));
            }

            var result = _serializer.Deserialize<StatementResult>(json);
            if (result == null)
            {
                throw new StatementApiException("The statement service returned an empty response.");
            }

            return result;
        }

        private string ExtractMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            try
            {
                var error = _serializer.Deserialize<StatementResult>(json);
                return error == null ? string.Empty : error.Message;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        private static HttpClient CreateClient()
        {
            return new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }
    }

    public sealed class StatementApiException : Exception
    {
        public StatementApiException(string message) : base(message)
        {
        }
    }
}
