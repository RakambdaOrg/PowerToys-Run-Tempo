using System.Buffers.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PowerToys_Run_Tempo.jira.api.model;
using Wox.Plugin.Logger;

namespace PowerToys_Run_Tempo.jira.api;

public class JiraApi
{
    private readonly string _endpoint;
    private readonly HttpClient _client;

    public JiraApi(string endpoint, string username, string token)
    {
        _endpoint = endpoint;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{token}")));
    }

    public virtual IssueInfoResponse? GetIssueInfo(string issueKey)
    {
        LogWrapper.Info($"Requesting Jira issue information for {issueKey}", GetType());
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(new Uri(_endpoint), $"/rest/api/latest/issue/{issueKey}?fields=summary")
        };

        var response = _client.Send(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new JiraApiException(response.StatusCode);
        }

        using var stream = response.Content.ReadAsStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();
        return JsonSerializer.Deserialize<IssueInfoResponse>(content);
    }
}