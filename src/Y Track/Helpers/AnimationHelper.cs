using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Y_Track.Helpers
{
    public class AnimationHelper
    {

        public static void AnimateOpacity(DependencyObject target, double from, double to)
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            Storyboard.SetTarget(opacityAnimation, target);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Control.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();
        }
    }
}
