using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using PowerToys_Run_Tempo.api.tempo.model;
using PowerToys_Run_Tempo.api.tempo.model.model;
using Wox.Plugin.Logger;

namespace PowerToys_Run_Tempo.tempo.api;

public class TempoApi
{
    private const string Endpoint = "https://api.tempo.io";

    private readonly HttpClient _client;

    public TempoApi(string token)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public AddWorklogResponse? AddWorklog(AddWorklogSchema schema)
    {        
        LogWrapper.Info($"Adding worklog {schema}", GetType());
        var (_, content) = Send(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(new Uri(Endpoint), "/4/worklogs"),
            Content = new StringContent(
                JsonSerializer.Serialize(schema),
                Encoding.UTF8,
                MediaTypeNames.Application.Json),
        });
        return JsonSerializer.Deserialize<AddWorklogResponse>(content);
    }

    public void DeleteWorklog(string id)
    {
        LogWrapper.Info($"Deleting worklog with id {id}", GetType());
        Send(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(new Uri(Endpoint), $"/4/worklogs/{id}"),
        });
    }

    private (HttpResponseMessage, string) Send(HttpRequestMessage request)
    {
        var response = _client.Send(request);

        using var stream = response.Content.ReadAsStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();

        if (!response.IsSuccessStatusCode)
        {
            ErrorResponse? errorResponse = null;
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content);
            }

            throw new TempoApiException(response.StatusCode, errorResponse);
        }

        return (response, content);
    }
}