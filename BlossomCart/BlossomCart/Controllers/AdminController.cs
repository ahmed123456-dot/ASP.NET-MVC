using BlossomCart.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using BlossomCart.ViewModels;
using System.Net.Mail;
using System.Net;

namespace BlossomCart.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
        private readonly BlossomCartsContext db;
        public AdminController(BlossomCartsContext _db)
        {
            db = _db;
        }
      
		public IActionResult Index()
		{
            var dta1 = db.Bouquets.Include(p => p.Category);
            ViewBag.data = dta1;
            var dta = db.Orders.Include(p => p.OrderDetails);
			return View(dta);
		}

        // Trash Products Start
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
                    .Where(p=> p.Status == 2)
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

        public IActionResult TrashProducts(int page=1)
        {
            int pagesize = 4;
            var repository = new ProductRepository1(new BlossomCartsContext());
            var Products = repository.GetProducts(page, pagesize);

            var totalProducts = repository.GetTotalProducts();
            var pager = new Pager
            {
                TotalItems = totalProducts,
                PageSize = pagesize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pagesize)
            };
            var viewmodel = new CategoryIsotopeViewModel { Products = Products, Pager = pager };
            return View(viewmodel);
        }
        //Trash Products End

        public IActionResult Restore(int id)
        {
            ViewBag.CatId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            var bouquetTorestore = db.Bouquets.Find(id);
            if (bouquetTorestore == null)
            {
                return NotFound();
            }
            return View(bouquetTorestore);

        }
        [HttpPost, ActionName("Restore")]
        public IActionResult RestoreConfirmed(int id)
        {
            var bouquetTorestore = db.Bouquets.Find(id);
            if (bouquetTorestore == null)
            {
                return NotFound();
            }

            // Update the status instead of actually deleting the record
            bouquetTorestore.Status = 1;
            db.Bouquets.Update(bouquetTorestore);

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
            TempData["ProductRestore"] = "Product Restore Successfully..!";
            return RedirectToAction("TrashProducts", "Admin");
        }
        public IActionResult PermanentDelete(int id)
        {
            var bouquetTodelete = db.Bouquets.Find(id);
            db.Bouquets.Remove(bouquetTodelete);
            db.SaveChanges();
            TempData["ProductDelete"] = "Product Delete Successfully..!";

            return RedirectToAction("TrashProducts");

        }

        // Order Table Start
        public class ProductRepository
        {
            private readonly BlossomCartsContext context;
            public ProductRepository(BlossomCartsContext _context)
            {
                context = _context;
            }
            public IEnumerable<Order> GetProducts(int page, int pagesize)
            {
                return context.Orders
                    .Where(p => p.Status == "Pending"&& (p.PaymentStatus=="Cod"|| p.PaymentStatus=="Paid"))
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize).Include(p => p.OrderDetails);
            }
            public int GetTotalProducts()
            {
                
                
                    return context.Orders
                    .Where(p => p.Status == "Pending" && (p.PaymentStatus == "Cod" || p.PaymentStatus == "Paid"))
                    .Count();
                
               
               
            }
        }
        public ActionResult Orders(int page = 1)
        {

            int pagesize = 4;
            var repository = new ProductRepository(new BlossomCartsContext());
            var Products = repository.GetProducts(page, pagesize);

            var totalProducts = repository.GetTotalProducts();
            var pager = new Pager
            {
                TotalItems = totalProducts,
                PageSize = pagesize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pagesize)
            };
            var viewmodel = new CategoryIsotopeViewModel { Products1 = Products, Pager = pager };
            return View(viewmodel);

        }
        // Orders Table End
        // Order Shipped Start
        public class ProductRepository2
        {
            private readonly BlossomCartsContext context;
            public ProductRepository2(BlossomCartsContext _context)
            {
                context = _context;
            }
            public IEnumerable<Order> GetProducts(int page, int pagesize)
            {
                return context.Orders
                    .Where(p => p.Status == "Shipped")
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize).Include(p => p.OrderDetails);

            }
            public int GetTotalProducts()
            {


                return context.Orders
                      .Where(p => p.Status == "Shipped")
                    .Count();



            }
        }
        public ActionResult ShippedOrders(int page = 1)
        {

            int pagesize = 4;
            var repository = new ProductRepository2(new BlossomCartsContext());
            var Products = repository.GetProducts(page, pagesize);

            var totalProducts = repository.GetTotalProducts();
            var pager = new Pager
            {
                TotalItems = totalProducts,
                PageSize = pagesize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pagesize)
            };
            var viewmodel = new CategoryIsotopeViewModel { Products1 = Products, Pager = pager };
            return View(viewmodel);

        }

        public IActionResult Shipped(int id)
        {
            var order = db.Orders.Include(p=> p.User);
            var bouquetToDelete = order.FirstOrDefault(p=> p.OrderId ==id);
            
            if (bouquetToDelete == null)
            {
                return NotFound();
            }

            // Update the status instead of actually deleting the record
            bouquetToDelete.Status = "Shipped";
            db.Orders.Update(bouquetToDelete);

			if(bouquetToDelete.User != null && !string.IsNullOrEmpty(bouquetToDelete.User.EMail))

	        {
				if (SendEmail(bouquetToDelete.User.EMail, "Your Order has been Delivered Successfully.", "Successfully Delivered"))
				{
					ViewBag.msg = "Check your Inbox.";
				}
			}

	        else
			{
				// Handle the scenario where the user's email is not available
				ViewBag.msg = "User email not found.";
			}





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



            TempData["ProductShipped"] = "Order Shipped Successfully..!";
            return RedirectToAction("Orders", "Admin");
        }
        public bool SendEmail(string email,string message,string subject)
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
        public IActionResult Delete(int id)
        {
            var Delete = db.Orders.Find(id);
             if (Delete != null)
            {
                var Orderdetail1 = db.PaymentTables.Where(p => p.OrderId == id).ToList();
                foreach (var item in Orderdetail1)
                {
                    db.PaymentTables.Remove(item);
                    db.SaveChanges();
                }
                var Orderdetail = db.OrderDetails.Where(p => p.OrderId==id).ToList();
                foreach(var item in Orderdetail)
                {
                    db.OrderDetails.Remove(item);
                    db.SaveChanges();
                }
                db.Orders.Remove(Delete);
                db.SaveChanges();
                TempData["DeleteMessage"] = "Order Deleted Successfully..!";
                return RedirectToAction("Orders");
            }
            else
            {
                TempData["DeleteMessage"] = "Order Not Deleted Successfully";
                return RedirectToAction("Orders");
            }
            

        }


    }
}
