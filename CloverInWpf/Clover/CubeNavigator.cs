using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

/**
@date		:	2012/02/27
@filename	: 	CubeNavigator.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	一个立方体导航块
**/

/*   cube by kid..
      4 ______7
      /|      /
   0 /_|___3 /|
     | |____|_|
     | /5   | /6
    1|/____2|/    
*/


namespace Clover
{
    public class CubeNavigator : ManualObject
    {

//        // point list
//PL = [[-20, 20, 20], [-20, -20, 20], [20, -20, 20], [20, 20, 20],
//      [-20, 20, -20], [-20, -20, -20], [20, -20, -20], [20, 20, -20]]
//// index list /ft/bk/lt/rt/up/dn
//IL = [[0,1,3], [3,1,2],
//      [7,6,4], [4,6,5],
//      [4,5,0], [0,5,1],
//      [3,2,7], [7,2,6],
//      [4,0,7], [7,0,3],
//      [1,5,2], [2,5,6]]
//// height light index list
//HIL = [[0,1,3,2], [7,6,4,5], [4,5,0,1],
//       [3,2,7,6], [4,0,7,3], [1,5,2,6]]
//// texture coordinates list
//TC = [[[0.6667, 0], [0.6667, 1], [0.8333, 0]],
//      [[0.8333, 0],[0.6667, 1], [0.8333, 1]],
//      [[0.833, 0], [0.833, 1], [1, 0]], 
//      [[1, 0], [0.833, 1], [1, 1]],
//      [[0.3333, 0], [0.3333, 1], [0.5, 0]], 
//      [[0.5, 0], [0.3333, 1], [0.5, 1]],
//      [[0.5, 0], [0.5, 1], [0.6667, 0]], 
//      [[0.6667, 0], [0.5, 1], [0.6667, 1]],
//      [[0, 0], [0, 1], [0.1667, 0]], 
//      [[0.1667, 0], [0, 1], [0.1667, 1]],
//      [[0.1667, 0], [0.1667, 1], [0.3333, 0]], 
//      [[0.3333, 0], [0.1667, 1], [0.3333, 1]]]

//// face quaternion list, quaternion for six face
//FQL = [Quaternion(1, 0, 0, 0), //ft
//       Quaternion(0, 0, 1, 0), //bk
//       Quaternion(math.sqrt(0.5), 0, math.sqrt(0.5), 0), //lt
//       Quaternion(math.sqrt(0.5), 0, -math.sqrt(0.5), 0), //rt
//       Quaternion(math.sqrt(0.5), math.sqrt(0.5), 0, 0), //up
//       Quaternion(math.sqrt(0.5), -math.sqrt(0.5), 0, 0)] //dn

        public CubeNavigator(SceneNode cloverRoot) : base("CubeNavigator")
        {
            //MaterialPtr material = (MaterialPtr)MaterialManager.Singleton.Create("CubeNavMat", "General");
            //Pass pass = material.GetTechnique(0).GetPass(0);
            //pass.LightingEnabled = false;
            //pass.DepthCheckEnabled = false;
            //pass.CreateTextureUnitState("CubeNavTex.png");
            //System.Windows.MessageBox.Show(this.Name);
        }
    }
}
