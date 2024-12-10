using System.Net;
using PowerToys_Run_Tempo.api.tempo.model;

namespace PowerToys_Run_Tempo.tempo.api;

public class TempoApiException(HttpStatusCode statusCode, ErrorResponse? errorResponse) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public ErrorResponse? ErrorResponse { get; } = errorResponse;
}