using System;
using System.IO;
using System.IO.Compression;

public static class StreamExtensions
{
    public static void CopyTo(this Stream input, Stream output)
    {
        byte[] buffer = new byte[16 * 1024];
        int bytesRead;
        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }
}