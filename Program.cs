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
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }

        public Book(string title, string author, int year, double price)
        {
            Title = title;
            Author = author;
            Year = year;
            Price = price;
        }

        public override string ToString()
        {
            // Форматований вивід: Author (15 символів), Title (20 символів) і т.д.
            return $"{Author,-15} | \"{Title,-20}\" | {Year} | {Price,7:F2} UAH";
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
            Console.WriteLine(">> [Strategy] Сортування за АВТОРОМ (A-Z)...");
            books.Sort((a, b) => string.Compare(a.Author, b.Author, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Стратегія: Сортування за Роком (від нових до старих).
    /// Реалізація через алгоритм Bubble Sort (для демонстрації).
    /// </summary>
    public class SortByYearDescendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            Console.WriteLine(">> [Strategy] Сортування за РОКОМ (спочатку нові)...");
            int n = books.Count;
            // Бульбашкове сортування
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (books[j].Year < books[j + 1].Year) // Змінити знак на >, щоб було навпаки
                    {
                        // Swap
                        var temp = books[j];
                        books[j] = books[j + 1];
                        books[j + 1] = temp;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Стратегія: Сортування за Ціною (від дешевих до дорогих).
    /// </summary>
    public class SortByPriceAscendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            Console.WriteLine(">> [Strategy] Сортування за ЦІНОЮ (Low -> High)...");
            books.Sort((a, b) => a.Price.CompareTo(b.Price));
        }
    }

    // =========================================================================
    // 4. CONTEXT (Контекст - Бібліотека)
    // =========================================================================
    public class Library
    {
        private List<Book> _books;
        private ISortStrategy _sortStrategy;

        public Library()
        {
            _books = new List<Book>();
            // Встановлюємо стратегію за замовчуванням, щоб уникнути NullReferenceException
            _sortStrategy = new SortByAuthorStrategy(); 
        }

        public void AddBook(Book book)
        {
            _books.Add(book);
        }

        /// <summary>
        /// Метод для динамічної зміни алгоритму сортування (Runtime).
        /// </summary>
        public void SetSortStrategy(ISortStrategy strategy)
        {
            _sortStrategy = strategy;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[System] Стратегію змінено на: {strategy.GetType().Name}");
            Console.ResetColor();
        }

        public void SortBooks()
        {
            if (_books.Count == 0)
            {
                Console.WriteLine("Бібліотека порожня.");
                return;
            }
            // Делегування виконання конкретній стратегії
            _sortStrategy.Sort(_books);
        }

        public void ShowLibrary()
        {
            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"{"Author",-15} | {"Title",-20} | {"Year"} | {"Price"}");
            Console.WriteLine(new string('-', 65));
            
            foreach (var book in _books)
            {
                Console.WriteLine(book);
            }
            Console.WriteLine(new string('-', 65) + "\n");
        }
    }

    // =========================================================================
    // 5. PROGRAM (Точка входу)
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного відображення символів
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Lab 7: Strategy Pattern (Books) ===\n");

            Library myLibrary = new Library();

            // Наповнення даними
            myLibrary.AddBook(new Book("Shevchenko T.", "Kobzar", 1840, 350.00));
            myLibrary.AddBook(new Book("Orwell G.", "1984", 1949, 210.50));
            myLibrary.AddBook(new Book("King S.", "It", 1986, 450.00));
            myLibrary.AddBook(new Book("Rowling J.K.", "Harry Potter", 1997, 300.00));
            myLibrary.AddBook(new Book("Franko I.", "Zakhar Berkut", 1883, 180.00));

            Console.WriteLine("--- Початковий стан ---");
            myLibrary.ShowLibrary();

            // 1. Використання стратегії за замовчуванням (Author)
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 2. Зміна стратегії на сортування за Роком
            myLibrary.SetSortStrategy(new SortByYearDescendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 3. Зміна стратегії на сортування за Ціною
            myLibrary.SetSortStrategy(new SortByPriceAscendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            Console.WriteLine("Програма завершена. Натисніть Enter.");
            Console.ReadLine();
        }
    }
}
