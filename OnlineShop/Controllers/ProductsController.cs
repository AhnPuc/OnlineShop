using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;
using System.Text.RegularExpressions;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Product> products = _context.Products.OrderByDescending(x => x.Id).ToList();
            return View(products);
        }

        public IActionResult SearchProducts(string SearchText)
        {
            var products = _context.Products
                .Where(x =>
                EF.Functions.Like(x.Title, "%" + SearchText + "%") ||
                EF.Functions.Like(x.Tags, "%" + SearchText + "%")
                )
                .OrderBy(x => x.Title)
                .ToList();
            return View("Index", products);
        }

        public IActionResult ProductDetails(int id)
        {
            Product? product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) 
            {
                return NotFound();
            }
            ViewData["gallery"] = _context.ProductGaleries.Where(x => x.ProductId == id).ToList();

            ViewData["NewProducts"] = _context.Products.Where(x => x.Id != id).
                                        Take(6).OrderByDescending(x => x.Id).ToList();

            ViewData["comments"] = _context.Comments.Where(x => x.ProductId == id).
                                        OrderByDescending(x => x.CreateDate).ToList();

            if (!string.IsNullOrEmpty(product.Tags))
            {
                // Split the current product's tags
                var productTags = product.Tags.Split(',').Select(t => t.Trim().ToLower()).ToList();

                // Find products with similar tags
                var recommendedProducts = _context.Products
                    .Where(p => p.Id != id && !string.IsNullOrEmpty(p.Tags))
                    .AsEnumerable() // Process in memory to use custom comparison logic
                    .Select(p => new
                    {
                        Product = p,
                        MatchedTags = p.Tags.Split(',')
                            .Select(t => t.Trim().ToLower())
                            .Intersect(productTags).Count()
                    })
                    .Where(x => x.MatchedTags > 0) // Must have at least one matching tag
                    .OrderByDescending(x => x.MatchedTags) // Order by number of matching tags
                    .ThenByDescending(x => x.Product.Id) // Secondary ordering by newest
                    .Take(4) // Limit to 4 recommended products
                    .Select(x => x.Product)
                    .ToList();

                ViewData["RecommendedProducts"] = recommendedProducts;
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult SubmitComment(string name, string email, string comment, int productID)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(comment) && productID != 0)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (!match.Success)
                {
                    TempData["ErrorMessage"] = "Email is not valid";
                    return Redirect("/Products/ProductDetails/" + productID);
                }
                
                Comment newComment = new Comment();
                newComment.Name = name;
                newComment.Email = email;
                newComment.CommentText = comment;
                newComment.ProductId = productID;
                newComment.CreateDate = DateTime.Now;

                _context.Comments.Add(newComment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your comment submited success fully";
                return Redirect("/Products/ProductDetails/" + productID);
            }
            else
            {
                TempData["ErrorMessage"] = "Please complete your information";
                return Redirect("/Products/ProductDetails/" + productID);
            }    
        }
    }
}
