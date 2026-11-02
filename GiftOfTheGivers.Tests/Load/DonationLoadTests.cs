using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using GiftOfTheGivers.Tests.Integration;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Load;

// A.3: small-scale load test. Configuration (override with environment variables):
//   LOAD_USERS      concurrent users          (default 5)
//   LOAD_ITERATIONS requests per user per step (default 10)
//   LOAD_MAX_MS     allowed 95th percentile    (default 2000 ms)
[Trait("Category", "Load")]
public class DonationLoadTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    private static int Setting(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var v) && v > 0 ? v : fallback;

    public DonationLoadTests(WebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task ConcurrentUsers_BrowseAndDonate_AllRequestsSucceedQuickly()
    {
        var users = Setting("LOAD_USERS", 5);
        var iterations = Setting("LOAD_ITERATIONS", 10);
        var maxP95 = Setting("LOAD_MAX_MS", 2000);

        var timings = new ConcurrentBag<double>();
        var failures = new ConcurrentBag<string>();
        var total = Stopwatch.StartNew();

        // Warm up so JIT/start-up time does not skew the numbers
        await _factory.CreateClient().GetAsync("/");

        var tasks = Enumerable.Range(1, users).Select(async user =>
        {
            var client = _factory.CreateClient();
            for (var i = 0; i < iterations; i++)
            {
                await Timed(client, HttpMethod.Get, "/", null);
                await Timed(client, HttpMethod.Get, "/Donor/Donate", null);
                await Timed(client, HttpMethod.Post, "/Donor/DonateMoney", new Dictionary<string, string>
                {
                    ["DonationAmount"] = (10 + i).ToString(),
                    ["Currency"] = "ZAR",
                    ["Email"] = $"load{user}@example.com",
                    ["DonorName"] = $"Load User {user}"
                });
            }
        }).ToList();

        async Task Timed(HttpClient client, HttpMethod method, string url, Dictionary<string, string>? form)
        {
            var sw = Stopwatch.StartNew();
            var request = new HttpRequestMessage(method, url);
            if (form != null) request.Content = new FormUrlEncodedContent(form);
            var response = await client.SendAsync(request);
            sw.Stop();
            timings.Add(sw.Elapsed.TotalMilliseconds);
            if (response.StatusCode != HttpStatusCode.OK)
                failures.Add($"{method} {url} -> {(int)response.StatusCode}");
        }

        await Task.WhenAll(tasks);
        total.Stop();

        var sorted = timings.OrderBy(t => t).ToArray();
        var p95 = sorted[(int)Math.Ceiling(sorted.Length * 0.95) - 1];

        _output.WriteLine($"Users: {users}, iterations/user: {iterations}, requests: {sorted.Length}");
        _output.WriteLine($"Total time: {total.Elapsed.TotalSeconds:F2}s, throughput: {sorted.Length / total.Elapsed.TotalSeconds:F1} req/s");
        _output.WriteLine($"Average: {sorted.Average():F1} ms, median: {sorted[sorted.Length / 2]:F1} ms, p95: {p95:F1} ms, max: {sorted.Last():F1} ms");
        _output.WriteLine($"Failures: {failures.Count}");

        Assert.Empty(failures);
        Assert.True(p95 <= maxP95, $"95th percentile {p95:F0} ms is above the {maxP95} ms limit.");
    }
}
