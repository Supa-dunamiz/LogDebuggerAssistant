using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LogDebuggerAssistant.Utilities.Models
{
    public class EmbeddingService
    {
        private readonly HttpClient _http;
        public EmbeddingService(HttpClient http) => _http = http;

        public async Task<float[]> EmbedAsync(string content)
        {
            var response = await _http.PostAsJsonAsync("http://localhost:11434/api/embeddings", new
            {
                model = "nomic-embed-text",
                prompt = content
            });

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
            return result?.embedding ?? [];
        }

        private class EmbeddingResponse
        {
            public float[] embedding { get; set; }
        }
    }
}
