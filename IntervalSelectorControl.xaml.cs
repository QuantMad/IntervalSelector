using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IntervalSelector
{
    /// <summary>
    /// Логика взаимодействия для IntervalSelector.xaml
    /// </summary>
    public partial class IntervalSelectorControl : UserControl
    {
        private Pen mainPen = new Pen(Brushes.Black, 1.0);
        private Brush back = Brushes.Blue.Clone();
        private Pen backPen = new Pen();
        private readonly Typeface tf = new Typeface("Lucida Console");
        private DrawingGroup backingStore = new DrawingGroup();

        public IntervalSelectorControl()
        {
            back.Opacity = 0.1;
            OnScaleChanged += ScaleChanged;
            OnPositionChanged += PositionChanged;
            MouseMove += PreviewMouseMoveHandler;
            MouseWheel += MouseWheelHandler;
            SizeChanged += IntervalSelector_SizeChanged;
            InitializeComponent();
            CalculateGraduations();
        }

        #region render
        public void Render()
        {
            var drawingContext = backingStore.Open();
            Render(drawingContext);
            drawingContext.Close();
        }

        private void Render(DrawingContext dc)
        {
            double heigthUnit = ActualHeight / 2;

            Rect textRect = new Rect(0, 0, ActualWidth, heigthUnit / 2);
            dc.DrawRectangle(Brushes.Aqua, mainPen, textRect);

            Rect scaleRect = new Rect(0, heigthUnit / 2, ActualWidth, heigthUnit / 2);
            dc.DrawRectangle(Brushes.Lime, mainPen, scaleRect);

            {
                dc.DrawLine(mainPen, scaleRect.BottomLeft, scaleRect.BottomRight);

                double gradStep = ActualWidth / GraduationsCount;
                double scaledGradSize = gradStep / Scale;
                double extraX = Position > 0 ? ((ActualWidth - Position) % gradStep) / Scale : 0;
                int visibleGraduations = (int)((ScaledEnd - Position) / gradStep);

                for (int i = -1; i < visibleGraduations + 1; i++)
                {
                    double x = extraX + scaledGradSize * i;

                    dc.DrawLine(mainPen, new Point(x, scaleRect.BottomLeft.Y), new Point(x, scaleRect.TopLeft.Y));

                    var time = CalculateTimeForGraduation(PassedGraduations + i);
                    string timeString = TimeToString(time);

                    var t = new FormattedText(
                        timeString,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        tf, 10, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(t, new Point(x - t.Width / 2, textRect.Bottom - t.Height - t.Height / 2));

                    for (int j = 1; j < SubGraduations + 1; j++)
                    {
                        double subX = x + scaledGradSize / (SubGraduations + 1) * j;
                        dc.DrawLine(mainPen,
                            new Point(subX, scaleRect.BottomLeft.Y),
                            new Point(subX, scaleRect.BottomLeft.Y - scaleRect.Height / 2));
                    }
                }
            }

            Rect recordRect = new Rect(0, scaleRect.BottomLeft.Y, ActualWidth, heigthUnit / 3 * 2);
            dc.DrawRectangle(Brushes.Yellow, mainPen, recordRect);

            #region preview
            Rect previewRect = new Rect(0, recordRect.BottomLeft.Y, ActualWidth, heigthUnit / 3);
            dc.DrawRectangle(Brushes.Lime, mainPen, previewRect);

            {
                double infoY = previewRect.Y;
                dc.DrawLine(mainPen, previewRect.BottomLeft, previewRect.BottomRight);

                double x, w = (ActualWidth / 24);
                for (int i = 0; i < 24; i++)
                {
                    x = i * w;
                    dc.DrawLine(mainPen, new Point(x, infoY), new Point(x, ActualHeight));
                }

                Brush b = Brushes.Aqua.Clone();
                b.Opacity = 0.5;
                dc.DrawRectangle(b, mainPen, new Rect(Position, infoY, ActualWidth * Scale, previewRect.Height));
            }
            #endregion preview
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Render();
            dc.DrawDrawing(backingStore);
        }
        #endregion render

        #region helpers
        private (int, int) CalculateTimeForGraduation(int graduationNumber)
        {
            int totalSecond = SECONDS_IN_DAY / GraduationsCount * graduationNumber;
            (int h, int m, _) = SecondToTime(totalSecond);

            return (h, m);
        }

        private (int, int, int) SecondToTime(int second)
        {
            int hour = second / 3600;
            int min = (second - hour * 3600) / 60;
            int sec = second - hour * 3600 - min * 60;

            return (hour, min, sec);
        }

        private static string TimeToString((int hour, int min) time)
        {
            string hStr = time.hour < 10 ? $"0{time.hour}" : $"{time.hour}";
            string mStr = time.min < 10 ? $"0{time.min}" : $"{time.min}";

            return $"{hStr}:{mStr}";
        }
        #endregion helpers

        #region interactivity
        double oldX = -1;
        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            double newX = e.MouseDevice.GetPosition(sender as IInputElement).X;

            if (oldX == -1)
            {
                oldX = newX;
                return;
            }

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                double delta = oldX - newX;
                Position += delta * Scale;
                Debug.WriteLine(SecondToTime(ViewportPosition));
            }

            oldX = newX;
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            Scale += (e.Delta > 0d ? -(0.02d * Scale) : e.Delta < 0 ? 0.02d * Scale : 0);
        }

        private void IntervalSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Position = Position == 0 ? 0 : e.NewSize.Width / (e.PreviousSize.Width / Position);
        }
        #endregion interactivity
    }
}
