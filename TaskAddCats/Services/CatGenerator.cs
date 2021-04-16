using Bogus;
using EFDataContext.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAddCats.Services
{
    /// <summary>
    ///     Статичний клас, який за допомогою Bogus генерує обєкт кота
    /// </summary>
    public static class CatGenerator
    {
        /// <summary>
        ///     Властивість, яка містить правила створення нового обєкта
        ///     і методом Generate(), повертає нового кота
        /// </summary>
        public static Faker<CatModel> catFaker { get; set; } = null;
        /// <summary>
        ///     Метод, який генерує і повертає обєкт CatModel
        /// </summary>
        /// <returns>Повертає згенерований обєкт CatModel</returns>
        public static CatModel GetCat() 
        {
            //  Перевірка властивості чи вона не пуста і якщо пуста проініціалізувати
            //  елемент, встановивши правила генерації котів
            if (catFaker == null)
                InitializeCat();
            //  Повернення згенерованого обєкта кота
            return catFaker.Generate();
        }
        /// <summary>
        ///     Метод, який ініціалізує властивість catFaker
        ///     і встановлює правила генерації обєкта
        /// </summary>
        private static void InitializeCat() 
        {
            //  Ініціалізація обєкта Faker
            //  а саме встановлення правил для генерації обєкта кота
            catFaker = new Faker<CatModel>("uk")
                .RuleFor(cat => cat.Name, (f, tp) => f.Name.FirstName())
                .RuleFor(cat => cat.Price, (f, tp) => Math.Round(f.Random.Decimal(100, 1000)))
                .RuleFor(cat => cat.Birthday, (f, tp) => f.Date.Past(5, DateTime.Now))
                .RuleFor(cat => cat.ImgUrl, (f, tp) => "https://zooblog.ru/wp-content/uploads/2021/01/pallas_catsweb-1140x694.jpg");
                
        }
    }
}
