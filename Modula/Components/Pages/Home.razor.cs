using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modula.Components.Pages
{
    public partial class Home
    {
        private Product? FocusedRow { get; set; }
        public List<Product> SelectedRows => Products.Where(p => p.IsSelected).ToList();
        public List<Product> Products { get; set; } = new()
        {
            new Product { Code = "SP001", Name = "Sản phẩm A", QRCodeUrl = "images/qr1.png", Location = "Kho A1", Status = "Active" },
            new Product { Code = "SP002", Name = "Sản phẩm B", QRCodeUrl = "images/qr2.png", Location = "Kho B2", Status = "Inactive" }
        };
        private void OnRowClick(Product item)
        {
            FocusedRow = item;
        }
        private void ToggleSelectAll(ChangeEventArgs e)
        {
            bool check = (bool)(e.Value ?? false);

            foreach (var p in Products)
                p.IsSelected = check;

            FocusedRow = null;
        }

        private void ToggleSingle(Product item, ChangeEventArgs e)
        {
            item.IsSelected = (bool)(e.Value ?? false);
        }
    }
    public class Product
    {
        public bool IsSelected { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string QRCodeUrl { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }
}
