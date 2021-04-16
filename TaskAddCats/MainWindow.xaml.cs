using EFDataContext;
using EFDataContext.Models;
using EFDataContext.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskAddCats.Services;

namespace TaskAddCats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///     Обєкт для роботи з БД
        /// </summary>
        public EFContext _context { get; set; }
        /// <summary>
        ///     Обєкт, який відповідає за валідацію введених даних
        ///     тобто є контекстом програми
        /// </summary>
        public WindowElement _windowElement { get; set; } = new WindowElement();
        /// <summary>
        ///     Обєкт, який методом WaitOne() зупиняє роботу потока
        ///     і очікує викликання методу Set(). Потребує автоматичного скидання,
        ///     тобто обєкт якщо діждавася метод Set(), то щоб він в подальшому зупиняв
        ///     роботу потоку потрібно вручну викликати метод Reset(), який скидає налаштування обєкту
        /// </summary>
        public ManualResetEvent resetEvent { get; set; } = new ManualResetEvent(true);
        /// <summary>
        ///     Прапорець, який вказує чи скасована операція додавання
        ///     котів у БД
        /// </summary>
        public bool IsCancel { get; set; } = false;
        /// <summary>
        ///     Конструктор, який ініціалізує вікно
        ///     і присвоює контексту додатку обєкт WindowElement,
        ///     який відповідає за валідацію введених даних
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _windowElement;
        }
        /// <summary>
        ///     Подія, яка виникає, коли вікно завантажилось
        ///     Викликає асинхронно сідер і заповнює DataGrid асинхронно
        ///     за допомогою конструкції await, яка виносить виконання методу у вторичний потік
        /// </summary>
        /// <param name="sender">Обєкт, який згенерував подію</param>
        /// <param name="e">Параметри</param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  Ініціалізація обєкту, який створює звязок з БД
            _context = new EFContext();
            //  Викликання сідера асинхронно
            await DbSeeder.SeedAllAsync(_context);
            //  Метод, який асинхронно заповнює DataGrid
            string time = await FillDataGrid();
            //  Налаштування:
            //  Встановлення у статусбар часу виконання запиту, який заповнює DataGrid
            this.txtTime.Text = "Час виконання запиту: " + time;
            //  Встановлення заборони на додавання користувачем обєкта до DataGrid
            //  саме в DataGrid
            this.dgCats.CanUserAddRows = false;
        }
        /// <summary>
        ///     Метод, який заповнює DataGrid  об'єктами з БД
        /// </summary>
        /// <returns>Повертає час роботи ініціалізації данних з БД</returns>
        private async Task<string> FillDataGrid()
        {
            //  Запускає виконання методу асинхронно
            return await Task<string>.Run(() => {
                //  Змінення тексту статусбара за допомогою Dispatcher,
                //  оскільки робота в потоці не може на пряму впливати на
                //  елементи Window, оскільки вони закріплені за головним потоком
                Dispatcher.Invoke(new Action(() => {
                    this.txtStatus.Text = "Початок ініціалізації DataGrid";
                }));
                //  Ініціалізація спеціального таймера, який відслідковує роботу
                //  витягання обєктів з БД і присвоєння до DataGrid цих данних
                Stopwatch stopwatch = new Stopwatch();
                //  Активування таймеру
                stopwatch.Start();
                //  Ініціалізація колекції елементів, які витягуюються з БД
                var list = _context.Cats.Select(x => new CatModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.CatPrices.OrderByDescending(y => y.Id).FirstOrDefault().Price,
                    ImgUrl = x.ImgUrl,
                    Birthday = x.Birthday
                })
                .ToList();
                //  Присвоєння данних до DataGrid за допомогою Dispatcher,
                //  оскільки робота в потоці не може на пряму впливати на
                //  елементи Window, оскільки вони закріплені за головним потоком
                Dispatcher.Invoke(new Action(() => { 
                    this.dgCats.ItemsSource = list;
                }));
                //  Зупинка роботи таймера і повернення обєкту TimeSpan
                //  у якому зберігається час роботи таймера
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                //  Формування строки, яка відображає час роботи ініціалізації
                //  Ця строка повертається
                string result = String.Format("{0:00}:{1:00}:{2:00}:{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds/10);
                //  Змінення тексту статусбара за допомогою Dispatcher,
                //  оскільки робота в потоці не може на пряму впливати на
                //  елементи Window, оскільки вони закріплені за головним потоком
                Dispatcher.Invoke(new Action(() => {
                    this.txtStatus.Text = "Дані завантажено!";
                }));
                //  Повернення строки, яка позначає час роботи ініціалізації колекції
                return result;
            });
        }
        /// <summary>
        ///     Подія, яка додає певну кількість елементів у БД
        ///     Кількість елементів береться із текстового поля
        /// </summary>
        /// <param name="sender">Кнопка, яка згенерувала подію</param>
        /// <param name="e">Параметри обєкта</param>
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //  Метод, який скидає налаштування обєкту ManualResetEvent
            this.Resume();
            //  Асинхронний делегат, який додає елементи у БД
            await Task.Run(() => {
                //  Використання обєкту транзакцій
                //  Цей обєкт записує усі зміни БД у одну транзакцію
                //  Завдяки цьому обєкту можна скасувати транзакцію
                //  після викликання SaveChanges(), завдяки методу Rollback(),
                //  який скасовує транзакцію
                using (IDbContextTransaction t = this._context.Database.BeginTransaction()) 
                {
                    //  Створення прапорця, який повідомляє чи коректно введені дані
                    bool isCorrect = false;
                    Dispatcher.Invoke(() => {
                        //  Встановлення значення. Чи валідні дані
                        isCorrect = string.IsNullOrEmpty((this.DataContext as WindowElement).Error);
                    });
                    //  Якщо дані валідні
                    if (isCorrect)
                    {
                        //  Отримання кількості елементів, що необхідно додати
                        int count = int.Parse(_windowElement.Count);
                        //  Встановлення блокування кнопки і максимального значення для ProgressBar
                        //  за допомогою Dispatcher, оскільки елементи вікна привязані до одного потоку
                        //  і не можуть бути змінені з другого потоку
                        Dispatcher.Invoke(() => { 
                            this.btnAdd.IsEnabled = false;
                            this.pbCats.Maximum = count;
                        });
                        //  Цикл, який додає дані у БД
                        for (int i = 0; i < count; i++) 
                        {
                            //  Перевірка чи не скасована операція
                            if (this.IsCancel) 
                            {
                                break;
                            }
                            //  Викликання методу WaitOne(), який очікує 
                            //  метода Set(), що відновлює роботу потоку
                            //  По замовчанню викликаєтсья метод Set()
                            resetEvent.WaitOne();
                            //  Встановлення значення кнопки і значення для ProgressBar
                            //  за допомогою Dispatcher, оскільки елементи вікна привязані до одного потоку
                            //  і не можуть бути змінені з другого потоку
                            Dispatcher.Invoke(() => { 
                                this.btnAdd.Content = (i+1).ToString();
                                this.pbCats.Value = (i+1);
                            });

                        
                        /// Додавання кота у БД
                        //  Генерація кота за допомогою Bogus
                            CatModel cat = CatGenerator.GetCat();
                            //  Формування обєкту AppCat, для подільшого додавання у БД
                                var dbCat = new EFEntities.AppCat
                        {
                            Name = cat.Name,
                            Birthday = cat.Birthday,
                            ImgUrl = cat.ImgUrl
                        };
                            //  Додавання нового обєкту у БД (Дані привязуються до транзакцію)
                            this._context.Cats.Add(dbCat);
                        ///

                        /// Додавання ціни у БД
                        //  Додавання ціни для нового кота і додавання його у БД (Дані привязуються до транзакції)
                            this._context.CatPrices.Add(new EFEntities.AppPrice { 
                                DateCreate = DateTime.Now,
                                Cat = dbCat,
                                Price = cat.Price
                            });
                        ///
                        //  Зупинка потоку на 0.2 секунди
                        Thread.Sleep(200);
                    }
                        //  Встановлення розблокування кнопки і тексту для кнопки (відновлення поперднього тексту)
                        //  за допомогою Dispatcher, оскільки елементи вікна привязані до одного потоку
                        //  і не можуть бути змінені з другого потоку
                        Dispatcher.Invoke(() => { 
                            this.btnAdd.IsEnabled = true;
                            this.btnAdd.Content = "Додати котів";
                        });
                }
                    //  Збереження даних однією транзакцією у БД
                    _context.SaveChanges();
                    //  Дані налаштування викликаються лише після SaveChanges() (Після проведення операції)
                    if (!this.IsCancel)
                    {
                        //  Метод, який зберігає усі зміни БД
                        t.Commit();
                    }
                    else 
                    {
                        //  Метод, який скасовує транзакцію
                        t.Rollback();
                    }
                    //  Скидання прапорця, який відповідає за скасування
                    //  додавання котів у БД
                    this.IsCancel = false;
                }
            });
            //  Асинхронний метод який заповнює DataGrid даними
            await FillDataGrid();
        }
        /// <summary>
        ///     Метод, який скидає налаштування обєкта ManualResetEvent,
        ///     тобто обєкт може зупиняти роботу потоку методом WaitOne()
        /// </summary>
        private void Pause() => resetEvent.Reset();
        /// <summary>
        ///     Метод, який встановлеє налаштування, що обєкт 
        ///     ManualResetEvent методом WaitOne() не зупиняє роботу потоку
        /// </summary>
        private void Resume() => resetEvent.Set();
        /// <summary>
        ///     Подія, яка зупиняє роботу потоку
        /// </summary>
        /// <param name="sender">Кнопка, яка згенерувала подію</param>
        /// <param name="e">Парметри події</param>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            //  Метод, який скидає налаштування обєкта ManualResetEvent,
            //  щоб метод WaitOne() зупиняв роботу потоку
            Pause();
        }
        /// <summary>
        ///     Подія, яка продовжує роботу потоку
        /// </summary>
        /// <param name="sender">Кнопка, яка згенерувала подію</param>
        /// <param name="e">Параметри події</param>
        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            //  Метод, який розблокує подію WaitOne(),
            //  і завжди (поки налаштування не скинуться) не блокуватиме роботу потоку
            Resume();
        }
        /// <summary>
        ///     Подія, яка встановлє прапорець "Скасування операції" активним
        /// </summary>
        /// <param name="sender">Кнопка, яка згенерувала подію</param>
        /// <param name="e">Налаштування події</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //  Встановлення прапорця "Скасування операції" активним
            this.IsCancel = true;
        }
    }
}
