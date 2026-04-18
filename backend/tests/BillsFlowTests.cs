using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using InvoiceManager.Application.Audit;
using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Common;
using InvoiceManager.Tests.Infrastructure;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class BillsFlowTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task BillsEndpoint_ReturnsSeededBills()
    {
        await AuthorizeAsync();

        var payload = await _client.GetFromJsonAsync<PagedResult<BillListItemDto>>("/api/bills", TestJson.Options);

        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Items);
    }

    [Fact]
    public async Task BillsEndpoint_CanFilterByBillType()
    {
        await AuthorizeAsync();

        var payload = await _client.GetFromJsonAsync<PagedResult<BillListItemDto>>("/api/bills?billType=Tax", TestJson.Options);

        Assert.NotNull(payload);
        Assert.All(payload!.Items, bill => Assert.Equal("Tax", bill.Type.ToString()));
    }

    [Fact]
    public async Task CreatingBill_ThenUploadingAttachment_WritesAuditLog()
    {
        await AuthorizeAsync();
        var categoryId = await GetCategoryIdAsync("Water");

        var createResponse = await _client.PostAsJsonAsync("/api/bills", new
        {
            type = "Water",
            billCategoryId = categoryId,
            paymentStatus = "Pending",
            referenceNumber = "WTR-TEST-100",
            customerName = "Integration Customer",
            propertyName = "Integration Test Building",
            providerName = "Test Water",
            accountNumber = "ACC-100",
            amount = 4567.89m,
            currency = "TWD",
            periodStart = "2026-04-01",
            periodEnd = "2026-04-30",
            issueDate = "2026-05-01",
            dueDate = "2026-05-20",
            paidDate = (string?)null,
            notes = "Integration test bill",
            keywords = "test water"
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BillDetailDto>(TestJson.Options);
        Assert.NotNull(created);

        using var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("sample file")), "file", "sample.txt");

        var uploadResponse = await _client.PostAsync($"/api/bills/{created!.Id}/attachments", formData);
        uploadResponse.EnsureSuccessStatusCode();

        var detail = await _client.GetFromJsonAsync<BillDetailDto>($"/api/bills/{created.Id}", TestJson.Options);
        Assert.NotNull(detail);
        Assert.Single(detail!.Attachments);

        var audit = await _client.GetFromJsonAsync<PagedResult<AuditLogDto>>("/api/audit-logs", TestJson.Options);
        Assert.NotNull(audit);
        Assert.Contains(audit!.Items, item => item.Action == "bill.created");
        Assert.Contains(audit.Items, item => item.Action == "bill.attachment.uploaded");
    }

    [Fact]
    public async Task InvalidBillRequest_ReturnsValidationProblem()
    {
        await AuthorizeAsync();

        var response = await _client.PostAsJsonAsync("/api/bills", new
        {
            type = "Water",
            billCategoryId = Guid.Empty,
            paymentStatus = "Pending",
            referenceNumber = "",
            customerName = "",
            propertyName = "",
            providerName = "",
            accountNumber = "",
            amount = 0,
            currency = "",
            periodStart = "2026-05-01",
            periodEnd = "2026-04-01",
            issueDate = "2026-05-20",
            dueDate = "2026-05-01",
            paidDate = (string?)null,
            notes = "",
            keywords = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotCreateBill()
    {
        var auth = await TestAuthHelper.LoginAsViewerAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var categoryId = await GetCategoryIdAsync("Water");

        var response = await _client.PostAsJsonAsync("/api/bills", new
        {
            type = "Water",
            billCategoryId = categoryId,
            paymentStatus = "Pending",
            referenceNumber = "WTR-TEST-200",
            customerName = "Viewer Customer",
            propertyName = "Viewer Building",
            providerName = "Test Water",
            accountNumber = "ACC-200",
            amount = 1234m,
            currency = "TWD",
            periodStart = "2026-04-01",
            periodEnd = "2026-04-30",
            issueDate = "2026-05-01",
            dueDate = "2026-05-15",
            paidDate = (string?)null,
            notes = "Should be blocked",
            keywords = "viewer"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task AuthorizeAsync()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private async Task<Guid> GetCategoryIdAsync(string type)
    {
        var categories = await _client.GetFromJsonAsync<IReadOnlyCollection<BillCategoryDto>>("/api/categories?includeInactive=true", TestJson.Options);
        var category = categories?.FirstOrDefault(x => x.Type.ToString() == type && x.IsActive);
        return category?.Id ?? throw new InvalidOperationException($"Missing category for {type}.");
    }
}
