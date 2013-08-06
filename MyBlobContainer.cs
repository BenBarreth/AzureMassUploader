using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AzureMassUploader
{

	internal class MyBlobContainer
	{
		public MyBlobContainer() { }

		internal CloudBlobContainer GetBlobContainerByName(string containerName)
		{
			containerName = MakeSafeAzureContainerName(containerName);

			//Create service client for credentialed access to the Blob service.
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString);
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

			//Get a reference to a container, which may or may not exist.
			CloudBlobContainer container = blobClient.GetContainerReference(containerName);
			return container;
		}

		internal CloudBlobContainer CreateBlobContainerIfNotExists(string containerName)
		{
			CloudBlobContainer container = GetBlobContainerByName(containerName);
			container.CreateIfNotExists();
			container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
			Console.WriteLine("Container created with name: " + container.Name);

			return container;
		}

		public static string MakeSafeFileName(string unsafeName)
		{
			unsafeName = HttpUtility.HtmlDecode(unsafeName);

			// swap spaces with underscore
			unsafeName = Regex.Replace(unsafeName, @"[\s]", "-");

			// strip all non-alpha chars 
			unsafeName = Regex.Replace(unsafeName, @"[^A-Za-z0-9-]", "");

			return unsafeName;
		}

		public static string MakeSafeAzureContainerName(string unsafeName)
		{
			return MakeSafeFileName(unsafeName).TrimStart('-').ToLower();
		}

		public string GetImageContentType(string fileExtension)
		{
			string contentType = "";

			switch (fileExtension.ToLower())
			{
				case ".png":
					contentType = "image/png";
					break;
				case ".gif":
					contentType = "image/gif";
					break;
				case ".jpg":
					contentType = "image/jpg";
					break;
				case ".jpeg":
					contentType = "image/jpeg";
					break;
				case ".bmp":
					contentType = "image/bmp";
					break;
				case ".tif":
				case ".tiff":
					contentType = "image/tiff";
					break;
				default:
					Console.WriteLine("This file extension is not a jpg, jpeg, png or gif: " + contentType);
					break;
			}
			Console.WriteLine("ContentType is set to: " + contentType);
			return contentType;
		}
	}
}
