using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UmaFanCountChecker
{
    /// <summary>
    /// SliderBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SliderBox : UserControl
    {
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum),
                            typeof(double),
                            typeof(SliderBox),
                            new FrameworkPropertyMetadata(1.0,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum),
                            typeof(double),
                            typeof(SliderBox),
                            new FrameworkPropertyMetadata(0.0,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value),
                            typeof(double),
                            typeof(SliderBox),
                            new FrameworkPropertyMetadata(0.0,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register(nameof(Interval),
                            typeof(double),
                            typeof(SliderBox),
                            new FrameworkPropertyMetadata(0.01,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SliderBox()
        {
            InitializeComponent();
        }

        public double Interval
        {
            get { return (double)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

    }
}
