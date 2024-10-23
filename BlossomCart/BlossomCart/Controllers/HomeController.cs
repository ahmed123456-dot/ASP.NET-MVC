using BlossomCart.Models;
using BlossomCart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using System.Net.Mail;
using System.Net;

namespace BlossomCart.Controllers
{
	public class HomeController : Controller
	{

		private readonly BlossomCartsContext db;
		public HomeController(BlossomCartsContext _db)
		{
			db = _db;
		}

		public IActionResult Index(int? id)
		{

			var Productsdata = db.Bouquets.Include(p => p.Category);
			return View(Productsdata);

		}

		public IActionResult Privacy()
		{
			return View();
		}
		[Authorize (Roles ="user")]
		public IActionResult Details(int id)
		{
			var Productsdata = db.Bouquets.Include(p => p.Category);
			ViewBag.CatId = new SelectList(db.Categories, "CatId", "CatName");
			var ItemsData = db.Bouquets.FirstOrDefault(a => a.BouquetId == id);
			if (ItemsData != null && Productsdata != null)
			{
				Cart cart = new Cart();

				ViewBag.Cart = cart;

				return View(ItemsData);

			}
			else
			{
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
		[Authorize(Roles = "user")]
		public IActionResult AddtoCart(Cart cart)
		{
			var duplicateItem = db.Carts.FirstOrDefault(c => c.BouquetId == cart.BouquetId && c.UserId == cart.UserId);
			if (duplicateItem != null)
			{
				duplicateItem.Qty += cart.Qty;// qty = 1 +2 => 3
				duplicateItem.TotalPrice = duplicateItem.Qty * duplicateItem.Price;
				db.Carts.Update(duplicateItem);
				db.SaveChanges();

				return View();
			}
			else
			{
				cart.TotalPrice = cart.Price * cart.Qty;//5 * 100 = 500
				db.Carts.Add(cart);
				db.SaveChanges();
				TempData["Insert_Product"] = "Product Successfully Inserted Into Carts..!";

				return RedirectToAction("Categories","Shop");
			}




			
		}
		[Authorize(Roles = "user")]
		public IActionResult AddTocart()
		{

			var Productsdata = db.Carts.Include(p => p.Bouquet).Include(p => p.User);
			return View(Productsdata);
		}
		[Authorize(Roles = "user")]
		public IActionResult Delete(int id)
		{
			var bouquetTodelete = db.Carts.Find(id);
			if (bouquetTodelete != null)
			{
				db.Carts.Remove(bouquetTodelete);
				db.SaveChanges();
				TempData["Delete_Carts"] = "Product Deleted Into Carts..!";

				return RedirectToAction("AddTocart");
			}
			else
			{
				return RedirectToAction("AddTocart");
			}




		}
		[Authorize(Roles = "user")]
		public IActionResult Checkout()
		{
			var UserId = Convert.ToInt32(HttpContext.Session.GetInt32("id"));

			var cat = db.Carts.Include(p => p.Bouquet).ToList();
			ViewBag.Cartdata = cat;
		
		
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "user")]
		public IActionResult Checkout(Order order)
		{

			db.Orders.Add(order);
			db.SaveChanges();
			var UserId = Convert.ToInt32(HttpContext.Session.GetInt32("id"));
			var order1 = db.Orders
			  .Where(p => p.UserId == UserId && p.Status == "Pending")
			  .OrderBy(p => p.OrderId) // Replace SomeSortingProperty with the property you want to sort by
			  .LastOrDefault();

			var Orderid = order1?.OrderId;

			var Cartdata = db.Carts.Where(p => p.UserId == UserId).ToList();
			foreach (var item in Cartdata)
			{
				OrderDetail NewOrderdetail = new OrderDetail()
				{
					OrderId = (int)Orderid,
					BouquetId = item.BouquetId,
					UserId = item.UserId,
					Price = item.Price,
					Qty = item.Qty,
					TotalPrice = item.TotalPrice,

				};

				db.OrderDetails.Add(NewOrderdetail);
				db.Carts.Remove(item);
				db.SaveChanges();
			

			}
            if (order.PaymentType == "CreditCard")
            {
                TempData["Order_Id"] = Orderid;
                return RedirectToAction("Payment");
            }
            else
            {
                order.PaymentStatus = "Cod";
                db.Orders.Update(order);
                db.SaveChanges();

                TempData["Orderinsert"] = "Order Successfully Inserted..!";
                return RedirectToAction("Checkout");
            }

           
			}




		[Authorize(Roles = "user")]
		public IActionResult Contact()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Contact([Bind("FId,CustomerName,CustomerEmail,Feedback1")] Feedback feedback)
		{
			if (ModelState.IsValid)
			{
				db.Add(feedback);
				await db.SaveChangesAsync();
				TempData["Sendfeedback"]="Your Feedback is Send..!";
				return View();
			}
			return View(feedback);
		}
		public IActionResult Feedback()
		{

			return View(db.Feedbacks.ToList());
		}
		
		public IActionResult Payment()
		{
			
			
				return View();
			
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "user")]
		public IActionResult Payment(PaymentTable Payment)
        {
           
            var UserId = Convert.ToInt32(HttpContext.Session.GetInt32("id"));
			var order1 = db.Orders
				.Include(p=> p.User)
			  .Where(p => p.UserId == UserId && p.Status == "Pending")
			  .OrderBy(p => p.OrderId) // Replace SomeSortingProperty with the property you want to sort by
			  .LastOrDefault();
			
			if(order1.PaymentType == "CreditCard")
			{
                order1.PaymentStatus = "Paid";
                db.Orders.Update(order1);
				db.PaymentTables.Add(Payment);
				db.SaveChanges();
                if (order1.User != null && !string.IsNullOrEmpty(order1.User.EMail))

                {
                    if (SendEmail(order1.User.EMail, "Your Payment SuccessFully Paid.", "Successfully Paid"))
                    {
                        
                    }
                }
				TempData["Orderinsert"] = "Order Successfully Inserted. Check E-Mail...!";

				return RedirectToAction("Checkout");

            }
            return View();
        }


