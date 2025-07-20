using LogDebuggerAssistant.Utilities.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Radzen.Blazor.Rendering;
using static System.Net.WebRequestMethods;
using System.Net.NetworkInformation;
using DevelopmentCommons;
using Microsoft.JSInterop;


namespace LogDebuggerAssistant.Components.Pages
{
    public class HomeBase : ComponentBase
    {
        protected List<ErrorAnalysisResult> Results;
        protected bool isProcessing = false;
        protected string processingStatus = "Initializing analysis...";
        protected IBrowserFile uploadedFile;
        private DotNetObjectReference<HomeBase>? objRef;

        [Inject] LogParserService Parser {  get; set; }
        [Inject] LLMService LLM {  get; set; }
        [Inject] IJSRuntime JS {  get; set; }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                objRef = DotNetObjectReference.Create(this);
                //await JS.InvokeVoidAsync("fileDropHelper.registerDropZone", "dropZone", objRef);
            }
        }
        protected async Task HandleUpload(InputFileChangeEventArgs e)
        {
            uploadedFile = e.File;
            isProcessing = true;
            Results = null;

            try
            {
                using var reader = new StreamReader(uploadedFile.OpenReadStream());
                processingStatus = "Reading file...";
                var content = await reader.ReadToEndAsync();

                processingStatus = "Parsing log entries...";
                var entries = Parser.ParseLog(content);
                StateHasChanged();
                Results = new List<ErrorAnalysisResult>();

                for (int i = 0; i < entries.Count; i++)
                {
                    processingStatus = $"Analyzing entry {i + 1} of {entries.Count}...";
                    StateHasChanged();
                    CLogger.Log(processingStatus);
                    await AnalyzeEntryAsync(entries[i]); // Ensures one-at-a-time execution
                }
            }
            finally
            {
                isProcessing = false;
                CLogger.Log("Log Analysis completed");
            }
        }

        private async Task AnalyzeEntryAsync(LogEntry entry)
        {
            string context = entry.Message + "\n" + entry.StackTrace;

            string answer = await LLM.AnalyzeAsync(context); // Only one call at a time

            var (rootCause, suggestedFix) = ParseLLMResponse(answer);

            Results.Add(new ErrorAnalysisResult
            {
                Log = entry,
                SuggestedFix = suggestedFix,
                RootCause = rootCause
            });
        }


        private (string RootCause, string SuggestedFix) ParseLLMResponse(string response)
        {
            const string rootCauseMarker = "Root Cause:";
            const string suggestedFixMarker = "Suggested Fix:";

            string rootCause = "";
            string suggestedFix = "";

            // Split response into lines and process
            using var reader = new StringReader(response);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(rootCauseMarker))
                {
                    rootCause = line.Substring(rootCauseMarker.Length).Trim();
                    // Continue reading multi-line root cause
                    while ((line = reader.ReadLine()) != null &&
                          !line.StartsWith(suggestedFixMarker))
                    {
                        rootCause += "\n" + line.Trim();
                    }
                }

                if (line != null && line.StartsWith(suggestedFixMarker))
                {
                    suggestedFix = line.Substring(suggestedFixMarker.Length).Trim();
                    // Continue reading multi-line fixes
                    while ((line = reader.ReadLine()) != null)
                    {
                        suggestedFix += "\n" + line.Trim();
                    }
                }
            }

            // Fallback if markers not found
            if (string.IsNullOrEmpty(rootCause))
            {
                rootCause = "Could not determine root cause";
                suggestedFix = response;
            }

            return (rootCause, suggestedFix);
        }
        protected string GetFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

        [JSInvokable]
        public async Task HandleDroppedFile(DroppedFile file)
        {
            isProcessing = true;
            Results = null;

            try
            {
                var stream = new MemoryStream(file.Content.ToArray());
                using var reader = new StreamReader(stream);
                processingStatus = "Reading file...";
                var content = await reader.ReadToEndAsync();

                processingStatus = "Parsing log entries...";
                var entries = Parser.ParseLog(content);
                StateHasChanged();
                Results = new List<ErrorAnalysisResult>();

                for (int i = 0; i < entries.Count; i++)
                {
                    processingStatus = $"Analyzing entry {i + 1} of {entries.Count}...";
                    StateHasChanged();
                    CLogger.Log(processingStatus);
                    await AnalyzeEntryAsync(entries[i]);
                }
            }
            finally
            {
                isProcessing = false;
                CLogger.Log("Log Analysis completed");
            }
        }

        public async ValueTask DisposeAsync()
        {
            objRef?.Dispose();
        }

        public class DroppedFile
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public long Size { get; set; }
            public List<byte> Content { get; set; }
        }
    }
}
