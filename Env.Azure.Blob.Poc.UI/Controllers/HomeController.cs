using Azure.Storage.Blobs;
using Env.Azure.Blob.Poc.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Env.Azure.Blob.Poc.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string ContainerName = "myfiles";
        public const string SuccessMessageKey = "SuccessMessage";
        public const string ErrorMessageKey = "ErrorMessage";
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;


        public HomeController(ILogger<HomeController> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<IActionResult> Index()
        {
            var blobs = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }
            return View(blobs);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(file.FileName);
                await blobClient.UploadAsync(file.OpenReadStream(), true);
                TempData[SuccessMessageKey] = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData[ErrorMessageKey] = "An error occurred while uploading the file: " + ex.Message;
                return View();
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;
                var contentType = blobClient.GetProperties().Value.ContentType;
                return File(memoryStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData[ErrorMessageKey] = "An error occurred while downloading the file: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                TempData[SuccessMessageKey] = "File deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData[ErrorMessageKey] = "An error occurred while deleting the file: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
