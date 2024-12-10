using System.Net;

namespace PowerToys_Run_Tempo.jira.api;

public class JiraApiException(HttpStatusCode statusCode) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}