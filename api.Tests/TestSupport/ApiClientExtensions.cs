using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shouldly;

namespace BikeShop.Api.Tests.TestSupport;

public static class ApiClientExtensions
{
    // Matches the API: camelCase names and enums as strings ("Owner").
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public static Task<HttpResponseMessage> SignInAsAsync(this HttpClient client, string username, string password) =>
        client.PostAsJsonAsync("/api/auth/sign-in", new { username, password });

    public static void ShouldHaveMediaType(this HttpResponseMessage response, string expectedMediaType) =>
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe(expectedMediaType);

    public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
    {
        T? body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return body ?? throw new InvalidOperationException($"Expected a {typeof(T).Name} but the body was JSON null.");
    }
}
