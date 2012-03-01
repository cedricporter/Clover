using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace MogreInWpf
{
    public struct ViewportDefinition
    {
        #region Fields

        public Camera Camera;
        public float Left;
        public float Top;
        public float Width;
        public float Height;
        public ColourValue BackgroundColour;

        #endregion

        #region Construction

        public ViewportDefinition(Camera camera)
        {
            Left = 0;
            Top = 0;
            Width = 1;
            Height = 1;
            BackgroundColour = new ColourValue(0, 0, 0, 0);
            Camera = camera;
        }

        #endregion

    }
}
