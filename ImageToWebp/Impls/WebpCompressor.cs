using System;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Logging;
using WebPWrapper;
using WebPWrapper.Encoder;

namespace ImageWebp.ImageToWebp.Impls
{
    public class WebpCompressor : ICompressor
    {
        public void Compress(IServiceProvider serviceProvider, string srcFile, string dstFile)
        {
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            var logger = loggerFactory.CreateLogger("ImageToWebp");

            string tempfile = null;
            var path = Path.Combine(Path.GetFullPath("."), "webp");
            //根据当前系统平台下载google压缩工具
            if (Directory.Exists(path) == false)
            {
                logger.LogInformation("begin WebPExecuteDownloader.Download");
                WebPExecuteDownloader.Download();
                logger.LogInformation("end WebPExecuteDownloader.Download");
            }

            var builder = new WebPEncoderBuilder();

            var encoder = builder
                .AlphaConfig(x => x // 透明处理
                    .TransparentProcess(
                        TransparentProcesses.Exact
                    )
                )
                .Build();
            var ext = Path.GetExtension(srcFile);
            if (string.Equals(ext, ".gif", StringComparison.CurrentCultureIgnoreCase))
            {
                //gif需要更改工具名称
                var type = encoder.GetType();
                var fieldInfo = type.GetField("_executeFilePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var _executeFilePath = (string)fieldInfo.GetValue(encoder).ToString().Replace("cwebp", "gif2webp");
                fieldInfo.SetValue(encoder, _executeFilePath);
                fieldInfo = type.GetField("_arguments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                fieldInfo.SetValue(encoder, "-q 70");
            }


            while (true)
            {
                try
                {
                    logger.LogInformation("build webp for " + dstFile);
                    using (var outputFile = File.Open(dstFile, FileMode.Create))
                    using (var inputFile = File.Open(srcFile, FileMode.Open))
                    {
                        encoder.Encode(inputFile, outputFile);
                    }
                    if (tempfile != null)
                    {
                        try
                        {
                            File.Delete(tempfile);
                        }
                        catch { }
                    }
                    break;
                }
                catch (Exception ex)
                {

                    logger.LogError(ex.ToString());

                    if (tempfile != null)
                    {
                        try
                        {
                            File.Delete(tempfile);
                        }
                        catch { }
                        tempfile = null;
                    }

                    if (ex.Message.Contains("16383"))
                    {
                        //图像太大，把分辨率转小
                        using (var bitmap = Bitmap.FromFile(srcFile))
                        {
                            int newwidth = bitmap.Size.Width;
                            int newheight = bitmap.Size.Height;
                            if (newwidth > 16383)
                            {
                                newheight = (int)(newheight * (16383.0 / newwidth));
                                newwidth = 16383;
                            }
                            if (newheight > 16383)
                            {
                                newwidth = (int)(newwidth * (16383.0 / newheight));
                                newheight = 16383;
                            }
                            using (var newbitmap = new Bitmap(newwidth, newheight, bitmap.PixelFormat))
                            {
                                using (Graphics g = Graphics.FromImage(newbitmap))
                                {
                                    g.DrawImage(bitmap, new Rectangle(0, 0, newwidth, newheight), new Rectangle(Point.Empty, bitmap.Size), GraphicsUnit.Pixel);
                                }
                                var rawformat = bitmap.RawFormat;
                                bitmap.Dispose();
                                tempfile = dstFile + ".tmp.png";
                                while (System.IO.File.Exists(tempfile))
                                {
                                    tempfile = dstFile + DateTime.Now.ToString("yyyyMMddHHmmss") + ".tmp.png";
                                }
                                srcFile = tempfile;
                                newbitmap.Save(tempfile, rawformat);
                            }

                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }

        }
    }
}