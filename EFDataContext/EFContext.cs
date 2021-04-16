using EFEntities;
using Microsoft.EntityFrameworkCore;
using System;

namespace EFDataContext
{
    /// <summary>
    ///     Клас, який встановлює зєднання з БД
    ///     і дозволяє маніпулювати таблицями (елементами) БД
    /// </summary>
    public class EFContext : DbContext
    {
        /// <summary>
        ///     Властивість позначає колекцію котів
        ///     які є у БД
        /// </summary>
        public DbSet<AppCat> Cats { get; set; }
        /// <summary>
        ///     Властивість позначає колекцію цін на котів, які є у БД
        ///     Між ними є ForeignKey звязок
        /// </summary>
        public DbSet<AppPrice> CatPrices { get; set; }
        /// <summary>
        ///     Метод, який ініціалізує звязок з БД
        /// </summary>
        /// <param name="optionsBuilder">Приймає обєкт DbContextOptionsBuilder, який встановлює
        /// звязок з БД</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //  Метод, який підключається до БД, використовуючи строку підключення
            optionsBuilder.UseNpgsql("Server=91.238.103.51;Port=5743;Database=denysdb;Username=denys;Password=qwerty1*;");
        }
    }
}
