using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EFDataContext.Models
{
    /// <summary>
    ///     Клас, який позначає елемент
    ///     для роботи з головним вікном
    /// </summary>
    public class WindowElement : INotifyPropertyChanged, IDataErrorInfo
    {
        //  Властивість, яка початково ініціалізує текстове поле
        private string _count = "0";
        //  Встастивість, яка викликає метод валідації і повертає строку помилки
        public string this[string columnName] => GetValidData(columnName);
        //  Властивість, яка надає доступ до властивості _count
        public string Count
        {
            get { return _count; }
            set { _count = value;
                //  Викликання методу, який змінює дані на UI
                OnPropertyChanged("Count");
            }
        }
        /// <summary>
        ///     Метод, який перевіряє текстове поле на валідність
        ///     і у випадку помилки повертає відповідну строку помилки
        /// </summary>
        /// <param name="columnName">Приймає назву Властивості</param>
        /// <returns>Повертає строку помилки або null</returns>
        private string GetValidData(string columnName)
        {
            //  Перевірка чи назва елемента UI привязуєтсься до властивості Count
            if (columnName == "Count")
            {
                int res;
                //  Перевіряє чи текст поля можна перетворити у int
                if (!int.TryParse(_count, out res))
                {
                    //  Якщо не вийшло перетворити повертає помилку
                    return "Некоректний тип!";
                }
                    //  Перевіряє чи результат менший за 0
                if (res < 0)
                {
                    //  Якщо результат менший за 0 повертає відповідну помилку
                    return "Не може бути кількість меншою за 0!";
                }
            }
            //  Якщо помилок не виявлено повретається тип null
            return null;
        }
        /// <summary>
        ///     Властивість, яка викликає метод валідації,
        ///     і повертає строку помилки якщо вона виявлена
        /// </summary>
        public string Error 
            {
                get 
                {
                //  Повернення резульату роботи методу GetValidData
                return GetValidData("Count");
                }
            }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        ///     Метод, який змінює UI, а саме елементи
        ///     до який привязуються властивості БД
        /// </summary>
        /// <param name="prop">Приймається нозва властивості, яка змінена</param>
        private void OnPropertyChanged(string prop) 
        {
            //  Подія, до якої автоматично привязується метод і яка змінює елементи
            //  UI, які привязані до властивостей даного класу
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
