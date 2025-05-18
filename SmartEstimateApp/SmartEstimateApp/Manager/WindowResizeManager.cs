using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SmartEstimateApp.Manager
{
    /// <summary>
    /// Класс-помощник для управления изменением размеров окон с пользовательским интерфейсом
    /// </summary>
    public class WindowResizeManager
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int BORDER_WIDTH = 8;

        private Window _window;

        /// <summary>
        /// Инициализирует новый экземпляр WindowResizeManager.
        /// </summary>
        /// <param name="window">Окно, для которого включается функция изменения размера</param>
        public WindowResizeManager(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            // Убедимся, что окно имеет правильный режим изменения размера
            _window.ResizeMode = ResizeMode.CanResize;

            // Подписываемся на событие инициализации окна
            _window.SourceInitialized += Window_SourceInitialized;
        }

        /// <summary>
        /// Обработчик события инициализации источника окна
        /// </summary>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
        }

        /// <summary>
        /// Обрабатывает оконные сообщения для обеспечения изменения размера
        /// </summary>
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST && _window.ResizeMode != ResizeMode.NoResize)
            {
                // Получаем координаты курсора в экранных координатах
                Point screen = new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16);

                // Преобразуем экранные координаты в клиентские координаты
                Point client = _window.PointFromScreen(screen);

                // Получаем размеры окна
                double windowWidth = _window.ActualWidth;
                double windowHeight = _window.ActualHeight;

                // Определяем, находится ли курсор рядом с границей или углом
                bool isLeft = client.X < BORDER_WIDTH;
                bool isRight = client.X > windowWidth - BORDER_WIDTH;
                bool isTop = client.Y < BORDER_WIDTH;
                bool isBottom = client.Y > windowHeight - BORDER_WIDTH;

                // Возвращаем соответствующий идентификатор попадания в зависимости от положения курсора
                if (isTop && isLeft) { handled = true; return new IntPtr(HTTOPLEFT); }
                else if (isTop && isRight) { handled = true; return new IntPtr(HTTOPRIGHT); }
                else if (isBottom && isLeft) { handled = true; return new IntPtr(HTBOTTOMLEFT); }
                else if (isBottom && isRight) { handled = true; return new IntPtr(HTBOTTOMRIGHT); }
                else if (isLeft) { handled = true; return new IntPtr(HTLEFT); }
                else if (isRight) { handled = true; return new IntPtr(HTRIGHT); }
                else if (isTop) { handled = true; return new IntPtr(HTTOP); }
                else if (isBottom) { handled = true; return new IntPtr(HTBOTTOM); }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Перетаскивание окна
        /// </summary>
        public void DragMove()
        {
            _window.DragMove();
        }

        /// <summary>
        /// Обработка кнопки максимизации окна
        /// </summary>
        public void ToggleMaximize()
        {
            if (_window.WindowState == WindowState.Maximized)
            {
                _window.WindowState = WindowState.Normal;
            }
            else
            {
                _window.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Обработка двойного клика для развертывания окна
        /// </summary>
        public void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else
            {
                DragMove();
            }
        }

        /// <summary>
        /// Минимизация окна
        /// </summary>
        public void Minimize()
        {
            _window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        public void Close()
        {
            _window.Close();
        }
    }
}
