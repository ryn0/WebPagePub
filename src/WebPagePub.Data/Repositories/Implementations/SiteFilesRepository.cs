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
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models.AzureStorage.Blob;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SiteFilesRepository : BaseBlobFiles, ISiteFilesRepository
    {
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
            var container = blobClient.GetContainerReference(StringConstants.ContainerName);

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
                var container = blobClient.GetContainerReference(StringConstants.ContainerName);

                if (prefix != null && prefix.StartsWith("/"))
                {
                    prefix = prefix.Remove(0, 1);
                }

                foreach (IListBlobItem item in container.ListBlobsSegmentedAsync(
                                                            prefix,
                                                            false,
                                                            BlobListingDetails.None,
                                                            int.MaxValue,
                                                            null,
                                                            null,
                                                            null).Result.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        var blob = (CloudBlockBlob)item;
                        var file = blob.Uri.ToString();

                        if (!file.EndsWith(StringConstants.FolderFileName))
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
                        throw new Exception(nameof(CloudPageBlob));
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
                var container = blobClient.GetContainerReference(StringConstants.ContainerName);

                if (IsFolderPath(blobPath))
                {
                    blobPath = string.Format("{0}{1}", blobPath, StringConstants.FolderFileName);

                    if (blobPath.StartsWith("/"))
                    {
                        blobPath = blobPath.Remove(0, 1);
                    }
                }

                var endIndexOfCotainerName = blobPath.IndexOf(StringConstants.ContainerName) + StringConstants.ContainerName.Length;

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
                var fileName = FileNameUtilities.CleanFileName(file.FileName);
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
                fileName = FileNameUtilities.CleanFileName(fileName);

                if (fileName == StringConstants.FolderFileName)
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
                var container = blobClient.GetContainerReference(StringConstants.ContainerName);
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
                var container = blobClient.GetContainerReference(StringConstants.ContainerName);
                var path = string.Format("{0}/{1}", folderPath, StringConstants.FolderFileName);

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

        public async Task ChangeFileName(string currentFileName, string newFileName)
        {
            var blobClient = this.storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(StringConstants.ContainerName);
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blobCopy = container.GetBlockBlobReference(newFileName);
            if (!await blobCopy.ExistsAsync())
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(currentFileName);

                if (await blob.ExistsAsync())
                {
                    await blobCopy.StartCopyAsync(blob);
                    await blob.DeleteIfExistsAsync();
                }
            }
        }

        private static bool IsFolderPath(string blobPath)
        {
            return blobPath.EndsWith("/");
        }

        private List<IListBlobItem> GetDirContents(string prefix = null)
        {
            var blobClient = this.storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(StringConstants.ContainerName);

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
            var pathFromRoot = new Uri(cloudBlobDirectory.Uri.ToString()).LocalPath.Replace(string.Format("/{0}", StringConstants.ContainerName), string.Empty);

            directory.FileItems.Add(new SiteFileItem
            {
                FilePath = cloudBlobDirectory.Uri.ToString(),
                IsFolder = true,
                FolderName = folderName,
                FolderPathFromRoot = pathFromRoot
            });
        }
    }
}
