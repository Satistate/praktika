using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf; // Убедитесь, что вы используете правильный namespace для PackIcon

public class ButtonAnimator
{
    private ScaleTransform scaleTransform;

    public void OnMouseEnter(Button button)
    {
        AnimateButton(button, 1.2, 90); // Увеличиваем размер на 20% и вращаем на 180 градусов
    }

    public void OnMouseLeave(Button button)
    {
        AnimateButton(button, 1.0, -90); // Возвращаем размер к 100% и вращаем на -180 градусов
    }

    private void AnimateButton(Button button, double scale, double angle)
    {
        // Устанавливаем точку привязки в центр кнопки
        button.RenderTransformOrigin = new Point(0.5, 0.5);

        // Если RenderTransform еще не установлен, создаем ScaleTransform и RotateTransform
        TransformGroup transformGroup = button.RenderTransform as TransformGroup;

        if (transformGroup == null)
        {
            scaleTransform = new ScaleTransform();
            RotateTransform rotateTransform = new RotateTransform();

            // Объединяем трансформации в одну
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            button.RenderTransform = transformGroup;
        }

        // Получаем текущие трансформации
        ScaleTransform currentScaleTransform = (ScaleTransform)transformGroup.Children[0];
        RotateTransform currentRotateTransform = (RotateTransform)transformGroup.Children[1];

        // Создаем анимацию для изменения масштаба
        DoubleAnimation scaleXAnimation = new DoubleAnimation
        {
            From = currentScaleTransform.ScaleX,
            To = scale,
            Duration = TimeSpan.FromMilliseconds(400), // Длительность анимации
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut } // Функция сглаживания
        };

        DoubleAnimation scaleYAnimation = new DoubleAnimation
        {
            From = currentScaleTransform.ScaleY,
            To = scale,
            Duration = TimeSpan.FromMilliseconds(400), // Длительность анимации
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut } // Функция сглаживания
        };

        // Применяем анимации к ScaleTransform
        currentScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
        currentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);

        // Создаем анимацию для вращения
        DoubleAnimation rotateAnimation = new DoubleAnimation
        {
            From = currentRotateTransform.Angle,
            To = angle,
            Duration = TimeSpan.FromMilliseconds(700), // Длительность анимации
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut } // Функция сглаживания
        };

        // Применяем анимацию к RotateTransform
        currentRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
    }
}