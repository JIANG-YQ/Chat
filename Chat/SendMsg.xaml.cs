using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Chat
{
    /// <summary>
    /// SendMsg.xaml 的交互逻辑
    /// </summary>
    public partial class SendMsg : UserControl
    {
        public SendMsg()
        {
            InitializeComponent();
        }

        private void LoadAnimation(object sender, RoutedEventArgs e)
        {
            Name = "msgLabel";
            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName(Name, this);
            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.From = 0;
            widthAnimation.To = ActualWidth + 10;
            widthAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            DoubleAnimation heightAnimation = new DoubleAnimation();
            heightAnimation.From = 0;
            heightAnimation.To = ActualHeight;
            heightAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            Storyboard.SetTargetName(widthAnimation, Name);
            Storyboard.SetTargetName(heightAnimation, Name);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(SendMsg.WidthProperty));
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(SendMsg.HeightProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(widthAnimation);
            storyboard.Children.Add(heightAnimation);
            storyboard.Begin(this);
        }
    }
}
