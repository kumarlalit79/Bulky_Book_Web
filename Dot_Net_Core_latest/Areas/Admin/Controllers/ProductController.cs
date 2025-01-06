using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.IO;

namespace Dot_Net_Core_latest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitofwork , IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties:"Category").ToList();
            return View(objProductList);
        }
        public IActionResult Upsert(int? id) // id ho bhi sakti hai or nahi bhi
        {
            // for dropdown
            IEnumerable<SelectListItem> CategoryList = _unitofwork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });

            ProductVM productVM = new()
            {
                CategoryList = CategoryList,// upper ko caterogyList banai hai IENum mai ye wo hai
                Product = new Product()
            };

            if (id == null || id == 0) // iska matlb ye hai ki ye create ke liye hai
            {
                return View(productVM);
            }
            else
            {
                // means it is update functionality
                productVM.Product = _unitofwork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj , IFormFile? file)// image ke liye IFormFile use kar re
        {
            if (ModelState.IsValid) 
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;// is line se hame wwwroot folder ka path mil jayega.

                if (file != null) // check krre ki file h ya nahi, agr file h to hi upload krenge.
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);// ye guid hamari file ko ek random name de dega and file ka extension bhi get kar liya yaha se hi.

                    // so abhi ham wwwroot path pe hai, waha se images ke andr product path pe navigate karna hai
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    // edit 
                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl)) // agr image url present hai
                    {
                        // we have to delete the old image
                        // old image ka path nikaal lo
                        var oldImagePath = Path.Combine(wwwRootPath,obj.Product.ImageUrl.Trim('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }// delete karne ke baad neech new image ka code phele ka likha hai.
                    

                    // now saving that image
                    using (var fileStream = new FileStream(Path.Combine(productPath , fileName), FileMode.Create))// ye meri file ka complete path ya fir kahi ki complete url dena hoga.
                    {
                        file.CopyTo(fileStream); // new location pe file ko copy kar dega.
                    }

                    // ab is image ko productVM mai bhi to store karna hai.
                    obj.Product.ImageUrl = @"images\product\" + fileName;

                }
                if (obj.Product.Id == 0)
                {
                    _unitofwork.Product.Add(obj.Product);
                }
                else
                {
                    _unitofwork.Product.Update(obj.Product);
                }
                _unitofwork.Save();// ye hamara kisi repository ke andar nahi hai so dont use Product after _unitofwork
                TempData["Success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product? ProductFromDb = _unitofwork.Product.Get(u => u.Id == id);
        //    if (ProductFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(ProductFromDb);
        //}
        //[HttpPost , ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Product? obj = _unitofwork.Product.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitofwork.Product.Remove(obj);
        //    _unitofwork.Save();
        //    TempData["Success"] = "Product deleted successfully";
        //    return RedirectToAction("Index");
        //}

        #region api Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data= objProductList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productToBeDeleted = _unitofwork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // we have to delete image as well
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitofwork.Product.Remove(productToBeDeleted);
            _unitofwork.Save();

            return Json(new { success = true, message = "Deleted successfully" });
        }

        #endregion 

    }
}
