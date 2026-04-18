using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Common;
using InvoiceManager.Tests.Infrastructure;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class CategoryManagementTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteCustomCategory()
    {
        await AuthorizeAsAdminAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/categories", new
        {
            name = "Water Special Cases",
            type = "Water",
            description = "Custom category for special properties.",
            sortOrder = 90,
            isActive = true,
            isSystemDefault = false
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BillCategoryDto>(TestJson.Options);
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/categories/{created!.Id}", new
        {
            name = "Water VIP Properties",
            type = "Water",
            description = "Updated custom category.",
            sortOrder = 95,
            isActive = true,
            isSystemDefault = false
        });

        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await _client.DeleteAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotManageCategories()
    {
        await AuthorizeAsViewerAsync();

        var response = await _client.PostAsJsonAsync("/api/categories", new
        {
            name = "Blocked Category",
            type = "Tax",
            description = "Viewer should not create categories.",
            sortOrder = 1,
            isActive = true,
            isSystemDefault = false
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletingSystemDefaultCategory_ReturnsConflict()
    {
        await AuthorizeAsAdminAsync();

        var categories = await _client.GetFromJsonAsync<IReadOnlyCollection<BillCategoryDto>>("/api/categories?includeInactive=true", TestJson.Options);
        Assert.NotNull(categories);

        var systemDefault = categories!.First(x => x.IsSystemDefault);
        var response = await _client.DeleteAsync($"/api/categories/{systemDefault.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task AuthorizeAsAdminAsync()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private async Task AuthorizeAsViewerAsync()
    {
        var auth = await TestAuthHelper.LoginAsViewerAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }
}
