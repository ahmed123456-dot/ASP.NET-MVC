
using BlossomCart.Models;
using System.Collections.Generic;

namespace BlossomCart.ViewModels
{
    public class CategoryIsotopeViewModel
    {

		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
		public int PageSize { get; set; }
		public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<BouquetViewModel>? Bouquets { get; set; } // Nullable because it may be empty when viewing all categories.
		public IEnumerable<Bouquet> Products { get; set; } = null!;
        public IEnumerable<Order> Products1 { get; set; } = null!;
		public IEnumerable<OrderDetail> Products2 { get; set; } = null!;
		public Pager Pager { get; set; } = null!;
		public string SearchQuery { get; set; }
	}

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class BouquetViewModel
    {
        public int BouquetId { get; set; }
        public string BouquetName { get; set; } = null!;
        public string BouquetDescription { get; set; } = null!;
        public int Price { get; set; }
        public string Image { get; set; } = null!;
        public int? Status { get; set; }
	}
	public class Pager
	{
		public int TotalItems { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
	}
}
