using EFEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDataContext.Services
{
    public static class DbSeeder
    {
        /// <summary>
        ///     Метод, який початково заповнює
        ///     БД синхронно
        /// </summary>
        /// <param name="context">Приймає обєкт, який є звязком з БД</param>
        private static void SeedAll(EFContext context) 
        {
            //  Перевірка чи БД не пуста
            if (!context.Cats.Any()) 
            {
                //  Ініціалізація обєкту AppCat
                AppCat cat = new AppCat {
                Name = "Вася",
                Birthday = DateTime.Now,
                ImgUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d6/Manoel.jpg/275px-Manoel.jpg",
                };
                //  Запис нового обєкту у БД
                context.Cats.Add(cat);
                //  Збереження змін
                context.SaveChanges();
                //  Ініціалізація нового обєкту AppPrice,
                //  який привязується до елемента AppCat
                AppPrice price = new AppPrice { 
                DateCreate = DateTime.Now,
                CatId = cat.Id,
                Price = 500
                };
                //  Запис нового обєкту у БД
                context.CatPrices.Add(price);
                //  Збереження змін
                context.SaveChanges();
            }
        }
        /// <summary>
        ///     Метод, який початково заповнює
        ///     БД асинхронно (визиваючи чинхронний метод у вторичному потоці)
        /// </summary>
        /// <param name="context">Приймає обєкт, який є звязком з БД</param>
        /// <returns>Повертає таску</returns>
        public static Task SeedAllAsync(EFContext context) 
        {
            //  Запускання сідера у вторичному потоці
            return Task.Run(() => SeedAll(context));
        }
    }
}
