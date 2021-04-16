using System;
using System.Collections.Generic;
using System.Text;

namespace EFDataContext.Models
{
    /// <summary>
    ///     Модель, яка працює з елементами котів 
    ///     які витягуються з БД
    /// </summary>
    public class CatModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImgUrl { get; set; }
        public DateTime Birthday { get; set; }
    }
}
