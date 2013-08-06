using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

namespace AzureMassUploader
{
	class Program
	{
		static void Main(string[] args)
		{
			string locationOfFolderToUpload = ConfigurationManager.AppSettings.Get("ImageFolderLocation"); 
			bool PauseAfterEachFile = bool.Parse(ConfigurationManager.AppSettings.Get("PauseAfterEachFile"));
			string StartUploadAtCharacter = ConfigurationManager.AppSettings.Get("StartUploadAtFilesBeginningWithLetter");
			bool UploadStarted = false; 

			Console.WriteLine("Press any key to proceed.");
			Console.ReadLine();

			foreach (string fullLocalFilename in System.IO.Directory.GetFiles(locationOfFolderToUpload, "*.*", System.IO.SearchOption.AllDirectories))
			{
				try
				{
					string filenameWithSlashes = fullLocalFilename.Replace(locationOfFolderToUpload, "").Replace("\\", "/").TrimStart('/');
					string containerName = filenameWithSlashes.Substring(0, filenameWithSlashes.IndexOf("/"));
					if (containerName.Length < 3) containerName = containerName + "1";

					filenameWithSlashes = filenameWithSlashes.Substring(filenameWithSlashes.IndexOf('/')).TrimStart('/').Replace("[","").Replace("]","");

					if (!containerName.ToLower().StartsWith(StartUploadAtCharacter.ToLower()) && !UploadStarted) continue;

					UploadStarted = true;

					Console.WriteLine("Container name: " + containerName);
					// if container doesn't exist, create it
					MyBlobContainer myBlob = new MyBlobContainer();
					CloudBlobContainer container = myBlob.CreateBlobContainerIfNotExists(containerName);

					Console.WriteLine("Attempting to upload file: " + filenameWithSlashes);
					//Get a reference to a blob, which may or may not exist.
					CloudBlockBlob blob = container.GetBlockBlobReference(filenameWithSlashes);
					blob.Properties.ContentType = myBlob.GetImageContentType(filenameWithSlashes.Substring(filenameWithSlashes.LastIndexOf('.')));
					
					using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fullLocalFilename)))
					{
						blob.UploadFromStream(ms);
					}

					Console.WriteLine("File Uploaded: " + filenameWithSlashes);
					Console.WriteLine();

					if (PauseAfterEachFile)
					{
						Console.WriteLine("Press any key to proceed.");
						Console.ReadLine();
					}
				}
				catch (Exception ex)
				{
					string errorMsg = "fullLocalFilename: " + fullLocalFilename + "\n ex.Message: " + ex.Message + "  \n ex.StackTrace: " + ex.StackTrace + "\n fullLocalFilename: " + fullLocalFilename;
					System.IO.File.AppendAllText("ErrorLog.txt", errorMsg); 
					Console.WriteLine(errorMsg);
					Console.ReadLine();
				}

			}
		}

	}

}
