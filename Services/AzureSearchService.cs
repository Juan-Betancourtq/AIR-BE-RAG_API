using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using ResumeChat.RagApi.Configuration;
using ResumeChat.RagApi.Models;

namespace ResumeChat.RagApi.Services;

public interface IAzureSearchService
{
    Task<List<ResumeDocument>> SearchAsync(string query, int maxResults = 5);
    Task<List<ResumeDocument>> VectorSearchAsync(List<float> queryVector, int maxResults = 5);
    Task IndexDocumentAsync(ResumeDocument document);
}

public class AzureSearchService : IAzureSearchService
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly ILogger<AzureSearchService> _logger;

    public AzureSearchService(
        IOptions<AzureSearchConfiguration> config,
        ILogger<AzureSearchService> logger)
    {
        _logger = logger;
        var searchConfig = config.Value;

        _indexClient = new SearchIndexClient(
            new Uri(searchConfig.Endpoint),
            new AzureKeyCredential(searchConfig.ApiKey));

        _searchClient = _indexClient.GetSearchClient(searchConfig.IndexName);
    }

    public async Task<List<ResumeDocument>> SearchAsync(string query, int maxResults = 5)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Size = maxResults,
                IncludeTotalCount = true
            };
            // Don't specify Select to get all available fields
            // The index may not have all expected fields yet

            var searchResults = await _searchClient.SearchAsync<ResumeDocument>(query, searchOptions);

            var documents = new List<ResumeDocument>();
            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                documents.Add(result.Document);
            }

            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            throw;
        }
    }

    public async Task<List<ResumeDocument>> VectorSearchAsync(List<float> queryVector, int maxResults = 5)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Size = maxResults,
                // Vector search configuration would go here
                // This is a simplified version
            };

            var searchResults = await _searchClient.SearchAsync<ResumeDocument>("*", searchOptions);

            var documents = new List<ResumeDocument>();
            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                documents.Add(result.Document);
            }

            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search");
            throw;
        }
    }

    public async Task IndexDocumentAsync(ResumeDocument document)
    {
        try
        {
            var batch = IndexDocumentsBatch.Upload(new[] { document });
            await _searchClient.IndexDocumentsAsync(batch);

            _logger.LogInformation("Successfully indexed document: {DocumentId}", document.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document: {DocumentId}", document.Id);
            throw;
        }
    }
}
