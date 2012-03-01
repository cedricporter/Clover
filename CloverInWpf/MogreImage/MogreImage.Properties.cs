using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Mogre;

namespace MogreInWpf
{
    public partial class MogreImage
    {
        #region IsDebugOverlayVisible

        private bool _isDebugOverlayVisible; // Non-DP field in case DP's GetValue each frame slows performance

        /// <summary>
        /// IsDebugOverlayVisible Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsDebugOverlayVisibleProperty =
            DependencyProperty.Register("IsDebugOverlayVisible", typeof(bool), typeof(MogreImage),
                new FrameworkPropertyMetadata((bool)false,
                    FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(OnIsDebugOverlayVisibleChanged)));

        /// <summary>
        /// Gets or sets the IsDebugOverlayVisible property. This dependency property 
        /// indicates whether the Ogre debug overlay is visible and updated.
        /// </summary>
        public bool IsDebugOverlayVisible
        {
            get { return (bool)GetValue(IsDebugOverlayVisibleProperty); }
            set { SetValue(IsDebugOverlayVisibleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsDebugOverlayVisible property.
        /// </summary>
        private static void OnIsDebugOverlayVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MogreImage mogreImage = (MogreImage)d;
            bool oldIsDebugOverlayVisible = (bool)e.OldValue;
            bool newIsDebugOverlayVisible = mogreImage.IsDebugOverlayVisible;
            mogreImage.OnIsDebugOverlayVisibleChanged(oldIsDebugOverlayVisible, newIsDebugOverlayVisible);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsDebugOverlayVisible property.
        /// </summary>
        protected virtual void OnIsDebugOverlayVisibleChanged(bool oldIsDebugOverlayVisible, bool newIsDebugOverlayVisible)
        {
            _isDebugOverlayVisible = newIsDebugOverlayVisible;

            var overlay = Mogre.OverlayManager.Singleton.GetByName("Core/DebugOverlay");

            if (newIsDebugOverlayVisible)
            {
                overlay.Show();
            }
            else
            {
                overlay.Hide();
            }
        }        

        #endregion

        #region ViewportSize Property

        public static readonly DependencyProperty ViewportSizeProperty =
            DependencyProperty.Register("ViewportSize", typeof(Size), typeof(MogreImage),
                                        new PropertyMetadata(new Size(100, 100), OnViewportProperyChanged)
                );

        private static void OnViewportProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageSource = (MogreImage)d;

            imageSource._reloadRenderTargetTime = Environment.TickCount;
        }

        public Size ViewportSize
        {
            get { return (Size)GetValue(ViewportSizeProperty); }
            set { SetValue(ViewportSizeProperty, value); }
        }

        #endregion

        #region AutoInitialise Property

        public static readonly DependencyProperty AutoInitialiseProperty =
            DependencyProperty.Register("AutoInitialise", typeof(bool), typeof(MogreImage),
                                        new PropertyMetadata(false));

        public bool AutoInitialise
        {
            get { return (bool)GetValue(AutoInitialiseProperty); }
            set { SetValue(AutoInitialiseProperty, value); }
        }

        #endregion

        #region CreateDefaultScene Property

        public static readonly DependencyProperty CreateDefaultSceneProperty =
            DependencyProperty.Register("CreateDefaultScene", typeof(bool), typeof(MogreImage),
                                        new PropertyMetadata(true));

        public bool CreateDefaultScene
        {
            get { return (bool)GetValue(CreateDefaultSceneProperty); }
            set { SetValue(CreateDefaultSceneProperty, value); }
        }

        #endregion

        #region ResizeRenderTargetDelay Property

        public static readonly DependencyProperty ResizeRenderTargetDelayProperty =
            DependencyProperty.Register("ResizeRenderTargetDelay", typeof(Duration), typeof(MogreImage),
            new PropertyMetadata(new Duration(new TimeSpan(200))));

        public Duration ResizeRenderTargetDelay
        {
            get { return (Duration)GetValue(ResizeRenderTargetDelayProperty); }
            set { SetValue(ResizeRenderTargetDelayProperty, value); }
        }

        #endregion

        #region FrameRate Property

        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(int?), typeof(MogreImage),
            new PropertyMetadata(FrameRate_Changed));

        private static void FrameRate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {            
            ((MogreImage)d).OnFrameRateChanged((int?)e.NewValue);
        }

        public int? FrameRate
        {
            get { return (int?)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        #endregion

    }
}
