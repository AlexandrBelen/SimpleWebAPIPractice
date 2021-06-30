using System.Collections.Generic;

namespace MyAPI.Models
{
    public class Category
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
