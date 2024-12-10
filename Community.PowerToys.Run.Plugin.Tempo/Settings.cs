using System.Text;
using Microsoft.PowerToys.Settings.UI.Library;

namespace PowerToys_Run_Tempo;

public class Settings
{
    public string? AccountId { get; set; }
    public string? JiraEndpoint { get; set; }
    public string? JiraUsername { get; set; }
    public string? JiraToken { get; set; }
    public string? TempoToken { get; set; }

    internal IEnumerable<PluginAdditionalOption> GetAdditionalOptions()
    {
        return
        [
            new PluginAdditionalOption
            {
                Key = nameof(AccountId),
                DisplayLabel = "Account ID",
                DisplayDescription = "Jira account ID, you can get by clicking on your profile icon, then 'Profile' and it'll appear in the URL",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = AccountId,
            },
            new PluginAdditionalOption
            {
                Key = nameof(JiraEndpoint),
                DisplayLabel = "Jira Endpoint",
                DisplayDescription = "Jira instance endpoint (https://yourdomain.atlassian.net)",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = JiraEndpoint,
            },
            new PluginAdditionalOption
            {
                Key = nameof(JiraUsername),
                DisplayLabel = "Jira Username",
                DisplayDescription = "Jira username (your mail)",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = JiraUsername,
            },
            new PluginAdditionalOption
            {
                Key = nameof(JiraToken),
                DisplayLabel = "Jira API Token",
                DisplayDescription = "Generate it here https://id.atlassian.com/manage-profile/security/api-tokens",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = JiraToken,
            },
            new PluginAdditionalOption
            {
                Key = nameof(TempoToken),
                DisplayLabel = "Tempo API Token",
                DisplayDescription = "Generate it here https://yourdomain.atlassian.net/plugins/servlet/ac/io.tempo.jira/tempo-app#!/configuration/api-integration",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = TempoToken,
            },
        ];
    }

    public void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions)
    {
        ArgumentNullException.ThrowIfNull(additionalOptions);

        var options = additionalOptions.ToList();
        AccountId = options.Find(x => x.Key == nameof(AccountId))?.TextValue ?? AccountId;
        JiraEndpoint = options.Find(x => x.Key == nameof(JiraEndpoint))?.TextValue ?? JiraEndpoint;
        JiraUsername = options.Find(x => x.Key == nameof(JiraUsername))?.TextValue ?? JiraUsername;
        JiraToken = options.Find(x => x.Key == nameof(JiraToken))?.TextValue ?? JiraToken;
        TempoToken = options.Find(x => x.Key == nameof(TempoToken))?.TextValue ?? TempoToken;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Settings{");

        sb.Append("AccountId:");
        sb.Append(AccountId);

        sb.Append(", JiraEndpoint");
        sb.Append(JiraEndpoint);

        sb.Append(", JiraUsername");
        sb.Append(JiraUsername);

        sb.Append(", JiraToken");
        sb.Append(GetSaveValue(JiraToken));

        sb.Append(", TempoToken");
        sb.Append(GetSaveValue(TempoToken));

        sb.Append('}');
        return sb.ToString();
    }

    private static string? GetSaveValue(string? value)
    {
        return string.IsNullOrEmpty(value) ? value : new string('*', value.Length);
    }
}