using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waseet.CQRS.Auditing;

/// <summary>
/// Elasticsearch audit logger that automatically creates indices.
/// </summary>
public class ElasticsearchAuditLogger : IAuditLogger
{
    private readonly HttpClient _httpClient;
    private readonly string _indexPrefix;
    private readonly HashSet<string> _createdIndices = new();
    private readonly SemaphoreSlim _indexCreationLock = new(1, 1);

    /// <summary>
    /// Initializes Elasticsearch audit logger.
    /// </summary>
    /// <param name="elasticsearchUrl">Elasticsearch URL (e.g., http://localhost:9200)</param>
    /// <param name="indexPrefix">Index prefix (e.g., "waseet-audit"). Indices will be named {prefix}-{yyyy.MM}</param>
    /// <param name="username">Optional username for authentication</param>
    /// <param name="password">Optional password for authentication</param>
    /// <param name="ignoreSslErrors">Set to true to ignore SSL certificate errors (for development/self-signed certificates)</param>
    public ElasticsearchAuditLogger(
        string elasticsearchUrl, 
        string indexPrefix = "waseet-audit",
        string? username = null,
        string? password = null,
        bool ignoreSslErrors = false)
    {
        _indexPrefix = indexPrefix;
        
        var handler = new HttpClientHandler();
        if (ignoreSslErrors)
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(elasticsearchUrl.TrimEnd('/'))
        };

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get or create index for current month
            var indexName = $"{_indexPrefix}-{entry.Timestamp:yyyy.MM}";
            await EnsureIndexExistsAsync(indexName, cancellationToken);

            // Index the document
            var json = JsonSerializer.Serialize(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(
                $"/{indexName}/_doc/{entry.Id}", 
                content, 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Failed to index audit log to Elasticsearch: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing audit log to Elasticsearch: {ex.Message}");
            // Don't throw - audit logging should not break the application
        }
    }

    public async Task<IEnumerable<AuditLogEntry>> QueryAsync(AuditQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchQuery = BuildSearchQuery(query);
            var json = JsonSerializer.Serialize(searchQuery);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"/{_indexPrefix}-*/_search",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<AuditLogEntry>();
            }

            var result = await response.Content.ReadFromJsonAsync<ElasticsearchSearchResponse>(cancellationToken: cancellationToken);
            return result?.Hits?.Hits?.Select(h => h.Source) ?? Enumerable.Empty<AuditLogEntry>();
        }
        catch
        {
            return Enumerable.Empty<AuditLogEntry>();
        }
    }

    private async Task EnsureIndexExistsAsync(string indexName, CancellationToken cancellationToken)
    {
        if (_createdIndices.Contains(indexName))
        {
            return;
        }

        await _indexCreationLock.WaitAsync(cancellationToken);
        try
        {
            if (_createdIndices.Contains(indexName))
            {
                return;
            }

            // Check if index exists
            var existsResponse = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, $"/{indexName}"),
                cancellationToken);

            if (existsResponse.IsSuccessStatusCode)
            {
                _createdIndices.Add(indexName);
                return;
            }

            // Create index with mapping
            var mapping = new
            {
                mappings = new
                {
                    properties = new
                    {
                        id = new { type = "keyword" },
                        timestamp = new { type = "date" },
                        requestType = new { type = "keyword" },
                        user = new { type = "keyword" },
                        requestData = new { type = "object", enabled = true },
                        responseData = new { type = "object", enabled = true },
                        durationMs = new { type = "long" },
                        success = new { type = "boolean" },
                        errorMessage = new { type = "text" },
                        category = new { type = "keyword" },
                        tags = new { type = "keyword" },
                        ipAddress = new { type = "ip" },
                        metadata = new { type = "object", enabled = true }
                    }
                }
            };

            var json = JsonSerializer.Serialize(mapping);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var createResponse = await _httpClient.PutAsync($"/{indexName}", content, cancellationToken);
            
            if (createResponse.IsSuccessStatusCode)
            {
                _createdIndices.Add(indexName);
                Console.WriteLine($"✅ Created Elasticsearch index: {indexName}");
            }
            else
            {
                var error = await createResponse.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Failed to create Elasticsearch index {indexName}: {error}");
            }
        }
        finally
        {
            _indexCreationLock.Release();
        }
    }

    private object BuildSearchQuery(AuditQuery query)
    {
        var must = new List<object>();

        if (query.From.HasValue || query.To.HasValue)
        {
            var range = new Dictionary<string, object>();
            if (query.From.HasValue) range["gte"] = query.From.Value;
            if (query.To.HasValue) range["lte"] = query.To.Value;
            must.Add(new { range = new { timestamp = range } });
        }

        if (!string.IsNullOrEmpty(query.User))
        {
            must.Add(new { term = new { user = query.User } });
        }

        if (!string.IsNullOrEmpty(query.RequestType))
        {
            must.Add(new { term = new { requestType = query.RequestType } });
        }

        if (!string.IsNullOrEmpty(query.Category))
        {
            must.Add(new { term = new { category = query.Category } });
        }

        if (query.Success.HasValue)
        {
            must.Add(new { term = new { success = query.Success.Value } });
        }

        return new
        {
            query = new { @bool = new { must } },
            size = query.PageSize,
            from = (query.Page - 1) * query.PageSize,
            sort = new[] { new { timestamp = new { order = "desc" } } }
        };
    }

    private class ElasticsearchSearchResponse
    {
        public HitsContainer? Hits { get; set; }
    }

    private class HitsContainer
    {
        public List<HitItem>? Hits { get; set; }
    }

    private class HitItem
    {
        [JsonPropertyName("_source")]
        public AuditLogEntry Source { get; set; } = new();
    }
}
