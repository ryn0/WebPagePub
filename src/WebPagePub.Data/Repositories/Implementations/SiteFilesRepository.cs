using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.BaseClasses;
using WebPagePub.Data.Models.AzureStorage.Blob;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SiteFilesRepository : BaseBlobFiles, ISiteFilesRepository
    {
        private const string FolderFileName = "_.txt";
        private const string ContainerName = "sitecontent";
        private readonly string connectionString;
        private readonly CloudStorageAccount storageAccount;

        public SiteFilesRepository(string connectionString)
        {
            this.connectionString = connectionString;

            if (string.IsNullOrWhiteSpace(this.connectionString))
            {
                return;
            }

            this.storageAccount = CloudStorageAccount.Parse(this.connectionString);

            var blobClient = this.storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(ContainerName);

            // todo: run async elsewhere
            Task.Run(async () =>
            {
                await this.CreateIfNotExists(container);
                await this.SetCorsAsync(blobClient);
            });
        }

        public SiteFileDirectory ListFiles(string prefix = null)
        {
            try
            {
                var directory = new SiteFileDirectory();
                var blobClient = this.storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(ContainerName);

                if (prefix != null && prefix.StartsWith("/"))
                {
                    prefix = prefix.Remove(0, 1);
                }

                foreach (IListBlobItem item in container.ListBlobsSegmentedAsync(
                                                            prefix, false, BlobListingDetails.None, int.MaxValue, null, null, null).Result.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        var blob = (CloudBlockBlob)item;
                        var file = blob.Uri.ToString();

                        if (!file.EndsWith(FolderFileName))
                        {
                            directory.FileItems.Add(new SiteFileItem
                            {
                                FilePath = blob.Uri.ToString(),
                                IsFolder = false
                            });
                        }
                        else if (item.Parent.Prefix != prefix)
                        {
                            this.AddDirectory(directory, item.Parent);
                        }
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        var pageBlob = (CloudPageBlob)item;

                        // todo: find out when this is used
                        throw new Exception("CloudPageBlob");
                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        this.AddDirectory(directory, item);
                    }
                }

                return directory;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteFileAsync(string blobPath)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
            {
                return;
            }

            try
            {
                var blobClient = this.storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(ContainerName);

                if (IsFolderPath(blobPath))
                {
                    blobPath = string.Format("{0}{1}", blobPath, FolderFileName);

                    if (blobPath.StartsWith("/"))
                    {
                        blobPath = blobPath.Remove(0, 1);
                    }
                }

                var endIndexOfCotainerName = blobPath.IndexOf(ContainerName) + ContainerName.Length;

                var path = blobPath.Substring(endIndexOfCotainerName, blobPath.Length - endIndexOfCotainerName).TrimStart('/');

                var blockBlob = await container.GetBlobReferenceFromServerAsync(path);

                await blockBlob.DeleteAsync();
            }
            catch (StorageException storEx)
            {
                if (storEx.ToString().Contains("The specified blob does not exist"))
                {
                    return;
                }
                else
                {
                    throw new Exception(storEx.Message, storEx);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteFolderAsync(string folderPath)
        {
            var allInDir = this.GetDirContents(folderPath);

            foreach (var item in allInDir)
            {
                await this.DeleteFileAsync(item.Uri.ToString());
            }
        }

        public async Task<Uri> UploadAsync(IFormFile file, string directory = null)
        {
            var memoryStream = new MemoryStream();

            try
            {
                await file.CopyToAsync(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var fileName = this.CleanFileName(file.FileName);
                return await this.UploadAsync(memoryStream, fileName, directory);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        public async Task<Uri> UploadAsync(Stream stream, string fileName, string directory = null)
        {
            try
            {
                fileName = this.CleanFileName(fileName);

                if (fileName == FolderFileName)
                {
                    return null;
                }

                var filePath = fileName;

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    filePath = directory + filePath;

                    if (filePath.StartsWith("/"))
                    {
                        filePath = filePath.Remove(0, 1);
                    }
                }

                var blobClient = this.storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(ContainerName);
                var blockBlob = container.GetBlockBlobReference(filePath);

                stream.Seek(0, SeekOrigin.Begin);

                await blockBlob.UploadFromStreamAsync(stream);
                var extension = fileName.GetFileExtensionLower();

                await this.SetPropertiesAsync(blockBlob, extension);

                return blockBlob.Uri;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                stream.Dispose();
            }
        }

        public async Task CreateFolderAsync(string folderPath, string directory = null)
        {
            var memoryStream = new MemoryStream();

            try
            {
                memoryStream.Seek(0, SeekOrigin.Begin);

                var tw = new StreamWriter(memoryStream);

                folderPath = folderPath.Replace("/", string.Empty);

                tw.WriteLine(folderPath);

                var blobClient = this.storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(ContainerName);
                var path = string.Format("{0}/{1}", folderPath, FolderFileName);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    path = directory + path;

                    if (path.StartsWith("/"))
                    {
                        path = path.Remove(0, 1);
                    }
                }

                var blockBlob = container.GetBlockBlobReference(path);

                if (await blockBlob.ExistsAsync())
                {
                    return;
                }

                await blockBlob.UploadFromStreamAsync(memoryStream);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        private static bool IsFolderPath(string blobPath)
        {
            return blobPath.EndsWith("/");
        }

        private List<IListBlobItem> GetDirContents(string prefix = null)
        {
            var directory = new SiteFileDirectory();
            var blobClient = this.storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(ContainerName);

            if (prefix != null && prefix.StartsWith("/"))
            {
                prefix = prefix.Remove(0, 1);
            }

            var allInDir = container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.None, int.MaxValue, null, null, null).Result.Results;

            return allInDir.ToList();
        }

        private async Task CreateIfNotExists(CloudBlobContainer container)
        {
            if (await container.CreateIfNotExistsAsync())
            {
                await this.SetPublicContainerPermissionsAsync(container);
            }
        }

        private void AddDirectory(SiteFileDirectory directory, IListBlobItem item)
        {
            var cloudBlobDirectory = (CloudBlobDirectory)item;
            var folderName = cloudBlobDirectory.Uri.ToString().Split('/')[cloudBlobDirectory.Uri.ToString().Split('/').Length - 2];
            var pathFromRoot = new Uri(cloudBlobDirectory.Uri.ToString()).LocalPath.Replace(string.Format("/{0}", ContainerName), string.Empty);

            directory.FileItems.Add(new SiteFileItem
            {
                FilePath = cloudBlobDirectory.Uri.ToString(),
                IsFolder = true,
                FolderName = folderName,
                FolderPathFromRoot = pathFromRoot
            });
        }

        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