        // Email
        public bool SendEmail(string email, string message, string subject)
        {


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("ahmedidrees247@gmail.com", "blvj vfmd eopq ywsz");

            MailMessage msg = new MailMessage("ahmedidrees247@gmail.com", email);
            msg.Subject = subject;

            msg.Body = message;


            // msg.Attachments.Add(new Attachment(PathToAttachment));
            client.Send(msg);


            return true;
        }

		public class ProductRepository1
		{
			private readonly BlossomCartsContext context1;
			public ProductRepository1(BlossomCartsContext _context)
			{
				context1 = _context;
			}
			public IEnumerable<OrderDetail> GetProducts(int page, int pagesize,int userid  )
			{
				return context1.OrderDetails
					
					
					.Where(p => p.UserId == userid && (p.Order.PaymentStatus == "Cod" || p.Order.PaymentStatus == "Paid"))
					.Include(p => p.Bouquet)
					.Include(p => p.Order)
					.Skip((page - 1) * pagesize)
					.Take(pagesize);
					


					
			}
			public int GetTotalProducts(int userid)
			{

				return context1.OrderDetails
					.Where(p=> p.UserId == userid && (p.Order.PaymentStatus=="Cod" || p.Order.PaymentStatus=="Paid" ))
					.Count();
			}
		}

		[Authorize(Roles = "user")]
		public IActionResult MyCart(int page = 1)
		{

			var UserId = Convert.ToInt32(HttpContext.Session.GetInt32("id"));

			int pagesize = 4;
			var repository = new ProductRepository1(new BlossomCartsContext());
			var Products = repository.GetProducts(page, pagesize, UserId);

			var totalProducts = repository.GetTotalProducts(UserId);
			var pager = new Pager
			{
				TotalItems = totalProducts,
				PageSize = pagesize,
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)totalProducts / pagesize)
			};
			var viewmodel = new CategoryIsotopeViewModel { Products2 = Products, Pager = pager };
			return View(viewmodel);
		}
	}


}
