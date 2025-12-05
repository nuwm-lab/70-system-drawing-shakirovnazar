using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GraphStrategyLab
{
    // =========================================================================
    // 1. STRATEGY INTERFACE
    // =========================================================================
    /// <summary>
    /// Інтерфейс стратегії: визначає, ЯКУ функцію ми будуємо.
    /// </summary>
    public interface IFunctionStrategy
    {
        string Name { get; }
        double Calculate(double x);
    }

    // =========================================================================
    // 2. CONCRETE STRATEGIES
    // =========================================================================

    /// <summary>
    /// Основна стратегія (Варіант 6): y = (3x + 1) / arctg(x)
    /// </summary>
    public class LabVariantStrategy : IFunctionStrategy
    {
        public string Name => "y = (3x + 1) / arctg(x)";

        public double Calculate(double x)
        {
            // Обробка особливої точки x -> 0, де arctg(x) -> 0
            if (Math.Abs(x) < 1e-4) 
                return double.NaN; // Позначаємо як "не число", щоб не малювати лінію в нескінченність

            return (3 * x + 1) / Math.Atan(x);
        }
    }

    /// <summary>
    /// Додаткова стратегія для тестування (синусоїда).
    /// </summary>
    public class SinStrategy : IFunctionStrategy
    {
        public string Name => "y = sin(x) * 5";
        public double Calculate(double x) => Math.Sin(x) * 5;
    }

    // =========================================================================
    // 3. RENDERER (Логіка малювання)
    // =========================================================================
    /// <summary>
    /// Клас, що відповідає за перетворення координат і малювання.
    /// Відокремлює логіку Graphics від форми.
    /// </summary>
    public class GraphRenderer
    {
        private float _minX = -10f;
        private float _maxX = 10f;
        private float _minY = -20f;
        private float _maxY = 20f;

        public void Draw(Graphics g, int width, int height, IFunctionStrategy strategy)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // 1. Розрахунок коефіцієнтів масштабування
            // Відступаємо по 20 пікселів з країв
            float margin = 40; 
            float drawWidth = width - 2 * margin;
            float drawHeight = height - 2 * margin;

            if (drawWidth <= 0 || drawHeight <= 0) return;

            // Коефіцієнти: скільки пікселів в одній одиниці координат
            float scaleX = drawWidth / (_maxX - _minX);
            float scaleY = drawHeight / (_maxY - _minY);

            // Функції перетворення координат (Світ -> Екран)
            float ToScreenX(double x) => margin + (float)((x - _minX) * scaleX);
            float ToScreenY(double y) => margin + drawHeight - (float)((y - _minY) * scaleY); // Y інвертуємо

            // 2. Малювання осей
            using (var penAxis = new Pen(Color.Black, 2))
            using (var font = new Font("Arial", 8))
            using (var brush = new SolidBrush(Color.Black))
            {
                // Вісь X (y = 0)
                float y0 = ToScreenY(0);
                if (y0 >= margin && y0 <= height - margin)
                    g.DrawLine(penAxis, margin, y0, width - margin, y0);

                // Вісь Y (x = 0)
                float x0 = ToScreenX(0);
                if (x0 >= margin && x0 <= width - margin)
                    g.DrawLine(penAxis, x0, margin, x0, height - margin);
                
                // Підписи меж
                g.DrawString(_minX.ToString(), font, brush, margin, y0 + 5);
                g.DrawString(_maxX.ToString(), font, brush, width - margin - 20, y0 + 5);
            }

            // 3. Малювання графіка функції
            using (var penGraph = new Pen(Color.Blue, 2))
            {
                // Крок дискретизації. Чим менше, тим плавніше.
                // Робимо крок залежним від пікселів (щоб завжди було плавно)
                double step = (_maxX - _minX) / drawWidth; 
                
                PointF? prevPoint = null;

                for (double x = _minX; x <= _maxX; x += step)
                {
                    double y = strategy.Calculate(x);

                    // Якщо точка не існує (NaN) або виходить далеко за межі Y (асимптота)
                    if (double.IsNaN(y) || double.IsInfinity(y) || Math.Abs(y) > _maxY * 2)
                    {
                        prevPoint = null; // Розрив лінії
                        continue;
                    }

                    // Перевірка на межі відображення (clipping), щоб не малювати за межами екрану
                    if (y < _minY || y > _maxY)
                    {
                        prevPoint = null;
                        continue;
                    }

                    float scrX = ToScreenX(x);
                    float scrY = ToScreenY(y);

                    PointF currentPoint = new PointF(scrX, scrY);

                    if (prevPoint.HasValue)
                    {
                        // З'єднуємо попередню точку з поточною
                        g.DrawLine(penGraph, prevPoint.Value, currentPoint);
                    }

                    prevPoint = currentPoint;
                }
            }
        }
    }

    // =========================================================================
    // 4. MAIN FORM (Контекст / UI)
    // =========================================================================
    public class MainForm : Form
    {
        private IFunctionStrategy _currentStrategy;
        private readonly GraphRenderer _renderer;
        private readonly ComboBox _strategySelector;

        public MainForm()
        {
            // Налаштування форми
            this.Text = "Lab 7: Strategy Pattern (Graphing)";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true; // Усуває миготіння при зміні розміру (вимога критики)
            this.MinimumSize = new Size(400, 300);

            // Ініціалізація компонентів
            _renderer = new GraphRenderer();
            
            // За замовчуванням - варіант 6
            _currentStrategy = new LabVariantStrategy();

            // UI для перемикання стратегій
            var panel = new Panel { Dock = DockStyle.Top, Height = 40 };
            
            _strategySelector = new ComboBox { 
                Left = 10, Top = 8, Width = 200, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            
            _strategySelector.Items.Add(new LabVariantStrategy()); // Варіант 6
            _strategySelector.Items.Add(new SinStrategy());        // Тест
            
            _strategySelector.DisplayMember = "Name";
            _strategySelector.SelectedIndex = 0; // Вибираємо перший елемент

            _strategySelector.SelectedIndexChanged += (s, e) =>
            {
                // Зміна стратегії (Runtime)
                if (_strategySelector.SelectedItem is IFunctionStrategy strategy)
                {
                    _currentStrategy = strategy;
                    this.Invalidate(); // Викликає перерисовку
                }
            };

            panel.Controls.Add(new Label { Text = "Function:", Left = 220, Top = 12, AutoSize = true });
            panel.Controls.Add(_strategySelector);
            this.Controls.Add(panel);
        }

        // Подія перерисовки (стандартний метод WinForms)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Делегуємо малювання рендереру, передаючи поточну стратегію
            _renderer.Draw(e.Graphics, this.ClientSize.Width, this.ClientSize.Height, _currentStrategy);
        }

        // При зміні розміру форми просто викликаємо перерисовку
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }

    // =========================================================================
    // 5. PROGRAM ENTRY POINT
    // =========================================================================
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
