using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Domoto.Helpers
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From", typeof(GridLength), typeof(GridLengthAnimation));

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        public IEasingFunction EasingFunction { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
                return From;

            var fromVal = From.IsAbsolute ? From.Value : ((GridLength)defaultOriginValue).Value;
            var toVal = To.IsAbsolute ? To.Value : ((GridLength)defaultDestinationValue).Value;
            var progress = animationClock.CurrentProgress.Value;

            if (EasingFunction != null)
                progress = EasingFunction.Ease(progress);

            return new GridLength(fromVal + (toVal - fromVal) * progress, GridUnitType.Pixel);
        }
    }
}
