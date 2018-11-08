using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class AssetMatrixSingleton
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string csrf { get; set; }

    private static AssetMatrixSingleton __instance;

    public static AssetMatrixSingleton Instance
    {
        get
        {
            if (__instance == null)
            {
                __instance = new AssetMatrixSingleton();
            }

            return __instance;
        }
    }

    public AssetMatrixSingleton()
    {

    }

    public async Task<string> GetAsync(Uri uri)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            return await reader.ReadToEndAsync();
        }
    }
}


