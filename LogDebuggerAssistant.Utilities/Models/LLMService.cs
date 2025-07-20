using DevelopmentCommons;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogDebuggerAssistant.Utilities.Models
{
    public class LLMService
    {
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; }
        public LLMService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<string> AnalyzeAsync(string context)
        {
            try
            {

                string result = string.Empty;

                var payload = new
                {
                    model = "llama3",
                    prompt = $"Given this context: {context}\n\n" +
                             "First identify the root cause, then provide a suggested fix.\n" +
                             "Format your response exactly as:\n" +
                             "Root Cause: [clear explanation here]\n" +
                             "Suggested Fix: [specific steps here]\n" +
                             "Keep responses under 50 words each for the Root cause and Suggested fix.",
                    stream = true,
                    options = new { temperature = 0.7, repeat_penalty = 1.1 }
                };


                var client = HttpClientFactory.CreateClient("Ollama");
                using var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
                {
                    Content = JsonContent.Create(payload)
                };

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();
                result = await ProcessResponseStream(stream);
                CLogger.Log($"Response result: {result}");
                return result;
            }
            catch(Exception ex)
            {
                CLogger.Log($"Exception thrown: {ex.ToString()}");
                return "No errors found";
            }
        }
        private async Task<string> ProcessResponseStream(Stream stream)
        {
            var responseBuilder = new StringBuilder();
            using var reader = new StreamReader(stream);
            CLogger.Log($"Response received {DateTime.Now}");
           // Read the entire content at once to minimize async overhead
           var content = await reader.ReadToEndAsync();

            // Process lines from memory instead of streaming
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Skip empty lines (though Split with RemoveEmptyEntries should handle this)
                if (line.Length == 0) continue;

                // Use JsonDocument instead of full deserialization for better performance
                using JsonDocument document = JsonDocument.Parse(line);
                if (document.RootElement.TryGetProperty("response", out JsonElement responseElement) &&
                    responseElement.ValueKind == JsonValueKind.String)
                {
                    responseBuilder.Append(responseElement.GetString());
                }
            }

            return responseBuilder.ToString();
        }

        private class LLMResponse
        {
            public string response { get; set; }
        }
    }
}
