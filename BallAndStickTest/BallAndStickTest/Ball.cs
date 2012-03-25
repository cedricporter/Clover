using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallAndStickTest
{
    public class Ball
    {
        public static Double timespan = 33;

        public Point position = new Point();
        public Double diameter = 30;

        public Boolean isAnchor = false;
        public Boolean isDraging = false;

        //public List<Ball> naighbours = new List<Ball>();
        public List<Stick> sticks = new List<Stick>();

        public Vector velocity = new Vector();

        public void UpdatePosition()
        {
            if (isDraging || isAnchor)
                return;
            Vector force = new Vector();
            foreach (Stick s in sticks)
                force += (s.GetNeighbour(this).position - this.position) * s.tension;
            position += force / 6;
        }
    }
}
