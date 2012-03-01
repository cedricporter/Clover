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
    public static class MogreRootManager
    {
        private static bool isInitialized = false;
        private static object isInitializedLock = new object();
        private static bool isInitializing = false;
        private static object isInitializingLock = new object();

        public static ManualResetEvent InitializedEvent
        {
            get
            {
                return initializedEvent;
            }
        }
        private static ManualResetEvent initializedEvent = new ManualResetEvent(false);

        // TODO - sort this out
        public static void StartInitializing()
        {
            //if (isInitialized) return;

            //lock (isInitializingLock)
            //{
            //    if (isInitializing) return;
            //    if (isInitialized) return;

            //     GetSharedRoot().In
            //}
            Root root = GetSharedRoot();
        }

        public static void WaitForInitialized()
        {
            if (isInitialized) return;

            Root root = GetSharedRoot();

            //lock (isInitializedLock)
            //{
            //    if (isInitialized) return;

            //}
        }

        #region (Private) Fields

        private static Root root;
        private static object rootLock = new object();

        #endregion

        #region (Public) Methods for Shared Root

        public static Root GetSharedRoot()
        {
            lock (rootLock)
            {
                if (root == null)
                {
                    root = MogreRootFactory.CreateRoot();

                    initializedEvent.Set();
                    isInitialized = true;
                }
            }
            return root;
        }

        public static void DisposeSharedRoot()
        {
            //lock (isInitializedLock)
            {
                lock (rootLock)
                {
                    MogreRootFactory.DisposeRoot(root);
                    root = null;
                    isInitialized = false;
                }
            }
        }

        #endregion

        public static bool HasRoot
        {
            get
            {
                lock (rootLock)
                { return root != null; }
            }
        }
    }
}
