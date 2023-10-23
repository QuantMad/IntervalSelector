using System.ComponentModel;
using System.Windows;

namespace IntervalSelector
{
    public partial class IntervalSelectorControl
    {
        private static readonly int SECONDS_IN_DAY = 86400;

        public delegate void IntervalSelectorEventHandler(IntervalSelectorControl sender, double oldVal, double newVal);
        public event IntervalSelectorEventHandler OnScaleChanged;
        public event IntervalSelectorEventHandler OnPositionChanged;

        private int SubGraduations;

        private int PassedGraduations => Position == 0 ? 0 : (int)(Position / (ActualWidth / GraduationsCount)) + 1;

        double scaled_grad_size;
        double grad_step;
        private double GradStep
        {
            get => grad_step;
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

        private void ScaleChanged(IntervalSelectorControl sender, double oldVal, double newVal)
        {
            if (Position + ScaledWidth > ActualWidth)
                Position = ActualWidth - ScaledWidth;
            
            CalculateGraduations();
            scaled_grad_size = grad_step / Scale;
            Render();
        }

        private void PositionChanged(IntervalSelectorControl sender, double oldVal, double newVal)
        {
            Render();
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

        [Description("Текущий масштаб шкалы (0-1"), Category("Common Properties")]
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set { if (value != 0) { SetValue(ScaleProperty, value); } }
        }

        //double position = 0;
        private int ViewportPosition
        {
            get
            {
                return (int)(SECONDS_IN_DAY / (ActualWidth / Position));
            }
            set
            {
                Position = ActualWidth / (SECONDS_IN_DAY / value);
                //ViewportPosition = (int)(SECONDS_IN_DAY / (ActualWidth / position));
            }
        }
        [Description("Позиция интервала"), Category("Common Properties")]
        private double Position
        {
            get => (double)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(IntervalSelectorControl),
            new PropertyMetadata(0.25d,
                (s, v) =>
                {
                    IntervalSelectorControl sender = (s as IntervalSelectorControl);
                    sender.OnScaleChanged?.Invoke(sender, (double)v.OldValue, (double)v.NewValue);
                },
                (_, v) =>
                {
                    double val = (double)v;
                    return val >= 1d ? 1d : val < .005d ? .005d : val;
                }));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(double), typeof(IntervalSelectorControl),
            new PropertyMetadata(0d,
                (s, v) =>
                {
                    IntervalSelectorControl sender = (s as IntervalSelectorControl);
                    sender.OnPositionChanged?.Invoke(sender, (double)v.OldValue, (double)v.NewValue);
                },
                (s, v) =>
                {
                    IntervalSelectorControl sender = s as IntervalSelectorControl;
                    double val = (double)v;
                    double maxPos = sender.ActualWidth - sender.ScaledWidth;
                    return val > maxPos ? maxPos : val <= 0d ? 0d : val;
                }));


        public double ScaledWidth => ActualWidth * Scale;

        public double ScaledEnd => Position + ScaledWidth;
    }
}
