
#region Timer selection

// This timer is used when FrameRate is specified.  (Otherwise, the WPF Rendering event will trigger rendering.)

// Uncomment one of the following #defines:

#define DispatcherTimer //

// [Jared: The TimersTimer requires dispatching to the dispatcher, so you might as well just use the DispatcherTimer.]
//#define TimersTimer // System.Timers.Timer

#endregion

#region Interop selection

// Uses the approach described here: http://www.codeproject.com/KB/WPF/OgreInTheWpf.aspx?msg=3319704#xx3319704xx
// If this works ok, the MogreWpf.Interop DLL reference can be removed.
// Currently, it doesn't seem to work, since the RenderSystem.Listener.EventOccurredHandler is not fired.

#define NoInteropDll

#endregion

#define MultiViewport

using System;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Mogre;
using System.Collections.Generic;
using System.Diagnostics;

#if TimersTimer
using TimerClass = System.Timers.Timer;
#endif

#if DispatcherTimer
using TimerClass = System.Windows.Threading.DispatcherTimer;
#endif

namespace MogreInWpf
{
    public partial class MogreImage : D3DImage, ISupportInitialize
    {

        

        #region (Static) Render Windows

        private static object renderWindowsLock = new object();
        private static Dictionary<IntPtr, RenderWindow> renderWindows = new Dictionary<IntPtr, RenderWindow>();

        #endregion

        #region Mogre Fields

        public Root Root
        {
            get { return root; }
            set { root = value; }
        } private Root root;

        public SceneManager SceneManager
        {
            get { return _sceneManager; }
            set { _sceneManager = value; }
        } private SceneManager _sceneManager;

        private TexturePtr _texture;

        private RenderWindow _renderWindow;
        private RenderTarget _renTarget;

        #region Mogre Scene

        private SceneNode _ogreNode;
        private Entity _ogreMesh;

        private void AddDefaultSceneObjects()
        {
            Light l = _sceneManager.CreateLight("MainLight");
            l.Position = new Vector3(20F, 80F, 50F);

            // load the "ogre head mesh" resource.
            _ogreMesh = _sceneManager.CreateEntity("ogre", "ogrehead.mesh");

            // create a node for the "ogre head mesh"
            _ogreNode = _sceneManager.RootSceneNode.CreateChildSceneNode("ogreNode");
            _ogreNode.AttachObject(_ogreMesh);

            // Note: currently the ogre node is rotated at a fixed rate per render as a visual aid to seeing how many
            // frames are being rendered.  For a more typical time-based rotation, consider using the commented
            // stopwatch code.

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            this.PreRender += new EventHandler((o, a) =>
            {
                //sw.Stop();
                //_ogreNode.Rotate(Vector3.UNIT_Y, ((float)sw.ElapsedMilliseconds / 500f));
                _ogreNode.Rotate(Vector3.UNIT_Y, 0.01f);
                //sw.Reset();
                //sw.Start();
            });
        }

        #endregion

        #endregion

        #region (Private) Fields

        private int _reloadRenderTargetTime;
        private bool _imageSourceValid;
        private bool _eventAttatched;

        private Thread _currentThread;
        private TimerClass _timer;

        #endregion

        #region Ogre Resource Naming

        public string RenderWindowName
        {
            get { return renderWindowName; }
        } private string renderWindowName;
        private static int renderWindowCounter = 0;
        private static object renderWindowCounterLock = new object();

        public int instanceId;
        private static int sceneManagerCounter = 0;
        private static object sceneManagerCounterLock = new object();

        #endregion

        #region Ogre Debug Overlay elements

        private OverlayElement avgFpsElement;
        private OverlayElement lastFpsElement;
        private OverlayElement bestFpsElement;
        private OverlayElement worstFpsElement;
        private OverlayElement triangleCountElement;
        private OverlayElement batchCountElement;

        #endregion

        #region Initialization / Deinitialization
        
