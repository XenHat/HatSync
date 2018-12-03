// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public static partial class NetworkUtilities
{// From AaronLuna @ Github
    public static async Task<IPAddress> GetPublicIPv4AddressAsync()
    {
        var urlContent =
          await GetUrlContentAsStringAsync("http://ipv4.icanhazip.com/").ConfigureAwait(false);

        return ParseSingleIPv4Address(urlContent);
    }
    public static async Task<IPAddress> GetPublicIPv6AddressAsync()
    {
        var urlContent =
          await GetUrlContentAsStringAsync("http://ipv6.icanhazip.com/").ConfigureAwait(false);

        return ParseSingleIPv6Address(urlContent);
    }

    private static IPAddress ParseSingleIPv4Address(string urlContent)
    {
        bool success = IPAddress.TryParse(urlContent, out IPAddress address);
        if (success)
        {
            switch (address.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetwork:
                    return address;
                default:
                    return null;
            }
        }
        return null;
    }

    private static IPAddress ParseSingleIPv6Address(string urlContent)
    {
        bool success = IPAddress.TryParse(urlContent, out IPAddress address);
        if (success)
        {
            switch (address.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetworkV6:
                    return address;
                default:
                    return null;
            }
        }
        return null;
    }

    public static async Task<string> GetUrlContentAsStringAsync(string url)
    {
        var urlContent = string.Empty;
        try
        {
            using (var httpClient = new HttpClient())
            using (var httpResonse = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                urlContent =
                  await httpResonse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
        catch (HttpRequestException ex)
        {
            // Handle exception
            HatSync.Log.WriteLine(ex.ToString());
        }

        return urlContent;
    }
}
