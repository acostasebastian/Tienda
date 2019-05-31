using PaginaGabriel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaginaGabriel.ViewModels
{
    public class ShoppingCartViewModel : ShoppingCart
    {
        public List<Carrito> CartItems { get; set; }
        public decimal CartTotal { get; set; }

        
    }
}