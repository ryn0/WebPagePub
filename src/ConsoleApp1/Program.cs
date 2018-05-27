using ImageMagick;
using System;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = @"C:\temp\";

            Console.WriteLine($"Optimizing images in: {directory}");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Converting .pngs to .jpgs...");

            string[] filePaths = Directory.GetFiles(directory, "*.png",
                                         SearchOption.TopDirectoryOnly);

            foreach (var file in filePaths)
            {
                using (MagickImage image = new MagickImage(file))
                {
                    var newFileLocation = file.Remove(file.Length - 4, 4);
                    newFileLocation = $"{newFileLocation}.jpg";

                    image.Write(newFileLocation);
                    Console.Write($"{newFileLocation}... ");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Compressing images jpgs...");

            filePaths = Directory.GetFiles(directory, "*.jpg",
                                        SearchOption.TopDirectoryOnly);

            foreach (var file in filePaths)
            {
                var fileInfo = new FileInfo(file);

                Console.Write(file);
                Console.Write(" Bytes before: " + fileInfo.Length);

                ImageOptimizer optimizer = new ImageOptimizer();
                optimizer.LosslessCompress(fileInfo);

                fileInfo.Refresh();
                Console.Write(" Bytes after:  " + fileInfo.Length);
            }

            Console.WriteLine();
            Console.WriteLine("Done.");

            Console.Read();
        }
    }
}
