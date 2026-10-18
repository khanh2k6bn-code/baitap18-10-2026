using System;

namespace BookManagerApp
{
    public class BookModel
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CoverImagePath { get; set; } // Đường dẫn file ảnh bìa
    }
}
