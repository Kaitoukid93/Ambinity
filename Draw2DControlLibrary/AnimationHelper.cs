using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

namespace ZoomAndPan
{
    /// <summary>
    /// A helper class to simplify animation.
    /// </summary>
    internal static class AnimationHelper
    {
        /// <summary>
        /// Starts an animation to a particular value on the specified dependency property.
        /// </summary>
        public static void StartAnimation<T>(Control animatableElement, StyledProperty<T> dependencyProperty, T toValue,
            double animationDurationSeconds)
        {
            StartAnimation<T>(animatableElement, dependencyProperty, toValue, animationDurationSeconds, null);
        }

        /// <summary>
        /// Starts an animation to a particular value on the specified dependency property.
        /// You can pass in an event handler to call when the animation has completed.
        /// </summary>
        public static void StartAnimation<T>(Control animatableElement, StyledProperty<T> dependencyProperty, T toValue,
            double animationDurationSeconds, EventHandler completedEvent)
        {
            var fromValue = animatableElement.GetValue(dependencyProperty);

            var animation = CreateAnimation(dependencyProperty, fromValue, toValue, animationDurationSeconds);
            // animation.Completed += delegate (object sender, EventArgs e)
            // {
            //     //
            //     // When the animation has completed bake final value of the animation
            //     // into the property.
            //     //
            //     animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
            //     CancelAnimation(animatableElement, dependencyProperty);
            //     completedEvent?.Invoke(sender, e);
            // };
            // animation.Freeze();
            // animatableElement.BeginAnimation(dependencyProperty, animation);
            animation.PropertyChanged += delegate(object sender, AvaloniaPropertyChangedEventArgs e)
            {
                //
                // When the animation has completed bake final value of the animation
                // into the property.
                //
                switch (e.Property.Name)
                {
                    case (nameof(animation.IsAnimating)):
                        if (!animation.IsAnimating(dependencyProperty))
                        {
                            //animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
                            //CancelAnimation(animatableElement, dependencyProperty);
                            completedEvent?.Invoke(sender, e);
                        }

                        break;
                }
            };
            animation?.RunAsync(animatableElement);
        }

        /// <summary>
        /// Cancel any animations that are running on the specified dependency property.
        /// </summary>
        public static void CancelAnimation(Control animatableElement, StyledProperty<double> dependencyProperty)
        {
            //revert to default value
           // var fromValue = animatableElement.GetValue(dependencyProperty);
           // var animation = CreateAnimation(dependencyProperty, fromValue, fromValue, 0);
           // animation?.RunAsync(animatableElement);
         
           
        }


        /// <summary>
        /// Find first paretn of type T in VisualTree.
        /// </summary>
        public static T FindParentControl<T>(this AvaloniaObject control) where T : AvaloniaObject
        {
            AvaloniaObject parent = control.FindParentControl<T>();
            while (parent != null && !(parent is T))
                parent = control.FindParentControl<T>();
            return parent as T;
        }

        private static Animation CreateAnimation<T>(StyledProperty<T> dependencyProperty, T fromValue, T toValue,
            double duration)
        {
            return new Avalonia.Animation.Animation
            {
                Duration = TimeSpan.FromSeconds(duration),
                IterationCount = new IterationCount(0, IterationType.Infinite),
                PlaybackDirection = PlaybackDirection.Alternate,
                FillMode = FillMode.None,
                Delay = TimeSpan.FromSeconds(0),
                DelayBetweenIterations = TimeSpan.FromSeconds(0),
                SpeedRatio = 1d,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0),
                        Setters =
                        {
                            new Setter(dependencyProperty, fromValue),
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(duration),
                        Setters =
                        {
                            new Setter(dependencyProperty, toValue),
                        }
                    }
                }
            };
        }
    }
}