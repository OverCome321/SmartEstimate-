namespace UI.Animations;

public class GridLengthAnimation : AnimationTimeline
{
    // WPF требует, чтобы мы переопределили это свойство.
    public override Type TargetPropertyType
    {
        get { return typeof(GridLength); }
    }

    // WPF требует, чтобы мы создали новый "свободный" (незамороженный) экземпляр.
    protected override Freezable CreateInstanceCore()
    {
        return new GridLengthAnimation();
    }

    // Это свойство для хранения начального значения анимации.
    public GridLength From
    {
        get { return (GridLength)GetValue(FromProperty); }
        set { SetValue(FromProperty, value); }
    }

    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

    // Это свойство для хранения конечного значения анимации.
    public GridLength To
    {
        get { return (GridLength)GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));


    // ГЛАВНЫЙ МЕТОД: здесь происходит вся магия интерполяции.
    // Мы вычисляем текущее значение GridLength на основе прошедшего времени (progress).
    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        // Получаем начальное значение. Если не задано, берем текущее значение анимируемого свойства.
        double fromVal = ((GridLength)GetValue(FromProperty)).Value;
        if (fromVal == 0) // Если From не задано явно
        {
            fromVal = ((GridLength)defaultOriginValue).Value;
        }

        // Получаем конечное значение. Если не задано, берем текущее значение.
        double toVal = ((GridLength)GetValue(ToProperty)).Value;
        if (toVal == 0) // Если To не задано явно
        {
            toVal = ((GridLength)defaultDestinationValue).Value;
        }

        // Получаем прогресс анимации (от 0.0 до 1.0).
        // Если анимация остановилась (null), возвращаем конечное значение.
        if (!animationClock.CurrentProgress.HasValue)
        {
            return new GridLength(toVal, GridUnitType.Pixel);
        }

        double progress = animationClock.CurrentProgress.Value;

        // Простая линейная интерполяция
        double currentVal = fromVal + (toVal - fromVal) * progress;

        // Возвращаем новое значение GridLength. Важно использовать GridUnitType.Pixel.
        return new GridLength(currentVal, GridUnitType.Pixel);
    }
}