        #region ISupportInitialize Members

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (AutoInitialise)
            {
                InitOgreImage();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            IsFrontBufferAvailableChanged -= _isFrontBufferAvailableChanged;

            DetachRenderTarget(true, true);

            if (_currentThread != null)
            {
                _currentThread.Abort();
            }

            //MogreImageUtils.DisposeRoot(Root); - should only be done when shutting down
            if (root != null)
            {
                DisposeRenderTarget();
                CompositorManager.Singleton.RemoveAll(); // REVIEW
                this.Root = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Initialization

        protected bool _InitOgreImage()
        {
            lock (this)
            {

                #region Get Root

                if (root == null)
                {
                    root = MogreRootManager.GetSharedRoot();
                }

                if (root == null)
                {
                    throw new Exception("Root must be set.  See MogreUtils.CreateRoot() or MogreImage.AutoCreateRoot");
                }

                #endregion

                #region Create RenderWindow using hWnd

                lock (renderWindowsLock)
                {
                    if (_renderWindow == null)  // For now, try only one renderwindow.  TODO: Try multiple windows, dual monitor support etc.
                    {
                        IntPtr hWnd = IntPtr.Zero;

                        foreach (PresentationSource source in PresentationSource.CurrentSources)
                        {
                            var hwndSource = (source as HwndSource);
                            if (hwndSource != null)
                            {
                                hWnd = hwndSource.Handle;
                                break;
                            }
                        }

                        if (hWnd == IntPtr.Zero)
                        {
                            throw new Exception("Failed to get hWnd from PresentationSource.CurrentSources.");
                        }

                        if (!renderWindows.TryGetValue(hWnd, out _renderWindow))
                        {
                            renderWindowName = "MogreImage-" + renderWindowCounter++;

                            var misc = new NameValuePairList();
                            misc["externalWindowHandle"] = hWnd.ToString();
                            _renderWindow = root.CreateRenderWindow(renderWindowName, 0, 0, false, misc);
                            _renderWindow.IsAutoUpdated = false;

                            renderWindows.Add(hWnd, _renderWindow);
                        }

                        // FUTURE REVIEW: Try multiple viewports
                        //  - if there is tearing, try tweaking the vsync, vsyncInterval in misc
                        //  - Is parentWindowHandle needed? 

                    }
                }
                #endregion

#if NoInteropDll
                root.RenderSystem.EventOccurred += new RenderSystem.Listener.EventOccurredHandler(RenderSystem_EventOccurred);
#endif

                this.Dispatcher.Invoke(
                    (Action)delegate
                                       {
                                           if (CreateDefaultScene)
                                           {
                                               string sceneManagerName;
                                               string cameraName;
                                               lock (sceneManagerCounterLock)
                                               {
                                                   sceneManagerName = "SceneManager-" + sceneManagerCounter;
                                                   cameraName = "Camera-" + sceneManagerCounter;
                                                   sceneManagerCounter++;
                                               }

                                               //----------------------------------------------------- 
                                               // 4 Create the SceneManager
                                               // 
                                               //		ST_GENERIC = octree
                                               //		ST_EXTERIOR_CLOSE = simple terrain
                                               //		ST_EXTERIOR_FAR = nature terrain (depreciated)
                                               //		ST_EXTERIOR_REAL_FAR = paging landscape
                                               //		ST_INTERIOR = Quake3 BSP
                                               //----------------------------------------------------- 
                                               _sceneManager = root.CreateSceneManager(SceneType.ST_GENERIC, sceneManagerName);
                                               _sceneManager.AmbientLight = new ColourValue(0.5f, 0.5f, 0.5f);

                                               //----------------------------------------------------- 
                                               // 5 Create the camera 
                                               //----------------------------------------------------- 
                                               Camera camera;
                                               camera = _sceneManager.CreateCamera(cameraName);
                                               camera.Position = new Vector3(0f, 0f, 80f);
                                               // Look back along -Z
                                               camera.LookAt(new Vector3(0f, 0f, -300f));
                                               camera.NearClipDistance = 5;

                                               ViewportDefinitions = new ViewportDefinition[]
                                               {
                                                   new ViewportDefinition(camera),
                                               };


                                               AddDefaultSceneObjects();
                                           }


                                           IsFrontBufferAvailableChanged += _isFrontBufferAvailableChanged;

                                           var ev = Initialised; if (ev != null) ev(this, new RoutedEventArgs());

                                           ReInitRenderTarget();
                                           AttachRenderTarget(true);

                                           OnFrameRateChanged(this.FrameRate);

                                           _currentThread = null;
                                       });

                //avgFpsElement = OverlayManager.Singleton.GetOverlayElement("Core/AverageFps");
                //lastFpsElement = OverlayManager.Singleton.GetOverlayElement("Core/CurrFps");
                //bestFpsElement = OverlayManager.Singleton.GetOverlayElement("Core/BestFps");
                //worstFpsElement = OverlayManager.Singleton.GetOverlayElement("Core/WorstFps");

                //triangleCountElement = OverlayManager.Singleton.GetOverlayElement("Core/NumTris");
                //batchCountElement = OverlayManager.Singleton.GetOverlayElement("Core/NumBatches");


                return true;
            }
        }
        
        public bool InitOgreImage()
        {
            return _InitOgreImage();
        }

        public Thread InitOgreAsync(ThreadPriority priority, RoutedEventHandler completeHandler)
        {
            if (completeHandler != null)
                Initialised += completeHandler;

            _currentThread = new Thread(() => _InitOgreImage())
                                 {
                                     Name = "InitMogreImage",
                                     Priority = priority
                                 };
            _currentThread.Start();

            return _currentThread;
        }

        public void InitOgreAsync()
        {
            InitOgreAsync(ThreadPriority.Normal, null);
        }

        #endregion

        #region Reinit

        protected void ReInitRenderTarget()
        {
            DetachRenderTarget(true, false);
            DisposeRenderTarget();

            _texture = TextureManager.Singleton.CreateManual(
                "OgreImageSource RenderTarget",
                ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME,
                TextureType.TEX_TYPE_2D,
                (uint)ViewportSize.Width, (uint)ViewportSize.Height,
                0, Mogre.PixelFormat.PF_A8R8G8B8,
                (int)TextureUsage.TU_RENDERTARGET);

            _renTarget = _texture.GetBuffer().GetRenderTarget();

            _reloadRenderTargetTime = 0;

            int viewportCount = ViewportDefinitions.Length;
            viewports = new Viewport[viewportCount];

            for (int i = 0; i < viewportCount; i++)
            {
                Viewport viewport;
                ViewportDefinition vd = ViewportDefinitions[i];
                viewport = _renTarget.AddViewport(vd.Camera, zIndexCounter++, vd.Left, vd.Top, vd.Width, vd.Height);
                viewport.BackgroundColour = vd.BackgroundColour;
                viewports[i] = viewport;
            }

            var ev = ViewportsChanged;
            if (ev != null) ev();

            viewportDefinitionsChanged = false;
        }
        
        #endregion
        
        #region Deinit

        protected void DisposeRenderTarget()
        {
            try
            {
                if (_renTarget != null)
                {
                    if (viewports == null) return;
                    int viewportCount = viewports.Length;
                    for (int i = 0; i < viewportCount; i++)
                    {
                        if (viewports[i] != null)
                        {
                            Viewport viewport = viewports[i];
                            CompositorManager.Singleton.RemoveCompositorChain(viewport);
                        }
                    }
                    viewports = null;

                    _renTarget.RemoveAllListeners();
                    _renTarget.RemoveAllViewports();
                    root.RenderSystem.DestroyRenderTarget(_renTarget.Name);
                    _renTarget = null;
                }

                if (_texture != null)
                {
                    TextureManager.Singleton.Remove(_texture.Handle);
                    _texture.Dispose();
                    _texture = null;
                }
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("草泥马的又死了");
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        #endregion
        
        #region Attach / Detach Render Target

        protected virtual void AttachRenderTarget(bool attachEvent)
        {
            if (!_imageSourceValid)
            {
                Lock();
                try
                {
                    IntPtr surface;
                    _renTarget.GetCustomAttribute("DDBACKBUFFER", out surface);
                    SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface);

                    _imageSourceValid = true;
                }
                finally
                {
                    Unlock();
                }
            }

            if (attachEvent)
                UpdateEvents(true);
        }

        protected virtual void DetachRenderTarget(bool detatchSurface, bool detatchEvent)
        {
            if (detatchSurface && _imageSourceValid)
            {
                Lock();
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                Unlock();

                _imageSourceValid = false;
            }

            if (detatchEvent)
                UpdateEvents(false);
        }

        protected virtual void UpdateEvents(bool attach)
        {
            _eventAttatched = attach;
            if (attach)
            {
                if (!ManualRender)
                {
                    if (_timer != null)
                    {
#if DispatcherTimer
                        _timer.Tick += _eventRender;
#endif
#if TimersTimer
                        _timer.Elapsed += _eventRenderDispatch;
#endif
                    }
                    else
                    {
                        CompositionTarget.Rendering += _eventRender;
                    }
                }
            }
            else
            {
                if (_timer != null)
                {
#if DispatcherTimer
                    _timer.Tick -= _eventRender;
#endif
#if TimersTimer
                    _timer.Elapsed -= _eventRenderDispatch;
#endif
                }
                else
                {
                    CompositionTarget.Rendering -= _eventRender;
                }
            }
        }

        private void _isFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsFrontBufferAvailable)
                AttachRenderTarget(true); // might not succeed
            else
                // need to keep old surface attached because it's the only way to get the front buffer active event.
                DetachRenderTarget(false, true);
        }

        #endregion
        
        #region Frame Rate

        private void OnFrameRateChanged(int? newFrameRate)
        {
            bool wasAttached = _eventAttatched;
            UpdateEvents(false);

            if (newFrameRate == null)
            {
                if (_timer != null)
                {
#if TimersTimer
                    _timer.Elapsed -= _eventRenderDispatch;
#endif
#if DispatcherTimer
                    _timer.Tick -= _eventRender;
#endif
                    _timer.Stop();
                    _timer = null;
                }
            }
            else
            {
                if (_timer == null)
                    //_timer = new DispatcherTimer();
                    _timer = new TimerClass();

#if TimersTimer
                _timer.Interval = ((double)1000) / newFrameRate.Value;
#endif
#if DispatcherTimer
                _timer.Interval = TimeSpan.FromMilliseconds(1000 / newFrameRate.Value);
#endif

                _timer.Start();
            }

            if (wasAttached)
                UpdateEvents(true);
        }

        #endregion

        #endregion

#if NoInteropDll

        #region Device Lost Support

        private bool isDeviceLost;

        // Catch Ogre RenderSystem events to detect "DeviceLost" and DeviceRestored events
        void RenderSystem_EventOccurred(string eventName, Const_NameValuePairList parameters)
        {
            if (eventName.Equals("DeviceLost"))
            {
                this.isDeviceLost = true;
            }
            else if (eventName.Equals("DeviceRestored"))
            {
                this.isDeviceLost = false;
            }
        }
        #endregion

#endif

        #region (Public) Events

        public event RoutedEventHandler Initialised;
        public event EventHandler PreRender;
        public event EventHandler PostRender;

        #endregion

        #region Viewports

        #region Viewport Definitions

        /// <summary>
        /// Status flag to indicate that viewports should be recreated at next opportunity
        /// </summary>
        private bool viewportDefinitionsChanged;

        public ViewportDefinition[] ViewportDefinitions
        {
            get { return viewportDefinitions; }
            set
            {
                viewportDefinitionsChanged = true;
                viewportDefinitions = value;
            }
        } private ViewportDefinition[] viewportDefinitions;

        #endregion

        #region Mogre Viewports

        /// <summary>
        /// Get a viewport that was created based on ViewportDefinitions.
        /// Warning: The ViewportsChanged event indicates when the Viewport returned may have become invalid.  Listen for this
        /// event to know when to get new versons of the viewports.
        /// </summary>
        /// <param name="index">The index corresponding to the ViewportDefinition in ViewportDefinitions</param>
        /// <returns>The Mogre Viewport corresponding to the ViewportDefinition in ViewportDefinitions</returns>
        public Viewport GetViewport(int index)
        {
            return viewports[index];
        }

        /// <summary>
        /// The private array of viewports in use.
        /// </summary>
        private Viewport[] viewports;

        /// <summary>
        /// Indicates that viewports have been recreated.  If you got a viewport via GetViewport, you will likely have to get it again.
        /// </summary>
        public event Action ViewportsChanged;

        #endregion

        #endregion

        #region (Private) Rendering
        
        // BUG  - I want to reset zIndexCounter to 0 every time viewports are reset, but for some reason Ogre is not liking that (maybe not deregistering old viewports?)
        int zIndexCounter = 0;
        protected void _RenderFrame()
        {
            if (root == null)
                return;

#if OLD_MultiViewport
            int viewportIndex = 0;
            foreach (var kvp in cameras.ToArray())
            {
                // REVIEW this.  Does it have to be in the render loop?
                if (kvp.Viewport == null)
                {
#if OLDER_SingleViewport
             //if ((_camera != null) && (_viewport == null))
            //{
            //    _viewport = _renTarget.AddViewport(_camera);
            //    _viewport.BackgroundColour = new ColourValue(0.0f, 0.0f, 0.0f, 0.0f);
            //}
#endif
                    Viewport viewport;
                    if(viewportIndex == 0)
                    {
                        if (cameras.Count == 1)
                        {
                            viewport = _renTarget.AddViewport(kvp.Camera, zIndexCounter++);
                        }
                        else
                        {
                            viewport = _renTarget.AddViewport(kvp.Camera, zIndexCounter++, 0f, 0f, 0.5f, 0.5f);
                        }                        
                    }
                    else if(viewportIndex == 1)
                    {
                        viewport = _renTarget.AddViewport(kvp.Camera, zIndexCounter++, 0.5f, 0f, 0.5f, 0.5f);
                    }
                    else if (viewportIndex == 2)
                    {
                        viewport = _renTarget.AddViewport(kvp.Camera, zIndexCounter++, 0f, 0.5f, 0.5f, 0.5f);
                    }
                    else if (viewportIndex == 3)
                    {
                        viewport = _renTarget.AddViewport(kvp.Camera, zIndexCounter++, 0.5f, 0.5f, 0.5f, 0.5f);
                    }
                    else
                    {
                        throw new Exception("Max of 4 viewports supported.");
                    }
                    
                    viewport.BackgroundColour = new ColourValue(0.0f, 0.0f, 0.0f, 0.0f);

                    kvp.Viewport = viewport;
                }
                viewportIndex++;
            }
#else
            // Moved to RenderFrame()
#endif

            var preRenderCopy = PreRender; if (preRenderCopy != null) preRenderCopy(this, EventArgs.Empty);


            //if (IsMaster) // TEMP - Bug: events aren't really pre/post
            //{
                root.RenderOneFrame();
            //}
            //else
            //{
            //    //MessageBox.Show("test");
            //}


            //_renTarget.Update(false);

            if (_isDebugOverlayVisible) UpdateDebugOverlayStats();

            var postRenderCopy = PostRender; if (postRenderCopy != null) postRenderCopy(this, EventArgs.Empty);
        }

        private void UpdateDebugOverlayStats()
        {
            float avgFps;
            float lastFps;
            float bestFps;
            float worstFps;

            _renTarget.GetStatistics(out lastFps, out avgFps, out bestFps, out worstFps);

            var statistics = _renTarget.GetStatistics();

#if true
            avgFpsElement.Caption = "Average: " + statistics.AvgFPS;
            lastFpsElement.Caption = "Current: " + statistics.LastFPS;
            bestFpsElement.Caption = "Best: " + statistics.BestFPS;
            worstFpsElement.Caption = "Worst: " + statistics.WorstFPS;

            triangleCountElement.Caption = "Triangle Count: " + statistics.TriangleCount;
            batchCountElement.Caption = "Batch Count: " + statistics.BatchCount;
#else
            avgFpsElement.Caption = "Average: " + avgFps;
            lastFpsElement.Caption = "Current: " + lastFps;
            bestFpsElement.Caption = "Best: " + bestFps;
            worstFpsElement.Caption = "Worst: " + worstFps;
#endif
        }

        private void _eventRender(object sender, EventArgs e)
        {
            //if (!manualRender)
            {
                RenderFrame();
            }
        }

        #endregion

        #region (Public) Manual Rendering

        public void RenderFrame()
        {
            if (root == null) return;

            if (!IsFrontBufferAvailable) return;

            if (_renderWindow == null) return;

#if NoInteropDll
            if (this.isDeviceLost)
            {
                _renderWindow.Update(); // try restore
                _reloadRenderTargetTime = -1;
            }

            if (this.isDeviceLost) return;
#else
            if (MogreWpf.Interop.D3D9RenderSystem.IsDeviceLost(_renderWindow))
            {
                _renderWindow.Update(); // try restore
                _reloadRenderTargetTime = -1;

                if (MogreWpf.Interop.D3D9RenderSystem.IsDeviceLost(_renderWindow))
                    return;
            }
#endif

            if (viewportDefinitionsChanged)
            {
                ReInitRenderTarget();
            }
            else
            {

                long durationTicks = ResizeRenderTargetDelay.TimeSpan.Ticks;

                // if the new next ReInitRenderTarget() interval is up
                if (((_reloadRenderTargetTime < 0) || (durationTicks <= 0))
                    // negative time will be reloaded immediatly
                    ||
                    ((_reloadRenderTargetTime > 0) &&
                     (Environment.TickCount >= (_reloadRenderTargetTime + durationTicks))))
                {
                    ReInitRenderTarget();
                }
            }

            if (!_imageSourceValid)
            {
                AttachRenderTarget(false);
            }

            Lock();
            try
            {
                _RenderFrame();
                AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            }
            finally
            {
                Unlock();
            }

        }
        
        /// <summary>
        /// If you wish to control when each Mogre frame is rendered, set this to true
        /// and invoke RenderFrame() or RenderFrameWithCheckAccess() in your application's loop.
        /// </summary>
        public bool ManualRender
        {
            get
            {
                return manualRender;
            }
            set
            {
                manualRender = value;
                UpdateEvents(!value);
            }
        } private bool manualRender;

        /// <summary>
        /// Invokes on this.Dispatcher if dispatch necessary, otherwise invoke RenderFrame immediately
        /// </summary>
        public void RenderFrameWithCheckAccess()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(RenderFrame));
            }
            else
            {
                RenderFrame();
            }
        }

        /// <summary>
        /// BeginInvokes on this.Dispatcher if dispatch necessary, otherwise invoke RenderFrame immediately
        /// </summary>
        public void RenderFrameAsyncWithCheckAccess()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(RenderFrame));
            }
            else
            {
                RenderFrame();
            }
        }

        public void DispatchRenderFrame()
        {
            Dispatcher.Invoke(new Action(RenderFrame));
        }

        #endregion

#if TimersTimer
        private void _eventRenderDispatch(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new EventHandler(_eventRender), null, null);
        }
#endif

        #region (Public) Statistics

        /// <summary>
        /// Reset the statistics on the Ogre RenderTarget
        /// </summary>
        public void ResetStatistics()
        {
            this._renTarget.ResetStatistics();
        }

        #endregion
        
        ///// <summary>
        ///// When creating multiple MogreImages, set this to false for instances that are not first. (TODO: Automate this)
        ///// (IN PROGRESS) (BUG): Try to determine if there is a way to support multiple MogreImage instances without hangs
        ///// </summary>
        //public bool IsMaster = true;  

    }
}