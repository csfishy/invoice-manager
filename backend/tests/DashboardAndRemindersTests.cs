using System.Net.Http.Headers;
using System.Net.Http.Json;
using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Dashboard;
using InvoiceManager.Tests.Infrastructure;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class DashboardAndRemindersTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task DashboardSummary_ReturnsPhase5Fields()
    {
        await AuthorizeAsAdminAsync();

        var summary = await _client.GetFromJsonAsync<DashboardSummaryDto>("/api/dashboard/summary", TestJson.Options);

        Assert.NotNull(summary);
        Assert.True(summary!.TotalUnpaidAmount >= 0);
        Assert.True(summary.BillsDueThisWeek >= 0);
        Assert.NotNull(summary.DueSoonBills);
        Assert.NotNull(summary.OverdueBillList);
        Assert.NotNull(summary.LatestUploadedBills);
        Assert.NotNull(summary.StorageUsage);
    }

    [Fact]
    public async Task Admin_CanManageReminderRules()
    {
        await AuthorizeAsAdminAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/reminder-rules", new
        {
            name = "Weekly Water Reminder",
            billCategoryId = (Guid?)null,
            billType = "Water",
            daysBeforeDue = 5,
            recipient = "ops@example.local",
            channel = "Email",
            isEnabled = true
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ReminderRuleDto>(TestJson.Options);
        Assert.NotNull(created);

        var list = await _client.GetFromJsonAsync<IReadOnlyCollection<ReminderRuleDto>>("/api/reminder-rules", TestJson.Options);
        Assert.NotNull(list);
        Assert.Contains(list!, x => x.Id == created!.Id);

        var updateResponse = await _client.PutAsJsonAsync($"/api/reminder-rules/{created!.Id}", new
        {
            name = "Weekly Water Reminder Updated",
            billCategoryId = (Guid?)null,
            billType = "Water",
            daysBeforeDue = 3,
            recipient = "finance@example.local",
            channel = "InApp",
            isEnabled = false
        });

        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await _client.DeleteAsync($"/api/reminder-rules/{created.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private async Task AuthorizeAsAdminAsync()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }
}
