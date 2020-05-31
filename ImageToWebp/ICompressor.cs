using System;

namespace ImageWebp.ImageToWebp
{
    public interface ICompressor
    {
         void Compress(IServiceProvider serviceProvider, string srcFile, string dstFile);
    }
}