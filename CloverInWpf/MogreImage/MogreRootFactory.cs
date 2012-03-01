using System;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Mogre;

namespace MogreInWpf
{
    public static class MogreRootFactory
    {
        public static readonly string[] SupportedRenderSystems = new string[]
        {
            "Direct3D9 Rendering Subsystem",
            "Direct3D9Ex Rendering Subsystem",
            "Direct3D11 Rendering Subsystem",
        };

        #region (Public) Parameters

        public static string ResourcesFileName = "resources.cfg";

        #endregion

        public static Root CreateRoot()
        {
            Root _root;

            // load the OGRE engine
            //
            _root = new Root();

            // configure resource paths from : "resources.cfg" file
            //
            var configFile = new ConfigFile();
            configFile.Load(ResourcesFileName, "\t:=", true);

            // Go through all sections & settings in the file
            //
            ConfigFile.SectionIterator sectionIterator = configFile.GetSectionIterator();

            // Normally we would use the foreach syntax, which enumerates the values, 
            // but in this case we need CurrentKey too;
            while (sectionIterator.MoveNext())
            {
                string secName = sectionIterator.CurrentKey;

                ConfigFile.SettingsMultiMap settings = sectionIterator.Current;
                foreach (var pair in settings)
                {
                    string typeName = pair.Key;
                    string archName = pair.Value;
                    ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
                }
            }

            // Configures the application and creates the Window
            // A window HAS to be created, even though we'll never use it.
            //
            bool foundit = false;
            foreach (RenderSystem rs in _root.GetAvailableRenderers())
            {
                if (rs == null) continue;

                _root.RenderSystem = rs;
                string rname = _root.RenderSystem.Name;
                if (SupportedRenderSystems.Contains(rname))
                {
                    foundit = true;
                    break;
                }
            }

            if (!foundit)
            {
                // Jared changed to exception
                throw new Exception("Failed to find a compatible render system.  See MogreImage.SupportedRenderSystems for a list of supported renderers.");
            }

            _root.RenderSystem.SetConfigOption("Full Screen", "No");
            _root.RenderSystem.SetConfigOption("Video Mode", "640 x 480 @ 32-bit colour");

            _root.Initialise(false);
            
            #region Get Render Window
            
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

            if (hWnd == IntPtr.Zero) throw new Exception("Could not get hWnd");

            var misc = new NameValuePairList();
            misc["externalWindowHandle"] = hWnd.ToString();
            RenderWindow _renderWindow = _root.CreateRenderWindow("OgreImageSource Windows", 0, 0, false, misc);
            _renderWindow.IsAutoUpdated = false;

            #endregion

            #region Load Resources

            InitResourceLoad();
            ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

            #endregion

            return _root;
        }

        public static void DisposeRoot(Root root)
        {
            if (root != null)
            {
                CompositorManager.Singleton.RemoveAll(); // REVIEW

                root.Dispose();
            }
        }

        #region Resource Loading

        private static double _currentProcess;
        private static double _resourceItemScalar;

        private static void CallResourceItemLoaded(ResourceLoadEventArgs e)
        {
            //Dispatcher.Invoke((MethodInvoker)(() => OnResourceItemLoaded(e)));
            OnResourceItemLoaded(e);
        }

        private static void InitResourceLoad()
        {
            CallResourceItemLoaded(new ResourceLoadEventArgs("Engine", 0));

            //ResourceGroupManager.Singleton.ResourceGroupLoadStarted += Singleton_ResourceGroupLoadStarted;
            ResourceGroupManager.Singleton.ResourceGroupScriptingStarted += Singleton_ResourceGroupScriptingStarted;
            ResourceGroupManager.Singleton.ScriptParseStarted += Singleton_ScriptParseStarted;
            ResourceGroupManager.Singleton.ResourceLoadStarted += Singleton_ResourceLoadStarted;
            ResourceGroupManager.Singleton.WorldGeometryStageStarted += Singleton_WorldGeometryStageStarted;

            _currentProcess = 0;
        }

        private static void Singleton_WorldGeometryStageStarted(string description)
        {
            _currentProcess += _resourceItemScalar;
            CallResourceItemLoaded(new ResourceLoadEventArgs(description, _currentProcess));
        }

        private static void Singleton_ResourceLoadStarted(ResourcePtr resource)
        {
            _currentProcess += _resourceItemScalar;
            CallResourceItemLoaded(new ResourceLoadEventArgs(resource.Name, _currentProcess));
        }

        private static void Singleton_ScriptParseStarted(string scriptName, out bool skipThisScript)
        {
            skipThisScript = false; // Jared added
            _currentProcess += _resourceItemScalar;
            CallResourceItemLoaded(new ResourceLoadEventArgs(scriptName, _currentProcess));
        }

        private static void Singleton_ResourceGroupScriptingStarted(string groupName, uint scriptCount)
        {
            _resourceItemScalar = (scriptCount > 0)
                                      ? 0.4d / scriptCount
                                      : 0;
        }

        private static void OnResourceItemLoaded(ResourceLoadEventArgs e)
        {
            if (ResourceLoadItemProgress != null) ResourceLoadItemProgress(null, e);
        }


        private static void Singleton_ResourceGroupLoadStarted(string groupName, uint resourceCount)
        {
            _resourceItemScalar = (resourceCount > 0)
                                      ? 0.6d / resourceCount
                                      : 0;
        }

        public static  event EventHandler<ResourceLoadEventArgs> ResourceLoadItemProgress;

        #endregion
    }
    public class ResourceLoadEventArgs : EventArgs
    {
        public ResourceLoadEventArgs(string name, double progress)
        {
            this.Name = name;
            this.Progress = progress;
        }

        public string Name { get; private set; }
        public double Progress { get; private set; }
    }
}
