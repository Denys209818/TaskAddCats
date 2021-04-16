using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFEntities
{
    /// <summary>
    ///     Клас, який представляє собою таблицю
    ///     яка працює з котами
    /// </summary>
    [Table("tblAppCatTaskAddCats")]
    public class AppCat
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        [Required, StringLength(4000)]
        public string ImgUrl { get; set; }
        public virtual ICollection<AppPrice> CatPrices { get; set; }
    }
}
