using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IconChop
{
    internal static class OpenAiImageClient
    {
        public static async Task<Bitmap> GenerateAsync(
            AppSettings settings,
            string prompt,
            IReadOnlyList<Bitmap> referenceImages,
            CancellationToken cancellationToken)
        {
            var key = settings.OpenAiApiKey?.Trim();
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("OpenAI API key is not set. Use Tools → Settings to add your key.");

            var baseUrl = (settings.OpenAiApiBaseUrl ?? "https://api.openai.com/v1").TrimEnd('/');
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            if (referenceImages.Count == 0)
                return await GenerateTextOnlyAsync(http, baseUrl, settings, prompt, cancellationToken).ConfigureAwait(false);

            return await GenerateWithReferencesAsync(http, baseUrl, settings, prompt, referenceImages, cancellationToken)
                .ConfigureAwait(false);
        }

        private static async Task<Bitmap> GenerateTextOnlyAsync(
            HttpClient http,
            string baseUrl,
            AppSettings settings,
            string prompt,
            CancellationToken cancellationToken)
        {
            var model = (settings.OpenAiImageModel ?? "dall-e-3").Trim();
            var size = NormalizeSizeForModel(settings.OpenAiImageSize ?? "1024x1024", model);
            var quality = (settings.OpenAiImageQuality ?? "standard").ToLowerInvariant();

            await using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                writer.WriteStartObject();
                writer.WriteString("model", model);
                writer.WriteString("prompt", prompt);
                writer.WriteNumber("n", 1);
                writer.WriteString("size", size);

                if (model.StartsWith("gpt-image", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteString("quality", MapQualityForGptImage(quality));
                    writer.WriteString("output_format", "png");
                }
                else if (string.Equals(model, "dall-e-3", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteString("quality", quality == "hd" ? "hd" : "standard");
                    writer.WriteString("response_format", "b64_json");
                }
                else
                {
                    // dall-e-2
                    writer.WriteString("response_format", "b64_json");
                }

                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(ms.ToArray());
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await http
                .PostAsync($"{baseUrl}/images/generations", content, cancellationToken)
                .ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(ParseErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {body}");

            return await DecodeFirstImageFromJsonAsync(body, http, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Bitmap> GenerateWithReferencesAsync(
            HttpClient http,
            string baseUrl,
            AppSettings settings,
            string prompt,
            IReadOnlyList<Bitmap> referenceImages,
            CancellationToken cancellationToken)
        {
            var configured = (settings.OpenAiImageModel ?? "gpt-image-1").Trim();
            var model = configured.StartsWith("gpt-image", StringComparison.OrdinalIgnoreCase)
                ? configured
                : "gpt-image-1.5";

            var size = NormalizeSizeForGptEdits(settings.OpenAiImageSize ?? "1024x1024");
            var quality = MapQualityForGptImage((settings.OpenAiImageQuality ?? "standard").ToLowerInvariant());

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model), "model");
            form.Add(new StringContent(prompt), "prompt");
            form.Add(new StringContent(size), "size");
            form.Add(new StringContent(quality), "quality");
            form.Add(new StringContent("png"), "output_format");

            for (var i = 0; i < referenceImages.Count; i++)
            {
                var pngMs = new MemoryStream();
                referenceImages[i].Save(pngMs, ImageFormat.Png);
                pngMs.Position = 0;
                var streamContent = new StreamContent(pngMs);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                form.Add(streamContent, "image[]", $"reference_{i + 1}.png");
            }

            using var response = await http
                .PostAsync($"{baseUrl}/images/edits", form, cancellationToken)
                .ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(ParseErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {body}");

            return await DecodeFirstImageFromJsonAsync(body, http, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Bitmap> DecodeFirstImageFromJsonAsync(string body, HttpClient http, CancellationToken cancellationToken)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            {
                var first = data[0];
                if (first.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
                {
                    var bytes = Convert.FromBase64String(b64.GetString()!);
                    using (var ms = new MemoryStream(bytes, writable: false))
                        return new Bitmap(ms);
                }

                if (first.TryGetProperty("url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                {
                    var url = urlEl.GetString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        var bytes = await http.GetByteArrayAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
                        using (var ms = new MemoryStream(bytes, writable: false))
                            return new Bitmap(ms);
                    }
                }
            }

            throw new InvalidOperationException("OpenAI response did not include image data (expected data[0].b64_json or url).");
        }

        private static string? ParseErrorMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    if (err.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        return msg.GetString();
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static string NormalizeSizeForModel(string size, string model)
        {
            if (model.StartsWith("gpt-image", StringComparison.OrdinalIgnoreCase))
                return NormalizeSizeForGptGenerations(size);
            if (string.Equals(model, "dall-e-3", StringComparison.OrdinalIgnoreCase))
                return size is "1024x1024" or "1792x1024" or "1024x1792" ? size : "1024x1024";
            // dall-e-2
            return size is "256x256" or "512x512" or "1024x1024" ? size : "1024x1024";
        }

        private static string NormalizeSizeForGptGenerations(string size)
        {
            return size switch
            {
                "1024x1536" or "1536x1024" or "1024x1024" => size,
                "1792x1024" => "1536x1024",
                "1024x1792" => "1024x1536",
                _ => "1024x1024"
            };
        }

        private static string NormalizeSizeForGptEdits(string size)
        {
            return size switch
            {
                "1024x1536" or "1536x1024" or "1024x1024" => size,
                "1792x1024" => "1536x1024",
                "1024x1792" => "1024x1536",
                _ => "1024x1024"
            };
        }

        private static string MapQualityForGptImage(string qualityOrLegacy)
        {
            return qualityOrLegacy switch
            {
                "hd" or "high" => "high",
                "standard" or "medium" => "medium",
                "low" => "low",
                _ => "auto"
            };
        }
    }
}
