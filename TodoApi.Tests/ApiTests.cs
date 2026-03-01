using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.Tests;

public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Register_ReturnsToken()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "123456"
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.False(string.IsNullOrWhiteSpace(auth?.Token));
    }

    [Fact]
    public async Task AuthenticatedUser_CanCrudTasks()
    {
        var client = factory.CreateClient();
        var token = await RegisterAndGetToken(client, $"{Guid.NewGuid():N}@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Minha tarefa",
            content = "Conteúdo",
            date = DateTime.UtcNow,
            status = "Pendente"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoTaskResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/tasks/{created!.Id}");
        getResponse.EnsureSuccessStatusCode();

        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = "Atualizada",
            content = "Novo conteúdo",
            date = DateTime.UtcNow,
            status = "Concluída"
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private static async Task<string> RegisterAndGetToken(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "123456"
        });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    private sealed record AuthResponse(string Token);
    private sealed record TodoTaskResponse(int Id);
}
