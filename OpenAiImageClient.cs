using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IconChop
{
    internal static class OpenAiImageClient
    {
        /// <summary>
        /// Sends extracted icon images to a vision model and returns a descriptive
        /// filename prefix for each one (no extension, filesystem-safe).
        /// </summary>
        public static async Task<List<string>> SuggestFilenamesAsync(
            AppSettings settings,
            IReadOnlyList<Bitmap> images,
            CancellationToken cancellationToken)
        {
            var key = settings.OpenAiTextApiKey?.Trim() is { Length: > 0 } tk
                ? tk
                : settings.OpenAiApiKey?.Trim();
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException(
                    "OpenAI API key is not set. Use Tools \u2192 Settings to add your key.");

            var baseUrl = (!string.IsNullOrWhiteSpace(settings.OpenAiTextBaseUrl)
                ? settings.OpenAiTextBaseUrl
                : settings.OpenAiApiBaseUrl ?? "https://api.openai.com/v1").TrimEnd('/');
            var model = (settings.OpenAiNamingModel ?? "gpt-4o-mini").Trim();
            if (string.IsNullOrEmpty(model)) model = "gpt-4o-mini";

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            await using var ms = new MemoryStream();
            using (var w = new Utf8JsonWriter(ms))
            {
                w.WriteStartObject();
                w.WriteString("model", model);
                w.WritePropertyName("messages");
                w.WriteStartArray();
                w.WriteStartObject();
                w.WriteString("role", "user");
                w.WritePropertyName("content");
                w.WriteStartArray();

                w.WriteStartObject();
                w.WriteString("type", "text");
                w.WriteString("text",
                    $"You are looking at {images.Count} small icon image(s) extracted from an icon sheet. " +
                    $"For each icon (in the order shown), suggest a concise descriptive filename " +
                    $"(lowercase, words separated by hyphens, no file extension, max 25 characters). " +
                    $"Names must be unique across the set. " +
                    $"Reply with exactly {images.Count} line(s), one filename per line, nothing else.");
                w.WriteEndObject();

                foreach (var img in images)
                {
                    var b64 = Convert.ToBase64String(BitmapToPngBytes(img));
                    w.WriteStartObject();
                    w.WriteString("type", "image_url");
                    w.WritePropertyName("image_url");
                    w.WriteStartObject();
                    w.WriteString("url", $"data:image/png;base64,{b64}");
                    w.WriteString("detail", "low");
                    w.WriteEndObject();
                    w.WriteEndObject();
                }

                w.WriteEndArray();  // content
                w.WriteEndObject(); // message
                w.WriteEndArray();  // messages
                w.WriteNumber("max_tokens", 500);
                w.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(ms.ToArray());
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await http
                .PostAsync($"{baseUrl}/chat/completions", content, cancellationToken)
                .ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    ParseErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {body}");

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            var names = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => SanitizeFilename(n.Trim()))
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            return EnsureUnique(names);
        }

        private static byte[] BitmapToPngBytes(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        private static string SanitizeFilename(string name)
        {
            name = Regex.Replace(name, @"^\d+[\.\)\-:\s]+", "");
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(
                    name.Where(c => Array.IndexOf(invalid, c) < 0).ToArray())
                .Trim(' ', '.', '-')
                .ToLowerInvariant();
            sanitized = Regex.Replace(sanitized, @"\s+", "-");
            sanitized = Regex.Replace(sanitized, @"-{2,}", "-");
            if (sanitized.Length > 30)
                sanitized = sanitized[..30].TrimEnd('-');
            return sanitized;
        }

        private static List<string> EnsureUnique(List<string> names)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                var candidate = string.IsNullOrEmpty(name) ? "icon" : name;
                var final = candidate;
                int suffix = 2;
                while (seen.Contains(final))
                {
                    final = $"{candidate}-{suffix}";
                    suffix++;
                }
                seen.Add(final);
                result.Add(final);
            }
            return result;
        }

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
