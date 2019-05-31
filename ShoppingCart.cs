using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using MercadoPago;
//using MercadoPago.Resources;
//using MercadoPago.DataStructures.Preference;

namespace PaginaGabriel.Models
{
    public class ShoppingCart
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public string ShoppingCartId { get; set; }

        
        
        public const string CartSessionKey = "cartId";


        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();

            cart.ShoppingCartId = cart.GetCartId(context);
            
            return cart;
        }

        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public void AddToCart(Producto producto)
        {
            var cartItem = db.Carritos.SingleOrDefault(c => c.CartId == ShoppingCartId && c.ProductoId == producto.ProductoId);

            if (cartItem == null)
            {
                cartItem = new Carrito
                {
                    ProductoId = producto.ProductoId,
                    CartId = ShoppingCartId,
                    Cantidad = 1,
                    //Cantidad = producto.CantidadComprar,
                    FechaCreacion = DateTime.Now
                };
                db.Carritos.Add(cartItem);
            }
            else
            {
                cartItem.Cantidad++;
            }

            db.SaveChanges();
        }

        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }

                else
                {
                    Guid tempCartId = Guid.NewGuid();
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }

            return context.Session[CartSessionKey].ToString();
        }

        public decimal GetTotal()
        {
            decimal? total = (from cartItems in db.Carritos
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Cantidad * cartItems.Producto.Precio).Sum();

            return total ?? decimal.Zero;
        }

        public List<Carrito> GetCartItems()
        {
            return db.Carritos.Where(cart => cart.CartId == ShoppingCartId).ToList();
        }

        public int RemoveFromCart(int id)
        {
            var cartItem = db.Carritos.SingleOrDefault(cart => cart.CartId == ShoppingCartId && cart.ProductoId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Cantidad > 1)
                {
                    cartItem.Cantidad--;
                    itemCount = cartItem.Cantidad;
                }
                else
                {
                    db.Carritos.Remove(cartItem);
                }

                db.SaveChanges();
            }
            return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = db.Carritos.Where(cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                db.Carritos.Remove(cartItem);
            }
            db.SaveChanges();
        }

        public int GetCount()
        {
            int? count =
                (from cartItems in db.Carritos where cartItems.CartId == ShoppingCartId select (int?)cartItems.Cantidad).Sum();

            return count ?? 0;
        }

        public int CreateOrder(PedidosCliente customerOrder)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            ////Mercado Pago

            //SDK.ClientId = "1782351866703164";
            //SDK.ClientSecret = "6nNax7XMsv6y4qiKxDhEJ4zmZZlCmlCN";

            //// Create a preference object
            //Preference preference = new Preference();



            foreach (var item in cartItems)
            {
                var stockProductos = db.Productos.FirstOrDefault(p => p.ProductoId == item.Producto.ProductoId);

                stockProductos.Stock = stockProductos.Stock - item.Cantidad;

                var orderedProduct = new ProductosPedidos
                {
                    ProductoId = item.ProductoId,
                    PedidoClienteId = customerOrder.Id,
                    Cantidad = item.Cantidad
                };

                orderTotal += (item.Cantidad * item.Producto.Precio);

                db.ProductosPedidos.Add(orderedProduct);

            //    //# Adding an item object
            //    preference.Items.Add(
            //      new Item()
            //      {
            //          Id = item.ProductoId.ToString(),
            //          Title = "Nombre: " + customerOrder.NombreCompleto,
            //          Quantity = item.Cantidad,
            //          CurrencyId = 0,
            //          UnitPrice = item.Producto.Precio
            //      }
            //    );            }

            
            //// Setting a payer object as value for Payer property
            //preference.Payer = new Payer()
            //{
            //    Email = customerOrder.Email,
            //    Name = customerOrder.Nombres,
            //    Surname = customerOrder.Apellido,
            //    Phone = new Phone
            //    {
            //        Number = customerOrder.Telefono
            //    },

            //    Date_created = customerOrder.FechaCreacion,


            //    Identification = new Identification
            //    {
            //        Type = "DNI",
            //        Number = customerOrder.DNI,
            //    }               

            };
            //// Save and posti   ng preference
            //preference.Save();

            customerOrder.Cantidad = orderTotal;
            //customerOrder.UrlMercadoPago = preference.InitPoint;


            db.SaveChanges();

            EmptyCart();

            return customerOrder.Id;
        }

        public void MigrateCart(string userName)
        {
            var shoppingCart = db.Carritos.Where(c => c.CartId == ShoppingCartId);
            foreach (Carrito item in shoppingCart)
            {
                item.CartId = userName;
            }

            db.SaveChanges();
        }
    }
}