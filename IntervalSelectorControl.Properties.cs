using System.Windows;

namespace IntervalSelector
{
    public partial class IntervalSelectorControl
    {
        private void IntervalSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            textRectHeigth = ActualHeight / 4;
            GradStep = ActualWidth / graduations_count;
            Position = _position == 0 ? 0 : e.NewSize.Width / (e.PreviousSize.Width / _position);
            CalcFirstVisibleGraduationPos();
            calcScaledWidth();
        }

        double scaled_width;
        private static readonly int SECONDS_IN_DAY = 86400;
        double textRectHeigth;// = ActualHeight / 4;
        private int SubGraduations;

        private int PassedGraduations => Position == 0 ? 0 : (int)(Position / (ActualWidth / graduations_count)) + 1;

        double firstVisibleGraduationPos = 0d;

        int visibleGraduations = 0;

        private void CalcFirstVisibleGraduationPos()
        {
            firstVisibleGraduationPos = _position > 0 ? ((ActualWidth - _position) % grad_step) / _scale : 0;
        }

        private void CalculateVisibleGraduations()
        {
            visibleGraduations = (int)((ScaledEnd - _position) / grad_step);
        }

        double scaled_grad_size;
        double grad_step;
        private double GradStep
        {
            set
            {
                grad_step = value;
                scaled_grad_size = value / Scale;
                CalcFirstVisibleGraduationPos();
                CalculateVisibleGraduations();
            }
        }
        int graduations_count;
        private int GraduationsCount
        {
            set
            {
                graduations_count = value;
                GradStep = ActualWidth / value;
            }
        }

        private void CalculateGraduations()
        {
            if (Scale < 0.0051d)
            {
                // каждые 1 минуy
                GraduationsCount = 1440;
                SubGraduations = 1;
            }
            else if (Scale < 0.059d)
            {
                // каждые 5 минут
                GraduationsCount = 288;
                SubGraduations = 4;
            }
            else if (Scale < 0.120d)
            {
                // каждые 10 минут
                GraduationsCount = 144;
                SubGraduations = 9;
            }
            else if (Scale < 0.198d)
            {
                // Каждые 15 минут
                GraduationsCount = 96;
                SubGraduations = 2;
            }
            else if (Scale < 0.302d)
            {
                // каждые пол часа
                GraduationsCount = 48;
                SubGraduations = 2;
            }
            else if (Scale < 0.594d)
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

        private double _scale = 0.25d;
        public double Scale
        {
            get => _scale;
            set
            {
                if (value < .005d || value > 1d) return;
                _scale = value;
                calcScaledWidth();
                if (Position + scaled_width > ActualWidth) /// TODO: cached maxPos
                    Position = ActualWidth - scaled_width;

                CalculateGraduations();
                GradStep = ActualWidth / graduations_count;
                scaled_grad_size = grad_step / _scale;
                Render();
            }
        }

        private double _position = 0;
        private double Position
        {
            get => _position;
            set
            {
                double maxPos = ActualWidth - scaled_width; /// TODO: cache
                if (value < 0 || value > maxPos) return;
                _position = value;
                CalcFirstVisibleGraduationPos();
                Render();
            }
        }
        
        private void calcScaledWidth()
        {
            scaled_width = ActualWidth * _scale;
        }

        public double ScaledEnd => Position + scaled_width;
    }
}
