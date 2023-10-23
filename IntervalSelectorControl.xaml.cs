using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IntervalSelector
{
    public partial class IntervalSelectorControl : UserControl
    {
        private readonly Pen mainPen = new Pen(Brushes.Black, 1.0);
        private readonly Brush back = Brushes.Blue.Clone();
        private readonly Brush previewWindowBrush = Brushes.Aqua.Clone();
        private readonly Pen backPen = new Pen();
        private readonly Typeface tf = new Typeface("Lucida Console");
        private readonly DrawingGroup backingStore = new DrawingGroup();

        public IntervalSelectorControl()
        {
            back.Opacity = 0.1;
            previewWindowBrush.Opacity = 0.5;
            previewWindowBrush.Freeze();
            mainPen.Freeze();
            back.Freeze();
            backPen.Freeze();

            MouseMove += PreviewMouseMoveHandler;
            MouseWheel += MouseWheelHandler;
            SizeChanged += IntervalSelector_SizeChanged;
            Loaded += (s, a) =>
            {
                CalcScaledWidth();
                CalcGraduationsCount();
                GenerateRects();
                Render();
            };
            InitializeComponent();
        }

        #region render
        public void Render()
        {
            var drawingContext = backingStore.Open();
            Render(drawingContext);
            drawingContext.Close();
        }

        private void GenerateRects()
        {
            double onequat = ActualHeight / 4;
            textRect = new Rect(0, 0, ActualWidth, onequat);
            scaleRect = new Rect(0, onequat, ActualWidth, onequat);
            recordRect = new Rect(0, scaleRect.BottomLeft.Y, ActualWidth, onequat + onequat / 2);
            previewRect = new Rect(0, recordRect.BottomLeft.Y, ActualWidth, onequat - onequat / 2);
        }

        private void Render(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.Aqua, mainPen, textRect);

            dc.DrawRectangle(Brushes.Lime, mainPen, scaleRect);

            {
                dc.DrawLine(mainPen, scaleRect.BottomLeft, scaleRect.BottomRight);

                for (int i = -1; i < visible_graduations_count + 1; i++)
                {
                    double x = first_visible_graduation_pos + scaled_graduation_width * i;

                    dc.DrawLine(mainPen, new Point(x, scaleRect.BottomLeft.Y), new Point(x, scaleRect.TopLeft.Y));

                    int gradNumber = passed_graduations + i;
                    int second = gradNumber > 0 ? SECONDS_IN_DAY / graduations_count * gradNumber : 0;
                    (int hour, int min, int sec) time = SecondToTime(second);
                    time.sec = -1;
                    string timeString = TimeToString(time);

                    var timestamp = new FormattedText(
                        timeString,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        tf, 10, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    if (timestamp_half_width == 0d) timestamp_half_width = timestamp.Width / 2;
                    if (timestamp_half_heigth == 0d) timestamp_half_heigth = timestamp.Height / 2;

                    double tpos = textRect.Bottom - timestamp_half_heigth * 2;
                    dc.DrawText(timestamp, new Point(x - timestamp_half_width, tpos));

                    for (int j = 1; j < sub_graduations_count + 1; j++)
                    {
                        double subX = x + sub_graduation_width * j;
                        dc.DrawLine(mainPen,
                            new Point(subX, scaleRect.BottomLeft.Y),
                            new Point(subX, scaleRect.BottomLeft.Y - scaleRect.Height / 2));
                    }
                }
            }

            dc.DrawRectangle(Brushes.Yellow, mainPen, recordRect);

            #region preview
            dc.DrawRectangle(Brushes.Lime, mainPen, previewRect);

            {
                dc.DrawLine(mainPen, previewRect.BottomLeft, previewRect.BottomRight);

                double x, width = (ActualWidth / 24);
                for (int i = 0; i < 24; i++)
                {
                    x = i * width;
                    dc.DrawLine(mainPen, new Point(x, previewRect.Y), new Point(x, ActualHeight));
                }

                dc.DrawRectangle(previewWindowBrush, mainPen, new Rect(position, previewRect.Y, scaled_width, previewRect.Height));
            }
            #endregion preview
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawDrawing(backingStore);
        }
        #endregion render

        #region helpers
        private (int, int, int) SecondToTime(int second)
        {
            int hour = second / 3600;
            int min = (second - hour * 3600) / 60;
            int sec = second - hour * 3600 - min * 60;

            return (hour, min, sec);
        }

        readonly StringBuilder sb = new StringBuilder(5, 8);
        private string TimeToString((int hour, int min, int sec) time)
        {
            sb.Clear();
            if (time.hour < 10) sb.Append('0');
            sb.Append(time.hour);
            sb.Append(':');
            if (time.min < 10) sb.Append('0');
            sb.Append(time.min);
            if (time.sec != -1)
            {
                sb.Append(':');
                if (time.sec < 10) sb.Append('0');
                sb.Append(time.sec);
            }

            return sb.ToString();
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
                Position += (oldX - newX) * scale;

            oldX = newX;
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            Scale = scale + (e.Delta > 0d ? -(0.02d * scale) : e.Delta < 0 ? 0.02d * scale : 0);
        }
        #endregion interactivity
    }
}
