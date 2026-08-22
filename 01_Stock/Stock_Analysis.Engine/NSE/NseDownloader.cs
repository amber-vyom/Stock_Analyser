namespace Stock_Analysis.Engine.NSE;

public class NseDownloader
{
    private readonly HttpClient _httpClient;

    public NseDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> DownloadAsync(
        string url,
        string filePath)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        byte[] data =
            await response.Content.ReadAsByteArrayAsync();

        string? directory =
            Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }
}