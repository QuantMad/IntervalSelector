using System.Collections.Generic;
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
        double scaled_window_width;        // Ширина масштабируемой области

        double graduation_width;                    // Ширина одной градации
        double sub_graduation_width;                // Ширина одной подрадации
        double scaled_graduation_width;             // Ширина масштабированной градации
        double first_visible_graduation_pos = 0d;   // Позиция на экране первой слева масштабированной градации
        int visible_graduations_count;              // Кол-во градаций попавших в окно масштаба
        int passed_graduations;                     // Кол-во градаций слева от текущей позиции масштабированного окна
        double timestamp_half_width;                // Половина ширины текста временной метки
        double timestamp_half_heigth;               // Половина высоты текста временной метки

        readonly Dictionary<int, int> gradsToSubGrads = new Dictionary<int, int>
        {
            {1440, 1},
            {288, 4},
            {96, 2},
            {48, 2},
            {24, 3},
            {12, 3}
        };

        // Квадраты для контента
        Rect textRect;
        Rect scaleRect;
        Rect recordRect;
        Rect previewRect;

        private void IntervalSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GenerateRects();
            CalcGraduationWidth();
            CalcScaledWindowWidth();
            position = position == 0 ? 0 : e.NewSize.Width / (e.PreviousSize.Width / position);
            CalcPassedGraduations();
            CalcSubGraduations();
            CalcGraduationsCount();
            CalcVisibleGraduationsCount();
            CalcFirstVisibleGraduationPos();
            Render();
        }

        public double Scale
        {
            set
            {
                if (value < .005d || value > 1d) return;
                scale = value;
                CalcScaledWindowWidth();

                double maxPos = ActualWidth - scaled_window_width;
                if (position > maxPos)
                {
                    position = maxPos;
                }

                CalcGraduationsCount();
                CalcVisibleGraduationsCount();
                CalcFirstVisibleGraduationPos();
                scaled_graduation_width = graduation_width / value;
                Render();
            }
        }

        private double Position
        {
            set
            {
                double maxPos = ActualWidth - scaled_window_width;
                position = value <= 0 ? 0 : value >= maxPos ? maxPos : value;
                CalcPassedGraduations();
                CalcFirstVisibleGraduationPos();
                Render();
            }
        }

        #region properties 
        private void CalcGraduationWidth()
        {
            graduation_width = ActualWidth / graduations_count;
            scaled_graduation_width = graduation_width / scale;
        }

        public double ScaledEnd => position + scaled_window_width;
        #endregion properties 

        #region calculators
        private void CalcPassedGraduations() =>
            passed_graduations = position == 0 ? 0 : (int)(position / (ActualWidth / graduations_count)) + 1;

        private void CalcSubGradWidth() =>
            sub_graduation_width = scaled_graduation_width / (sub_graduations_count + 1);

        private void CalcFirstVisibleGraduationPos() =>
            first_visible_graduation_pos = position > 0 ? ((ActualWidth - position) % graduation_width) / scale : 0;

        private void CalcVisibleGraduationsCount() =>
            visible_graduations_count = (int)((ScaledEnd - position) / graduation_width);

        private void CalcScaledWindowWidth() =>
            scaled_window_width = ActualWidth * scale;

        private void CalcGraduationsCount()
        {
            if (scale < 0.0051d)
            {
                // каждые 1 минуy
                graduations_count = 1440;
            }
            else if (scale < 0.059d)
            {
                // каждые 5 минут
                graduations_count = 288;
            }
            else if (scale < 0.120d)
            {
                // каждые 10 минут
                graduations_count = 144;
            }
            else if (scale < 0.198d)
            {
                // Каждые 15 минут
                graduations_count = 96;
            }
            else if (scale < 0.302d)
            {
                // каждые пол часа
                graduations_count = 48;
            }
            else if (scale < 0.594d)
            {
                // каждый час
                graduations_count = 24;
            }
            else
            { // каждые два часа
                graduations_count = 12;
            }

            CalcGraduationWidth();
            CalcPassedGraduations();
        }

        void CalcSubGraduations()
        {
            gradsToSubGrads.TryGetValue(graduations_count, out sub_graduations_count);
            CalcSubGradWidth();
        }

        #endregion calculators

        private void GenerateRects()
        {
            double onequat = ActualHeight / 4;
            textRect = new Rect(0, 0, ActualWidth, onequat);
            scaleRect = new Rect(0, onequat, ActualWidth, onequat);
            recordRect = new Rect(0, scaleRect.BottomLeft.Y, ActualWidth, onequat + onequat / 2);
            previewRect = new Rect(0, recordRect.BottomLeft.Y, ActualWidth, onequat - onequat / 2);
        }
    }
}
