using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallAndStickTest
{
    public class Stick
    {
        Ball b1 = null;
        Ball b2 = null;

        Double oriLen = 30;
        Double A = 2;
        Double B = 1;
        public Double tension = 0;

        public Stick()
        {

        }

        public Stick(Ball b1, Ball b2)
        {
            this.b1 = b1;
            this.b2 = b2;
        }

        // 公式：
        // tension = (len - oriLen)^A * B
        public void UpdateTension()
        {
            Double len = (b1.position - b2.position).Length - oriLen;
            //if (len < 0)
                //B *= -1;
            //tension = Math.Pow(Math.Abs(len), A) * B;
            tension = len / oriLen;
        }

        public Ball GetNeighbour(Ball b)
        {
            return b == b1 ? b2 : b1;
        }
    }
}
