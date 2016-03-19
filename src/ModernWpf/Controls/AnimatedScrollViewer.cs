﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    // modified from http://matthiasshapiro.com/2009/05/06/how-to-create-an-animated-scrollviewer-or-listbox-in-wpf/

    /// <summary>
    /// A scroll viewer that will animate where possible (currently on mouse wheel + click only).
    /// </summary>
    [TemplatePart(Name = "PART_AniVerticalScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_AniHorizontalScrollBar", Type = typeof(ScrollBar))]
    public class AnimatedScrollViewer : ScrollViewer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static AnimatedScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedScrollViewer), new FrameworkPropertyMetadata(typeof(AnimatedScrollViewer)));
        }

        const string PART_RealVBar = "PART_VerticalScrollBar";
        const string PART_RealHBar = "PART_HorizontalScrollBar";
        const string PART_FakeVBar = "PART_AniVerticalScrollBar";
        const string PART_FakeHBar = "PART_AniHorizontalScrollBar";

        const int smallStep = 16;
        const int largeStep = 48;
        ScrollBar _aniVerticalScrollBar;
        ScrollBar _aniHorizontalScrollBar;
        ScrollBar _realVerticalScrollBar;
        ScrollBar _realHorizontalScrollBar;
        bool _isHAnimating;
        bool _isVAnimating;
        bool _isHOverride;
        bool _isVOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedScrollViewer"/> class.
        /// </summary>
        public AnimatedScrollViewer()
        {
            UIHooks.AddMouseHWheelHandler(this, HandleHWheel);
        }

        private void HandleHWheel(object sender, MouseWheelEventArgs e)
        {
            if (e != null && !e.Handled)
            {
                if (e.Delta < 0)
                {
                    if (this.CanHScrollLeft())
                    {
                        TargetHorizontalOffset = Math.Max(0, HorizontalOffset - largeStep);
                        e.Handled = true;
                    }
                }
                else
                {
                    if (this.CanHScrollRight())
                    {
                        TargetHorizontalOffset = Math.Min(ScrollableWidth, HorizontalOffset + largeStep);
                        e.Handled = true;
                    }
                }
                if (e.Handled)
                {
                    AnimateNow();
                }
            }
        }


        #region ScrollViewer Override Methods

        /// <summary>
        /// Called when an internal process or application calls <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />, which is used to build the current template's visual tree.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            _realVerticalScrollBar = GetTemplateChild(PART_RealVBar) as ScrollBar;
            if (_realVerticalScrollBar != null)
            {
                _realVerticalScrollBar.ValueChanged += _realVerticalScrollBar_ValueChanged;
            }

            _realHorizontalScrollBar = GetTemplateChild(PART_RealHBar) as ScrollBar;
            if (_realHorizontalScrollBar != null)
            {
                _realHorizontalScrollBar.ValueChanged += _realHorizontalScrollBar_ValueChanged;
            }

            _aniVerticalScrollBar = GetTemplateChild(PART_FakeVBar) as ScrollBar;
            if (_aniVerticalScrollBar != null)
            {
                _aniVerticalScrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(AniVScrollBar_ValueChanged);
            }

            _aniHorizontalScrollBar = GetTemplateChild(PART_FakeHBar) as ScrollBar;
            if (_aniHorizontalScrollBar != null)
            {
                _aniHorizontalScrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(AniHScrollBar_ValueChanged);
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e != null && !e.Handled)
            {
                if (e.Delta < 0)
                {
                    if (this.CanVScrollDown())
                    {
                        TargetVerticalOffset = Math.Min(ScrollableHeight, VerticalOffset + largeStep);
                        e.Handled = true;
                    }
                    else if (this.CanHScrollRight() && ScrollViewerUI.GetSimulateHWheel(this))
                    {
                        TargetHorizontalOffset = Math.Min(ScrollableWidth, HorizontalOffset + largeStep);
                        e.Handled = true;
                    }
                }
                else
                {
                    if (this.CanVScrollUp())
                    {
                        TargetVerticalOffset = Math.Max(0, VerticalOffset - largeStep);
                        e.Handled = true;
                    }
                    else if (this.CanHScrollLeft() && ScrollViewerUI.GetSimulateHWheel(this))
                    {
                        TargetHorizontalOffset = Math.Max(0, HorizontalOffset - largeStep);
                        e.Handled = true;
                    }
                }
                if (e.Handled)
                {
                    AnimateNow();
                }
            }

            base.OnPreviewMouseWheel(e);
        }

        /// <summary>
        /// Responds to specific keyboard input and invokes associated scrolling behavior.
        /// </summary>
        /// <param name="e">Required arguments for this event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e !=null && Keyboard.Modifiers == ModifierKeys.None && CanKeyboardScroll)
            {
                //Debug.WriteLine("Got keydown override");
                switch (e.Key)
                {
                    case Key.Down:
                        if (this.CanVScrollDown())
                        {
                            TargetVerticalOffset = NormalizeScrollPos((VerticalOffset + smallStep), Orientation.Vertical);
                            e.Handled = true;
                        }
                        break;
                    case Key.PageDown:
                        if (this.CanVScrollDown())
                        {
                            TargetVerticalOffset = NormalizeScrollPos((VerticalOffset + ViewportHeight), Orientation.Vertical);
                            e.Handled = true;
                        }
                        break;
                    case Key.Up:
                        if (this.CanVScrollUp())
                        {
                            TargetVerticalOffset = NormalizeScrollPos((VerticalOffset - smallStep), Orientation.Vertical);
                            e.Handled = true;
                        }
                        break;
                    case Key.PageUp:
                        if (this.CanVScrollUp())
                        {
                            TargetVerticalOffset = NormalizeScrollPos((VerticalOffset - ViewportHeight), Orientation.Vertical);
                            e.Handled = true;
                        }
                        break;
                    case Key.Left:
                        if (this.CanHScrollLeft())
                        {
                            TargetHorizontalOffset = NormalizeScrollPos((HorizontalOffset - smallStep), Orientation.Horizontal);
                            e.Handled = true;
                        }
                        break;
                    case Key.Right:
                        if (this.CanHScrollRight())
                        {
                            TargetHorizontalOffset = NormalizeScrollPos((HorizontalOffset + smallStep), Orientation.Horizontal);
                            e.Handled = true;
                        }
                        break;
                }
                if (e.Handled)
                {
                    AnimateNow();
                }
            }
            base.OnKeyDown(e);
        }

        private double NormalizeScrollPos(double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > ScrollableHeight)
            {
                returnValue = ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > ScrollableWidth)
            {
                returnValue = ScrollableWidth;
            }

            return returnValue;
        }


        #endregion

        #region Custom Event Handlers

        // if the real scroll bar changed outside of animation then also update the fake scrollbars

        void _realHorizontalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isHAnimating)
            {
                _isHOverride = true;
                TargetHorizontalOffset = e.NewValue;
                _isHOverride = false;
            }
        }

        void _realVerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isVAnimating)
            {
                _isVOverride = true;
                TargetVerticalOffset = e.NewValue;
                _isVOverride = false;
            }
        }

        void AniVScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double oldTargetVOffset = (double)e.OldValue;
            double newTargetVOffset = (double)e.NewValue;

            if (newTargetVOffset != TargetVerticalOffset)
            {
                double deltaVOffset = Math.Round((newTargetVOffset - oldTargetVOffset), 3);

                if (deltaVOffset == 1)
                {
                    TargetVerticalOffset = oldTargetVOffset + ViewportHeight;

                }
                else if (deltaVOffset == -1)
                {
                    TargetVerticalOffset = oldTargetVOffset - ViewportHeight;
                }
                else if (deltaVOffset == 0.1)
                {
                    TargetVerticalOffset = oldTargetVOffset + 16.0;
                }
                else if (deltaVOffset == -0.1)
                {
                    TargetVerticalOffset = oldTargetVOffset - 16.0;
                }
                else
                {
                    TargetVerticalOffset = newTargetVOffset;
                }
            }
        }

        void AniHScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double oldTargetHOffset = (double)e.OldValue;
            double newTargetHOffset = (double)e.NewValue;

            if (newTargetHOffset != TargetHorizontalOffset)
            {

                double deltaVOffset = Math.Round((newTargetHOffset - oldTargetHOffset), 3);

                if (deltaVOffset == 1)
                {
                    TargetHorizontalOffset = oldTargetHOffset + ViewportWidth;

                }
                else if (deltaVOffset == -1)
                {
                    TargetHorizontalOffset = oldTargetHOffset - ViewportWidth;
                }
                else if (deltaVOffset == 0.1)
                {
                    TargetHorizontalOffset = oldTargetHOffset + 16.0;
                }
                else if (deltaVOffset == -0.1)
                {
                    TargetHorizontalOffset = oldTargetHOffset - 16.0;
                }
                else
                {
                    TargetHorizontalOffset = newTargetHOffset;
                }
            }
        }

        #endregion

        #region Custom Dependency Properties

        #region TargetVerticalOffset (DependencyProperty)(double)

        /// <summary>
        /// This is the VerticalOffset that we'd like to animate to
        /// </summary>
        public double TargetVerticalOffset
        {
            get { return (double)GetValue(TargetVerticalOffsetProperty); }
            set { SetValue(TargetVerticalOffsetProperty, value); }
        }
        /// <summary>
        /// The dependency property for <see cref="TargetVerticalOffset"/>.
        /// </summary>
        public static readonly DependencyProperty TargetVerticalOffsetProperty =
            DependencyProperty.Register("TargetVerticalOffset", typeof(double), typeof(AnimatedScrollViewer),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnTargetVerticalOffsetChanged)));

        private static void OnTargetVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)d;

            if ((double)e.NewValue != thisScroller._aniVerticalScrollBar.Value)
            {
                thisScroller._aniVerticalScrollBar.Value = (double)e.NewValue;
            }
            if (!thisScroller._isVOverride)
            {
                thisScroller.AnimateNow();
            }
        }

        #endregion

        #region TargetHorizontalOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the HorizontalOffset that we'll be animating to
        /// </summary>
        public double TargetHorizontalOffset
        {
            get { return (double)GetValue(TargetHorizontalOffsetProperty); }
            set { SetValue(TargetHorizontalOffsetProperty, value); }
        }
        /// <summary>
        /// The dependency property for <see cref="TargetHorizontalOffset"/>.
        /// </summary>
        public static readonly DependencyProperty TargetHorizontalOffsetProperty =
            DependencyProperty.Register("TargetHorizontalOffset", typeof(double), typeof(AnimatedScrollViewer),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnTargetHorizontalOffsetChanged)));

        private static void OnTargetHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)d;

            if ((double)e.NewValue != thisScroller._aniHorizontalScrollBar.Value)
            {
                thisScroller._aniHorizontalScrollBar.Value = (double)e.NewValue;
            }

            if (!thisScroller._isHOverride)
            {
                thisScroller.AnimateNow();
            }
        }

        #endregion

        #region HorizontalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual horizontal offset property we're going use as an animation helper
        /// </summary>
        public double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }
        /// <summary>
        /// The dependency property for <see cref="HorizontalScrollOffset"/>.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalScrollOffsetChanged)));

        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer)d;
            thisSViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }

        #endregion

        #region VerticalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual VerticalOffset we're going to use as an animation helper
        /// </summary>
        public double VerticalScrollOffset
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }
        /// <summary>
        /// The dependency property for <see cref="VerticalScrollOffset"/>.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalScrollOffsetChanged)));

        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer)d;
            thisSViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        #endregion


        #region Animation properties

        /// <summary>
        /// Gets or sets a value indicating whether scrolling will be animated (if supported).
        /// </summary>
        /// <value>
        ///   <c>true</c> if to animate scrolling; otherwise, <c>false</c>.
        /// </value>
        public bool AnimateScroll
        {
            get { return (bool)GetValue(AnimateScrollProperty); }
            set { SetValue(AnimateScrollProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="AnimateScroll"/>.
        /// </summary>
        public static readonly DependencyProperty AnimateScrollProperty =
            DependencyProperty.Register("AnimateScroll", typeof(bool), typeof(AnimatedScrollViewer), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));



        /// <summary>
        /// A property for changing the time it takes to scroll to a new
        /// position.
        /// </summary>
        /// <value>
        /// The duration to animate.
        /// </value>
        public TimeSpan AnimateDuration
        {
            get { return (TimeSpan)GetValue(AnimateDurationProperty); }
            set { SetValue(AnimateDurationProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="AnimateDuration"/>.
        /// </summary>
        public static readonly DependencyProperty AnimateDurationProperty =
            DependencyProperty.Register("AnimateDuration", typeof(TimeSpan), typeof(AnimatedScrollViewer), new FrameworkPropertyMetadata(new TimeSpan(0, 0, 0, 0, 250)));

        /// <summary>
        /// A property to allow users to describe a custom spline for the scrolling Animations.
        /// </summary>
        public KeySpline AnimateSpline
        {
            get { return (KeySpline)GetValue(AnimateSplineProperty); }
            set { SetValue(AnimateSplineProperty, value); }
        }

        /// <summary>
        /// The dependency property for <see cref="AnimateSpline"/>.
        /// </summary>
        public static readonly DependencyProperty AnimateSplineProperty =
            DependencyProperty.Register("AnimateSpline", typeof(KeySpline), typeof(AnimatedScrollViewer),
              new FrameworkPropertyMetadata(new KeySpline(0.024, 0.914, 0.717, 1)));

        #endregion

        #region CanKeyboardScroll (Dependency Property)

        /// <summary>
        /// The dependency property for <see cref="CanKeyboardScroll"/>.
        /// </summary>
        public static readonly DependencyProperty CanKeyboardScrollProperty =
            DependencyProperty.Register("CanKeyboardScroll", typeof(bool), typeof(AnimatedScrollViewer), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether common keyboard navigation keys are used to animate the scroll.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to detect keyboard keys; otherwise, <c>false</c>.
        /// </value>
        public bool CanKeyboardScroll
        {
            get { return (bool)GetValue(CanKeyboardScrollProperty); }
            set { SetValue(CanKeyboardScrollProperty, value); }
        }

        #endregion



        #endregion

        private void AnimateNow()
        {
            if (AnimateScroll && Animations.ShouldAnimate)
            {
                KeyTime targetKeyTime = AnimateDuration;
                KeySpline targetKeySpline = AnimateSpline;

                DoubleAnimationUsingKeyFrames animateHScrollKeyFramed = new DoubleAnimationUsingKeyFrames();
                animateHScrollKeyFramed.Completed += (s, e) => { _isHAnimating = false; };
                DoubleAnimationUsingKeyFrames animateVScrollKeyFramed = new DoubleAnimationUsingKeyFrames();
                animateVScrollKeyFramed.Completed += (s, e) => { _isVAnimating = false; };

                SplineDoubleKeyFrame HScrollKey1 = new SplineDoubleKeyFrame(TargetHorizontalOffset, targetKeyTime, targetKeySpline);
                SplineDoubleKeyFrame VScrollKey1 = new SplineDoubleKeyFrame(TargetVerticalOffset, targetKeyTime, targetKeySpline);
                animateHScrollKeyFramed.KeyFrames.Add(HScrollKey1);
                animateVScrollKeyFramed.KeyFrames.Add(VScrollKey1);

                _isHAnimating = true;
                BeginAnimation(HorizontalScrollOffsetProperty, animateHScrollKeyFramed);

                _isVAnimating = true;
                BeginAnimation(VerticalScrollOffsetProperty, animateVScrollKeyFramed);

                //CommandBindingCollection testCollection = CommandBindings;
                //int blah = testCollection.Count;
            }
            else
            {
                _isHAnimating = true;
                HorizontalScrollOffset = TargetHorizontalOffset;
                _isHAnimating = false;

                _isVAnimating = true;
                VerticalScrollOffset = TargetVerticalOffset;
                _isVAnimating = false;
            }

        }

        #region my old stuff

        //#region animation parts

        //bool _isVanimating;
        //bool _isHanimating;

        //private void AnimateHorizontal(double newOffset)
        //{
        //    if (_isHanimating) { return; }
        //    Debug.WriteLine("Scroll horizontal to " + newOffset);
        //    if (!AnimateScroll || SystemParameters.IsRemoteSession)
        //    {
        //        BindableHorizontalOffset = newOffset;
        //    }
        //    else
        //    {
        //        _isHanimating = true;
        //        var hStory = new Storyboard();
        //        hStory.Completed += (s, e) => { _isHanimating = false; };
        //        var hAnime = CreateDoubleAnimation(BindableHorizontalOffsetProperty);
        //        hStory.Children.Add(hAnime);
        //        hAnime.To = newOffset;
        //        BeginStoryboard(hStory, HandoffBehavior.SnapshotAndReplace);
        //    }
        //}

        //private void AnimateVertical(double newOffset)
        //{
        //    if (_isVanimating) { return; }
        //    Debug.WriteLine("Scroll vertical to " + newOffset);
        //    if (!AnimateScroll || SystemParameters.IsRemoteSession)
        //    {
        //        BindableVerticalOffset = newOffset;
        //    }
        //    else
        //    {
        //        _isVanimating = true;
        //        var vStory = new Storyboard();
        //        vStory.Completed += (s, e) => { _isVanimating = false; };
        //        var vAnime = CreateDoubleAnimation(BindableVerticalOffsetProperty);
        //        vStory.Children.Add(vAnime);
        //        vAnime.To = newOffset;
        //        BeginStoryboard(vStory, HandoffBehavior.SnapshotAndReplace);
        //    }
        //}

        //private DoubleAnimation CreateDoubleAnimation(DependencyProperty property)
        //{
        //    DoubleAnimation da = new DoubleAnimation
        //    {
        //        Duration = TimeSpan.FromSeconds(.25),
        //        //EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut }
        //        EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
        //    };
        //    Storyboard.SetTargetProperty(da, new PropertyPath(property));
        //    return da;
        //}

        //#endregion

        #endregion



    }
}
