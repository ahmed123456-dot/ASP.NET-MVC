using BlossomCart.Models;

using BlossomCart.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Drawing.Printing;
using System.Linq;

using static BlossomCart.Controllers.ProductsController;



namespace BlossomCart.Controllers
{
	[Authorize]
	public class ShopController : Controller
	{
		private readonly BlossomCartsContext _context;


		public ShopController(BlossomCartsContext context)
		{
			_context = context;
		}
		protected override void Dispose(bool disposing)
		{
			_context.Dispose();
		}
		public ActionResult Search(int? id, string query)
		{
			var cat = _context.Categories.ToList();
			ViewBag.cat = cat;
			if (string.IsNullOrEmpty(query))
			{
				return View(new List<Bouquet>());
			}

			var products = _context.Bouquets
		   .Where(p => p.BouquetName.Contains(query) || p.BouquetDescription.Contains(query)|| p.Category.CategoryName.Contains(query))
		   .ToList();
			return View(products);
		}
		public class ProductRepository1
		{
			private readonly BlossomCartsContext context1;
			public ProductRepository1(BlossomCartsContext _context)
			{
				context1 = _context;
			}
			public IEnumerable<Bouquet> GetProducts(int page, int pagesize)
			{
				return context1.Bouquets
					.Where(p => p.Status == 2)
					.Skip((page - 1) * pagesize)
					.Take(pagesize)
					.Include(p => p.Category);
			}
			public int GetTotalProducts()
			{


				return context1.Bouquets
					 .Where(p => p.Status == 2)
					.Count();
			}
		}
		[Authorize]
		
		
		public IActionResult Categories(int? id, int page = 1, int pageSize = 6)
		{
			// Define the model that will be returned to the view
			
			var model = new CategoryIsotopeViewModel
			
			{
				// Fetch and prepare the category data
				Categories = _context.Categories
					.Select(c => new CategoryViewModel
					{
						CategoryId = c.CategoryId,
						CategoryName = c.CategoryName
					})
					.ToList()
			};
			if (id.HasValue)
			{


				model.Bouquets = _context.Bouquets
					 .Where(b => b.CategoryId == id.Value)
					 .OrderByDescending(b=> b.BouquetId)

					.Select(b => new BouquetViewModel
					{
						BouquetId = b.BouquetId,
						BouquetName = b.BouquetName,
						BouquetDescription = b.BouquetDescription,
						Price = b.Price,
						Image = b.Image,
						Status = b.Status
					})
					.ToList();


			}
			else if (id == null)
			{
				model.Bouquets = _context.Bouquets
					.OrderByDescending(b => b.BouquetId)

				   .Select(b => new BouquetViewModel
				   {
					   BouquetId = b.BouquetId,
					   BouquetName = b.BouquetName,
					   BouquetDescription = b.BouquetDescription,
					   Price = b.Price,
					   Image = b.Image,
					   Status = b.Status
				   })
				   .ToList();

			}

			// Calculate the total number of bouquets for the selected category
			var totalBouquets = _context.Bouquets
				.OrderByDescending(b => b.BouquetId)
				.Where(b => !id.HasValue || b.CategoryId == id.Value)
				.Count();

			// Calculate the total number of pages
			var totalPages = (int)Math.Ceiling((double)totalBouquets / pageSize);

			// Fetch and prepare the paginated bouquet data
			model.Bouquets = _context.Bouquets
				.Where(b => !id.HasValue || b.CategoryId == id.Value)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(b => new BouquetViewModel
				{
					BouquetId = b.BouquetId,
					BouquetName = b.BouquetName,
					BouquetDescription = b.BouquetDescription,
					Price = b.Price,
					Image = b.Image,
					Status = b.Status
				})
				.ToList();

			// Add pagination information to the model

			model.CurrentPage = page;
			model.TotalPages = totalPages;
			model.PageSize = pageSize;

			return View(model);
		}



	}
}
