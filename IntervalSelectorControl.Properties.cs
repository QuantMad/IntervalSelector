namespace IntervalSelector
{
    public partial class IntervalSelectorControl
    {
        private static readonly int SECONDS_IN_DAY = 86400;

        private int SubGraduations;

        private int PassedGraduations => Position == 0 ? 0 : (int)(Position / (ActualWidth / GraduationsCount)) + 1;

        double scaled_grad_size;
        double grad_step;
        private double GradStep
        {
            //get => grad_step;
            set
            {
                grad_step = value;
                scaled_grad_size = value / Scale;
            }
        }
        int graduations_count;
        private int GraduationsCount
        {
            get => graduations_count;
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

                if (Position + ScaledWidth > ActualWidth) /// TODO: cached maxPos
                    Position = ActualWidth - ScaledWidth;

                CalculateGraduations();
                GradStep = ActualWidth / GraduationsCount;
                scaled_grad_size = grad_step / Scale;

                Render();
            }
        }

        private double _position = 0;
        private double Position
        {
            get => _position;
            set
            {
                double maxPos = ActualWidth - ScaledWidth; /// TODO: cache
                if (value < 0 || value > maxPos) return;
                _position = value;
                Render();
            }
        }

        public double ScaledWidth => ActualWidth * Scale;

        public double ScaledEnd => Position + ScaledWidth;
    }
}
