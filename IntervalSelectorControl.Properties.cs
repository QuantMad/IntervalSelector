using System.Windows;

namespace IntervalSelector
{
    public partial class IntervalSelectorControl
    {
        private static readonly int SECONDS_IN_DAY = 86400;

        double scale = 0.25d;       // Масштаб шкалы
        double position = 0;        // Позиция масштабируемой зоны
        int graduations_count;      // Общее кол-во нумеруемых градаций
        int sub_graduations_count;  // Кол-во подградаций
        double scaled_width;        // Ширина масштабируемой области

        double graduation_width;                    // Ширина одной градации
        double sub_graduation_width;                // Ширина одной подрадации
        double scaled_graduation_width;             // Ширина масштабированной градации
        double first_visible_graduation_pos = 0d;   // Позиция на экране первой слева масштабированной градации
        int visible_graduations_count;              // Кол-во градаций попавших в окно масштаба
        int passed_graduations;                     // Кол-во градаций слева от текущей позиции масштабированного окна
        double timestamp_half_width;                // Половина ширины текста временной метки
        double timestamp_half_heigth;               // Половина высоты текста временной метки

        // Квадраты для контента
        Rect textRect;
        Rect scaleRect;
        Rect recordRect;
        Rect previewRect;

        private void IntervalSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GenerateRects();
            CalcScaledWidth();
            GradStep = ActualWidth / graduations_count;
            Position = position == 0 ? 0 : e.NewSize.Width / (e.PreviousSize.Width / position);
        }

        #region properties 
        private int SubGraduations
        {
            set
            {
                sub_graduations_count = value;
                CalcSubGradWidth();
            }
        }

        private double GradStep
        {
            set
            {
                graduation_width = value;
                scaled_graduation_width = value / scale;
                CalcSubGradWidth();
                CalcFirstVisibleGraduationPos();
                CalcVisibleGraduationsCount();
            }
        }

        private int GraduationsCount
        {
            set
            {
                graduations_count = value;
                GradStep = ActualWidth / value;
            }
        }

        public double Scale
        {
            set
            {
                if (value < .005d || value > 1d) return;
                scale = value;
                CalcScaledWidth();

                if (position + scaled_width > ActualWidth)
                {
                    position = ActualWidth - scaled_width;
                    CalcFirstVisibleGraduationPos();
                }

                CalcGraduationsCount();
                GradStep = ActualWidth / graduations_count;
                scaled_graduation_width = graduation_width / value;
                Render();
            }
        }

        private double Position
        {
            get => position;
            set
            {
                double maxPos = ActualWidth - scaled_width;
                if (value < 0 || value > maxPos) return;
                position = value;
                CalcFirstVisibleGraduationPos();
                passed_graduations = value == 0 ? 0 : (int)(value / (ActualWidth / graduations_count)) + 1; ;
                Render();
            }
        }

        public double ScaledEnd => Position + scaled_width;
        #endregion properties 

        #region calculators
        private void CalcSubGradWidth() =>
            sub_graduation_width = scaled_graduation_width / (sub_graduations_count + 1);

        private void CalcFirstVisibleGraduationPos() =>
            first_visible_graduation_pos = position > 0 ? ((ActualWidth - position) % graduation_width) / scale : 0;

        private void CalcVisibleGraduationsCount() =>
            visible_graduations_count = (int)((ScaledEnd - position) / graduation_width);

        private void CalcScaledWidth() =>
            scaled_width = ActualWidth * scale;

        private void CalcGraduationsCount()
        {
            if (scale < 0.0051d)
            {
                // каждые 1 минуy
                GraduationsCount = 1440;
                SubGraduations = 1;
            }
            else if (scale < 0.059d)
            {
                // каждые 5 минут
                GraduationsCount = 288;
                SubGraduations = 4;
            }
            else if (scale < 0.120d)
            {
                // каждые 10 минут
                GraduationsCount = 144;
                SubGraduations = 9;
            }
            else if (scale < 0.198d)
            {
                // Каждые 15 минут
                GraduationsCount = 96;
                SubGraduations = 2;
            }
            else if (scale < 0.302d)
            {
                // каждые пол часа
                GraduationsCount = 48;
                SubGraduations = 2;
            }
            else if (scale < 0.594d)
            {
                // каждый час
                GraduationsCount = 24;
                SubGraduations = 3;
            }
            else
            { // каждые два часа
                GraduationsCount = 12;
                SubGraduations = 3;
            }
        }

        #endregion calculators
    }
}
