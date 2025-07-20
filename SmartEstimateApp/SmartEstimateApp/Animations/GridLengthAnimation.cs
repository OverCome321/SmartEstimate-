using System.Windows;
using System.Windows.Media.Animation;

namespace SmartEstimateApp.Animations;

public class GridLengthAnimation : AnimationTimeline
{
    public override Type TargetPropertyType => typeof(GridLength);

    protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
        "From", typeof(GridLength), typeof(GridLengthAnimation));
    public GridLength From
    {
        get => (GridLength)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
        "To", typeof(GridLength), typeof(GridLengthAnimation));
    public GridLength To
    {
        get => (GridLength)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
        "EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation));
    public IEasingFunction EasingFunction
    {
        get => (IEasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        double fromVal = From.IsAbsolute ? From.Value : ((GridLength)defaultOriginValue).Value;
        double toVal = To.IsAbsolute ? To.Value : ((GridLength)defaultDestinationValue).Value;

        if (!animationClock.CurrentProgress.HasValue)
            return new GridLength(fromVal);

        double progress = animationClock.CurrentProgress.Value;

        IEasingFunction easingFunction = EasingFunction;
        if (easingFunction != null)
        {
            progress = easingFunction.Ease(progress);
        }

        double currentVal = fromVal + (toVal - fromVal) * progress;
        return new GridLength(currentVal, GridUnitType.Pixel);
    }
}