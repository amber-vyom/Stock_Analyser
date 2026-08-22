using System.IO.Compression;

namespace Stock_Analysis.Engine.NSE;

public class ExtractZip
{
    public string Extract(string zipFile, string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        ZipFile.ExtractToDirectory(
            zipFile,
            outputFolder,
            true);

        return outputFolder;
    }
}