using System;
using System.Collections.Generic;
using System.Text;

namespace StrategyLab
{
    // =========================================================================
    // 1. DOMAIN MODEL (Модель даних)
    // =========================================================================
    public class Book
    {
        // Використовуємо 'init', щоб зробити об'єкт незмінним після створення (Immutability)
        public string Title { get; init; }
        public string Author { get; init; }
        public int Year { get; init; }
        public double Price { get; init; }

        // Виправлено порядок параметрів для логічної відповідності (Title, Author...)
        public Book(string title, string author, int year, double price)
        {
            Title = title;
            Author = author;
            Year = year;
            Price = price;
        }

        public override string ToString()
        {
            // Форматований вивід
            return $"{Author,-20} | \"{Title,-20}\" | {Year} | {Price,7:F2} UAH";
        }
    }

    // =========================================================================
    // 2. STRATEGY INTERFACE (Інтерфейс Стратегії)
    // =========================================================================
    public interface ISortStrategy
    {
        void Sort(List<Book> books);
    }

    // =========================================================================
    // 3. CONCRETE STRATEGIES (Конкретні алгоритми)
    // =========================================================================

    /// <summary>
    /// Стратегія: Сортування за Автором (А-Я).
    /// </summary>
    public class SortByAuthorStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            // Використовуємо ефективне вбудоване сортування
            books.Sort((a, b) => string.Compare(a.Author, b.Author, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Стратегія: Сортування за Роком (від нових до старих).
    /// </summary>
    public class SortByYearDescendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            // Сортування за спаданням року (b порівнюємо з a)
            books.Sort((a, b) => b.Year.CompareTo(a.Year));
        }
    }

    /// <summary>
    /// Стратегія: Сортування за Ціною (від дешевих до дорогих).
    /// </summary>
    public class SortByPriceAscendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            books.Sort((a, b) => a.Price.CompareTo(b.Price));
        }
    }

    // =========================================================================
    // 4. CONTEXT (Контекст - Бібліотека)
    // =========================================================================
    public class Library
    {
        private readonly List<Book> _books;
        private ISortStrategy _sortStrategy;

        // Подія для сповіщення UI про зміни (щоб уникнути Console.WriteLine всередині класу)
        public event EventHandler<string> OnStrategyChanged;

        public Library()
        {
            _books = new List<Book>();
            // Стратегія за замовчуванням
            _sortStrategy = new SortByAuthorStrategy(); 
        }

        public void AddBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            _books.Add(book);
        }

        /// <summary>
        /// Метод для динамічної зміни алгоритму сортування (Runtime).
        /// </summary>
        public void SetSortStrategy(ISortStrategy strategy)
        {
            // Захист від null згідно зауважень
            _sortStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            
            // Сповіщаємо підписників (Main) про зміну, замість прямого виводу в консоль
            OnStrategyChanged?.Invoke(this, strategy.GetType().Name);
        }

        public void SortBooks()
        {
            if (_books.Count == 0) return;
            
            _sortStrategy.Sort(_books);
        }

        public void ShowLibrary()
        {
            Console.WriteLine(new string('-', 70));
            Console.WriteLine($"{"Author",-20} | {"Title",-20} | {"Year"} | {"Price"}");
            Console.WriteLine(new string('-', 70));
            
            foreach (var book in _books)
            {
                Console.WriteLine(book);
            }
            Console.WriteLine(new string('-', 70) + "\n");
        }
    }

    // =========================================================================
    // 5. PROGRAM (Точка входу)
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Lab 7: Strategy Pattern (Books) ===\n");

            Library myLibrary = new Library();

            // Підписуємося на подію зміни стратегії (Decoupling UI from Logic)
            myLibrary.OnStrategyChanged += (sender, strategyName) => 
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[System] Стратегію змінено на: {strategyName}");
                Console.ResetColor();
            };

            // 1. Наповнення даними
            // ВИПРАВЛЕНО: Порядок аргументів (Title, Author, Year, Price)
            myLibrary.AddBook(new Book("Kobzar", "Shevchenko T.", 1840, 350.00));
            myLibrary.AddBook(new Book("1984", "Orwell G.", 1949, 210.50));
            myLibrary.AddBook(new Book("It", "King S.", 1986, 450.00));
            myLibrary.AddBook(new Book("Harry Potter", "Rowling J.K.", 1997, 300.00));
            myLibrary.AddBook(new Book("Zakhar Berkut", "Franko I.", 1883, 180.00));

            Console.WriteLine("--- Початковий стан (Сортування за замовчуванням: Author) ---");
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 2. Зміна стратегії -> Сортування за Роком (спадання)
            myLibrary.SetSortStrategy(new SortByYearDescendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 3. Зміна стратегії -> Сортування за Ціною (зростання)
            myLibrary.SetSortStrategy(new SortByPriceAscendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            Console.WriteLine("Програма завершена. Натисніть Enter.");
            Console.ReadLine();
        }
    }
}
