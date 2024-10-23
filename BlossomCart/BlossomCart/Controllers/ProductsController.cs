using Microsoft.AspNetCore.Mvc;
using BlossomCart.Models;
using BlossomCart.ViewModels;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


using Microsoft.AspNetCore.Authorization;


using System.Linq;
using System.Diagnostics;


namespace BlossomCart.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ProductsController : Controller
	{
		private readonly BlossomCartsContext db;
		public ProductsController(BlossomCartsContext _db)
		{
			db = _db;
		}
		
       
        public IActionResult Create()
		{
			ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Bouquet prob, IFormFile file)
		{

			// Process the file

			var Imagename = DateTime.Now.ToString("yymmddhhmmss");
			Imagename += Path.GetFileName(file.FileName);
			string Imagepath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Images");
			var Imagevalue = Path.Combine(Imagepath, Imagename);
			using (var stream = new FileStream(Imagevalue, FileMode.Create))
			{
				file.CopyTo(stream);
			}

			var dbimage = Path.Combine("/Images", Imagename);
			prob.Image = dbimage;


			db.Bouquets.Add(prob);
			db.SaveChanges();


			ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
			TempData["ProductInsert"] = "Product Inserted Successfully..!";
			return RedirectToAction("Index", "Products");
		}

		public IActionResult Edit(int id)
		{

			ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
			var updat = db.Bouquets.Find(id);
			return View(updat);

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Bouquet edit, IFormFile file, string oldimage)
		{
			var dbimage = "";
			if (file != null && file.Length > 0)
			{
				var Imagename = DateTime.Now.ToString("yymmddhhmmss");
				Imagename += Path.GetFileName(file.FileName);
				string Imagepath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Images");
				var Imagevalue = Path.Combine(Imagepath, Imagename);
				using (var stream = new FileStream(Imagevalue, FileMode.Create))
				{
					file.CopyTo(stream);
				}

				dbimage = Path.Combine("/Images", Imagename);
				edit.Image = dbimage;

			
				db.Bouquets.Update(edit);
				db.SaveChanges();
			}
			else
			{
				edit.Image = oldimage;
				db.Bouquets.Update(edit);
				db.SaveChanges();
			}
			ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            TempData["ProductInsert"] = "Product Updated Successfully..!";

            return RedirectToAction("Index", "Products");

		}
		public IActionResult DeleteProduct(int id)
		{
			ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
			var bouquetToDelete = db.Bouquets.Find(id);
			if (bouquetToDelete == null)
			{
				return NotFound();
			}
			return View(bouquetToDelete);

		}
		[HttpPost, ActionName("DeleteProduct")]
		public IActionResult Delete(int id)
		{
			var bouquetToDelete = db.Bouquets.Find(id);
			if (bouquetToDelete == null)
			{
				return NotFound();
			}

			// Update the status instead of actually deleting the record
			bouquetToDelete.Status = 2;
			db.Bouquets.Update(bouquetToDelete);

			try
			{
				db.SaveChanges();
			}
			catch (Exception ex)
			{
				// Log the exception or handle it as needed
				// return Content("An error occurred while deleting the bouquet.");
				return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
			}
            TempData["ProductDelete"] = "Move To Trash..!";

            return RedirectToAction("Index", "Products");
		}
		public class ProductRepository
		{
			private readonly BlossomCartsContext context;

			public ProductRepository(BlossomCartsContext _context)
			{
				context = _context;
			}

			// Method to get filtered and paginated products
			public IEnumerable<Bouquet> GetFilteredProducts(string query, int page, int pageSize)
			{
				var filteredProducts = context.Bouquets
					.Where(p => p.BouquetName.Contains(query) || p.BouquetDescription.Contains(query));

				return filteredProducts
					.Where(p=>p.Status == 1)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Include(p => p.Category)
					.ToList();
			}

			// Method to get total count of filtered products
			public int GetTotalFilteredProducts(string query)
			{
				return context.Bouquets
					.Where(p => p.BouquetName.Contains(query) || p.BouquetDescription.Contains(query) && p.Status  == 1)
					.Count();
			}
		}

		public ActionResult Index(string query = "", int page = 1)
		{
			int pageSize = 6;
			var repository = new ProductRepository(new BlossomCartsContext());

			// Get filtered products based on the search query
			var products = repository.GetFilteredProducts(query, page, pageSize);

			// Get total count of products that match the search query
			var totalProducts = repository.GetTotalFilteredProducts(query);

			// Create pager object for pagination
			var pager = new Pager
			{
				TotalItems = totalProducts,
				PageSize = pageSize,
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize)
			};

			// Create the view model with products and pager
			var viewModel = new CategoryIsotopeViewModel
			{
				Products = products,
				Pager = pager,
				SearchQuery = query // Add search query to view model if needed
			};

			return View(viewModel);
		}

		public ActionResult Search(string query = "", int page = 1)
		{
			// Redirect to the Index action with search query and page number
			return RedirectToAction("Index", new { query, page });
		}

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
		}




	}
}

