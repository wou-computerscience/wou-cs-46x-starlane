using System;
using System.Linq;
using iCollections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using iCollections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace iCollections.Controllers
{
    public class UploadPhotoController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICollectionsDbContext _collectionsDbContext;

        public UploadPhotoController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ICollectionsDbContext collectionsDbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _collectionsDbContext = collectionsDbContext;
        }

        // Users not logged in who try to upload photos will be redirected to the login page.
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        private int GetICollectionUserID(string id)
        {
            var user = _collectionsDbContext.IcollectionUsers.First(i => i.AspnetIdentityId == id);
            int numericUserId = user.Id;
            return numericUserId;
        }

        private bool isProperImage(string type)
        {
            return type == "image/jpeg" || type == "image/png" || type == "image/gif";
        }

        [HttpPost]
        public IActionResult UploadImage(string customName)
        {
            string nastyStringId = _userManager.GetUserId(User);
            int userId = GetICollectionUserID(nastyStringId);

            try
            {
                foreach (var file in Request.Form.Files)
                {
                    if (isProperImage(file.ContentType))
                    {
                        Photo photo = new Photo();
                        photo.Name = (String.IsNullOrEmpty(customName)) ? file.FileName : customName;
                        MemoryStream ms = new MemoryStream();
                        file.CopyTo(ms);
                        photo.Data = ms.ToArray();
                        photo.DateUploaded = DateTime.Now;
                        photo.UserId = userId;
                        ms.Close();
                        ms.Dispose();
                        _collectionsDbContext.Photos.Add(photo);
                    }
                }
                _collectionsDbContext.SaveChanges();

                return RedirectToAction("Success");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            // remove extension
            var extension = Path.GetExtension("monument.jpg").Replace(".", "");

            // get image by name
            Photo img = _collectionsDbContext.Photos.Where(p => p.Name == "monument.jpg").First();

            // convert bytes to string
            string imageBase64Data = Convert.ToBase64String(img.Data);

            // add extra info to string
            string imageDataURL = string.Format("data:image/{0};base64,{1}", extension, imageBase64Data);
            ViewBag.ImageTitle = img.Name;

            // putting it all together
            ViewBag.ImageDataUrl = imageDataURL;
            return View("Success");
        }
    }
}
