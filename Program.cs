using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyLab
{
    // ========================================================================
    // 1. DOMAIN MODEL (Варіант 6: Книги/Бібліотека)
    // ========================================================================
    #region Domain
    
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
            return $"{Author, -15} | \"{Title, -20}\" | {Year} | {Price, 7:F2} UAH";
        }
    }

    #endregion

    // =========================================================================
    // 2. STRATEGY INTERFACE
    // =========================================================================
    #region Interface

    /// <summary>
    /// Спільний інтерфейс для всіх алгоритмів сортування.
    /// </summary>
    public interface ISortStrategy
    {
        void Sort(List<Book> books);
    }

    #endregion

    // =========================================================================
    // 3. CONCRETE STRATEGIES
    // =========================================================================
    #region Strategies

    /// <summary>
    /// Стратегія 1: Сортування за Автором (алфавітний порядок).
    /// Використовує вбудований LINQ для лаконічності.
    /// </summary>
    public class SortByAuthorStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            Console.WriteLine(">> Застосовано: Сортування за АВТОРОМ (A-Z)...");
            // Сортуємо in-place
            books.Sort((a, b) => string.Compare(a.Author, b.Author, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Стратегія 2: Сортування за Роком видання (від нових до старих).
    /// Реалізовано через бульбашкове сортування (Bubble Sort) для демонстрації алгоритму.
    /// </summary>
    public class SortByYearDescendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            Console.WriteLine(">> Застосовано: Сортування за РОКОМ (Newest First) [Bubble Sort]...");
            int n = books.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (books[j].Year < books[j + 1].Year) // Порівняння для спадання
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
    /// Стратегія 3: Сортування за Ціною (від дешевих до дорогих).
    /// </summary>
    public class SortByPriceAscendingStrategy : ISortStrategy
    {
        public void Sort(List<Book> books)
        {
            Console.WriteLine(">> Застосовано: Сортування за ЦІНОЮ (Low -> High)...");
            books.Sort((a, b) => a.Price.CompareTo(b.Price));
        }
    }

    #endregion

    // =========================================================================
    // 4. CONTEXT
    // =========================================================================
    #region Context

    /// <summary>
    /// Контекст (Бібліотека), який зберігає посилання на стратегію
    /// і делегує їй виконання роботи.
    /// </summary>
    public class Library
    {
        private List<Book> _books;
        private ISortStrategy _sortStrategy;

        public Library()
        {
            _books = new List<Book>();
            // Стратегія за замовчуванням
            _sortStrategy = new SortByAuthorStrategy(); 
        }

        public void AddBook(Book book)
        {
            _books.Add(book);
        }

        public void SetSortStrategy(ISortStrategy strategy)
        {
            _sortStrategy = strategy;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[System] Стратегію змінено на: {strategy.GetType().Name}");
            Console.ResetColor();
        }

        /// <summary>
        /// Метод, що виконує сортування, не знаючи деталей алгоритму.
        /// </summary>
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
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"{"Author",-15} | {"Title",-20} | {"Year"} | {"Price"}");
            Console.WriteLine(new string('-', 60));
            
            foreach (var book in _books)
            {
                Console.WriteLine(book);
            }
            Console.WriteLine(new string('-', 60) + "\n");
        }
    }

    #endregion

    // =========================================================================
    // 5. CLIENT (Main)
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Lab 7: Strategy Pattern (Variant 6: Library) ===\n");

            // 1. Ініціалізація контексту
            Library myLibrary = new Library();

            // 2. Заповнення даними
            myLibrary.AddBook(new Book("Shevchenko T.", "Kobzar", 1840, 350.00));
            myLibrary.AddBook(new Book("Orwell G.", "1984", 1949, 210.50));
            myLibrary.AddBook(new Book("King S.", "It", 1986, 450.00));
            myLibrary.AddBook(new Book("Rowling J.K.", "Harry Potter", 1997, 300.00));
            myLibrary.AddBook(new Book("Franko I.", "Zakhar Berkut", 1883, 180.00));

            Console.WriteLine("--- Початковий стан ---");
            myLibrary.ShowLibrary();

            // 3. Використання Стратегії 1 (за замовчуванням - Автор)
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 4. Зміна стратегії на runtime -> Сортування за Роком (Bubble Sort)
            myLibrary.SetSortStrategy(new SortByYearDescendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            // 5. Зміна стратегії на runtime -> Сортування за Ціною
            myLibrary.SetSortStrategy(new SortByPriceAscendingStrategy());
            myLibrary.SortBooks();
            myLibrary.ShowLibrary();

            Console.WriteLine("Програма завершена.");
            Console.ReadLine();
        }
    }
}
