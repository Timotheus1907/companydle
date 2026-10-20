using System.Net;
using System.Text;
using Companydle.Services;

namespace Companydle.Tests;

public class CompanyDataServiceTests
{
    private const string MinimalJson = """
        {
          "dataAsOf": "2026-10-20",
          "currency": "USD",
          "answerOrder": ["acme"],
          "companies": [
            {
              "id": "acme",
              "name": "Acme",
              "aliases": ["ACME"],
              "country": "United States",
              "continent": "North America",
              "sector": "Technology",
              "industry": "Software",
              "exchange": "NASDAQ",
              "founded": 2000,
              "employees": 1000,
              "revenueUsd": 1000000,
              "netProfitUsd": 100000,
              "marketCapUsd": 5000000,
              "effectiveTaxRate": 20.0,
              "fiscalYear": 2025,
              "isAnswerCandidate": true,
              "confidence": "medium"
            }
          ]
        }
        """;

    [Test]
    public async Task LoadAsync_UsesRelativePathUnderTheAppBase()
    {
        Uri? requestedUri = null;
        var handler = new StubHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse(MinimalJson);
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/companydle/") };
        var service = new CompanyDataService(http);

        await service.LoadAsync();

        Assert.That(requestedUri, Is.EqualTo(new Uri("https://example.test/companydle/data/companies.json")));
    }

    [Test]
    public async Task LoadAsync_CachesUntilForced()
    {
        var handler = new StubHandler(_ => JsonResponse(MinimalJson));
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var service = new CompanyDataService(http);

        await service.LoadAsync();
        await service.LoadAsync();
        Assert.That(handler.Calls, Is.EqualTo(1));

        await service.LoadAsync(forceReload: true);
        Assert.That(handler.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task LoadAsync_CanRetryAfterFailure()
    {
        var calls = 0;
        var handler = new StubHandler(_ =>
        {
            calls++;
            return calls == 1
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : JsonResponse(MinimalJson);
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var service = new CompanyDataService(http);

        var threw = false;
        try
        {
            await service.LoadAsync();
        }
        catch (HttpRequestException)
        {
            threw = true;
        }

        Assert.That(threw, Is.True);
        var data = await service.LoadAsync();

        Assert.That(data.Companies.Single().Id, Is.EqualTo("acme"));
        Assert.That(handler.Calls, Is.EqualTo(2));
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(respond(request));
        }
    }
}
