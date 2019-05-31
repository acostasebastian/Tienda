using PaginaGabriel.Models;
using PaginaGabriel.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PaginaGabriel.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ShoppingCart
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            return View(viewModel);
            //return View();
        }


        public ActionResult IndexError()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal(),
            };

            return View(viewModel);

        }

        public ActionResult AddToCart(int id, int cantidadPedida)
        //public JsonResult AddToCart(int id, int cantidad)
        {          

            var addedProduct = db.Productos.Single(product => product.ProductoId == id);


            var stock = addedProduct.Stock;
            addedProduct.CantidadComprar = cantidadPedida;


            if (stock == null | stock == 0)
            {
                TempData["mensaje"] = "No hay Stock de " + addedProduct.Descripcion;

                return RedirectToAction("IndexError");


            }
            else
            {
                var cart = ShoppingCart.GetCart(this.HttpContext);

             


                cart.AddToCart(addedProduct);

                stock = stock - 1;

                db.Entry(addedProduct).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
            
           // return Json(addedProduct);
        }


        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            string productName = db.Carritos.FirstOrDefault(item => item.ProductoId == id).Producto.Descripcion;

            int itemCount = cart.RemoveFromCart(id);

            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(productName) + " fue removido del carrito.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };

            return Json(results);
        }

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            return PartialView("CartSummary");
        }

     


    }
}