﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;

namespace PurplePen
{
    // Manages a PDF map file and converting it to a bitmap.
    class PdfMapFile
    {
        private string pdfFileName;
        private string pngFileName;
        private ConversionStatus status;
        private string conversionOutput;
        private Task conversionTask;

        private const int Resolution = 600; // Resolution in DPI
        
        public PdfMapFile(string pdfFileName)
        {
            this.pdfFileName = pdfFileName;
            this.status = ConversionStatus.NotStarted;
        }

        public event EventHandler ConversionCompleted;

        public string PdfFileName {
            get { return pdfFileName; }
        }

        public string PngFileName
        {
            get
            {
                Debug.Assert(Status == ConversionStatus.Success);
                return pngFileName;
            }
        }

        public bool SourceExists {
            get {
                return File.Exists(pdfFileName);
            }
        }

        public ConversionStatus Status
        {
            get
            {
                return status;
            }
        }

        public string ConversionOutput
        {
            get {
                return conversionOutput;
            }
        }

        // Try to begin conversion into bitmap. 
        public ConversionStatus BeginConversion()
        {
            if (!SourceExists) {
                conversionOutput = string.Format("File '{0}' does not exist.", pdfFileName);
                status = ConversionStatus.Failure;
                return status;
            }

            CleanCacheDirectory();
            string cacheFileName = GetCacheFileName(pdfFileName);

            if (File.Exists(cacheFileName)) {
                // Cached file still exists. Use it.
                conversionOutput = "";
                pngFileName = cacheFileName;
                status = ConversionStatus.Success;
                return status;
            }

            return BeginUncachedConversion(cacheFileName, Resolution);
        }

        // Try to begin conversion into bitmap. 
        public ConversionStatus BeginUncachedConversion(string fileName, int resolution)
        {
            TaskScheduler currentContextScheduler = SynchronizationContext.Current == null ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();

            status = ConversionStatus.Working;
            pngFileName = fileName;
            conversionTask = Task.Factory.StartNew(() => ConvertAndSaveImage(fileName, resolution));
            conversionTask.ContinueWith(ConversionComplete, CancellationToken.None, TaskContinuationOptions.None, currentContextScheduler);
            return status;
        }

        void ConvertAndSaveImage(string destinationFileName, int resolution)
        {
            int pageNumber = 0;

            using (PdfDocument document = PdfDocument.Load(pdfFileName)) {
                SizeF sizeInPoints = document.PageSizes[pageNumber];
                int widthInPixels = (int)Math.Round(sizeInPoints.Width * (float)resolution / 72F);
                int heightInPixels = (int)Math.Round(sizeInPoints.Height * (float)resolution / 72F);
                using (Image image = document.Render(pageNumber, widthInPixels, heightInPixels, resolution, resolution, true)) {
                    image.Save(destinationFileName, ImageFormat.Png);
                }
            }

            GC.Collect();
        }

        void ConversionComplete(Task task)
        {
            if (task.IsFaulted) {
                status = ConversionStatus.Failure;
                Exception e = conversionTask.Exception;
                while (e.InnerException != null)
                    e = e.InnerException;
                conversionOutput = e.Message;
            }
            else if (task.IsCanceled) {
                status = ConversionStatus.Failure;
                conversionOutput = "Cancelled";
            }
            else {
                status = ConversionStatus.Success;
                conversionOutput = "";
            }

            conversionTask = null;

            ConversionCompleted?.Invoke(this, EventArgs.Empty);
        }

        internal string GetCacheFileName(string path)
        {
            string cacheDirectory = GetCacheDirectory();

            return Path.Combine(cacheDirectory, CalculateSha1(path) + ".png");
        }

        private static string GetCacheDirectory()
        {
            string tempPath = Path.GetTempPath();
            string cacheDirectory = Path.Combine(tempPath, "PurplePen");
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);
            return cacheDirectory;
        }

        // Clean stale caches (over 6 months old).
        private static void CleanCacheDirectory()
        {
            DateTime oldDate = DateTime.Now.Subtract(TimeSpan.FromDays(180));
            string cacheDirectory = GetCacheDirectory();

            try {
                foreach (string filename in Directory.GetFiles(cacheDirectory, "*.png", SearchOption.TopDirectoryOnly)) {
                    FileInfo fileInfo = new FileInfo(filename);
                    if (fileInfo.Exists && fileInfo.LastWriteTime < oldDate) {
                        fileInfo.Delete();
                    }
                }
            } 
            catch {
                // Do nothing. Not a problem if we get an exception here.
            }
        }

        internal string CalculateSha1(string path)
        {
            var hashAlgorithm = System.Security.Cryptography.SHA1.Create();
            byte[] hash = hashAlgorithm.ComputeHash(File.ReadAllBytes(path));
            hash[0] ^= 0xe9;   // Change hash so different from previous (GhostScript)
            return Hexify(hash);
        }

        private string Hexify(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes) {
                builder.Append(b.ToString("X2"));
            }
            return builder.ToString();
        }

        public enum ConversionStatus
        {
            NotStarted, Success, Failure, Working
        }
    }
}
