using System.Net;
using System.Text;
using System.Text.Json;
using GiftOfTheGivers.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace GiftOfTheGivers.Tests.Integration;

// Stands in for the deployed Azure Function. It runs the real GenerateTaxCertificate code in-process,
// so the web app -> function path is exercised without needing the Functions host or the network.
public class InProcessFunctionHandler : HttpMessageHandler
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public int Calls;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref Calls);

        if (request.RequestUri!.AbsolutePath.EndsWith("GenerateTaxCertificate", StringComparison.OrdinalIgnoreCase))
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Post;
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var function = new GenerateTaxCertificate(NullLogger<GenerateTaxCertificate>.Instance);
            var result = await function.Run(context.Request);

            return result switch
            {
                OkObjectResult ok => Respond(HttpStatusCode.OK, ok.Value),
                BadRequestObjectResult bad => Respond(HttpStatusCode.BadRequest, bad.Value),
                ObjectResult other => Respond((HttpStatusCode)(other.StatusCode ?? 500), other.Value),
                _ => Respond(HttpStatusCode.InternalServerError, new { message = "Unexpected result" })
            };
        }

        // LogProjectUpdate needs table storage, so just acknowledge it
        return Respond(HttpStatusCode.Created, new { id = "test" });
    }

    private static HttpResponseMessage Respond(HttpStatusCode status, object? value) =>
        new(status) { Content = new StringContent(JsonSerializer.Serialize(value, Json), Encoding.UTF8, "application/json") };
}
