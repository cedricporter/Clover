"""
a cube navigator

Create by kid at 2012/1/31
"""

import ogre.renderer.OGRE as ogre
import ogre.gui.CEGUI as CEGUI
import math


"""   cube by kid..
      4 ______7
      /|      /
   0 /_|___3 /|
     | |____|_|
     | /5   | /6
    1|/____2|/    
"""
# point list
PL = [[-20, 20, 20], [-20, -20, 20], [20, -20, 20], [20, 20, 20],
      [-20, 20, -20], [-20, -20, -20], [20, -20, -20], [20, 20, -20]]
# index list /ft/bk/lt/rt/up/dn
IL = [[0,1,2,3], [7,6,5,4],
      [4,5,1,0], [3,2,6,7],
      [4,0,3,7], [1,5,6,2]]

class CubeNavigator(ogre.ManualObject):
    # when over one of the six face, height light
    def onMove(self):
        #rotMat = ogre.Matrix3()
        #self.getParentSceneNode().getOrientation().ToRotationMatrix(rotMat)
        #print rotMat
        pass
    
    # when clicked, set focus
    def onPress(self):
        self.focus = True
        self.lastMousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        
    # when released, set no focus
    def onRelease(self):
        if self.focus == False:
            return
        self.focus = False
        self.lastOrientation = self.getParentSceneNode().getOrientation()
        
    # when draged, present navigation
    def onDrag(self):
        if self.focus == False:
            return
        if self.initialized == False:
            self.initialized = True
            self.lastOrientation = self.getParentSceneNode().getOrientation()
            #self.lastMousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        # find the rotate axis
        mousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        mouseOffset = mousePos - self.lastMousePos
        ogreMouseVec = ogre.Vector3(mouseOffset.d_x, -mouseOffset.d_y, 0)
        #ogreMouseVec.normalise()
        rotateAxis = ogreMouseVec.crossProduct(ogre.Vector3(0, 0, -1))
        rotateAxis.normalise()
        # add by kid ======>>
        #rotMat = ogre.Matrix3()
        #self.lastOrientation.ToRotationMatrix(rotMat)
        #rotateAxis =  rotMat * rotateAxis
        #print rotateAxis
        # <<====== add by kid
        # determine the rotate degree
        rotateDegree = math.sqrt(math.pow(mouseOffset.d_x, 2.0)
                                 + math.pow(mouseOffset.d_y, 2.0)) / 100.0
        # use the axis and degree to create quaternion
        quat = ogre.Quaternion(rotateDegree, rotateAxis)
        quat = self.lastOrientation * quat
        self.getParentSceneNode().setOrientation(quat)        
        # let the cloverRoot rotate as the cube
        self.cloverRoot.setOrientation(self.getParentSceneNode().getOrientation())
        
    # To Create a cube object
    def __init__(self, cloverRoot):
        # initialize
        ogre.ManualObject.__init__(self, "CubeNav")
        self.cloverRoot = cloverRoot
        self.lastMousePos = None
        self.lastOrientation = None
        self.focus = False
        self.initialized = False
        # create materials
        material = ogre.MaterialManager.getSingleton().create("CubeNavMat", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.setDepthCheckEnabled(False)
        ipass.createTextureUnitState("CubeNavTex.png")
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        # vertices
        for P in PL:
            self.position(P[0], P[1], P[2])
        # front
        for I in IL:
            self.quad(I[0], I[1], I[2], I[3])
        # back
        # left
        # right
        # up
        # down
        self.end()
        #self.position(PL[0][0],PL[0][1],PL[0][2])
        
        """# front
        self.clear()
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, 20); 
        self.textureCoord(0.6667, 0);
        self.position( -20, -20, 20); 
        self.textureCoord(0.6667, 1);
        self.position( 20, 20, 20);  
        self.textureCoord(0.8333, 0);
        self.position( 20, -20, 20);   
        self.textureCoord(0.8333, 1);
        self.end()
        # back
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( 20, 20, -20); 
        self.textureCoord(0.8333, 0);
        self.position( 20, -20, -20); 
        self.textureCoord(0.8333, 1);
        self.position( -20, 20, -20);  
        self.textureCoord(1, 0);
        self.position( -20, -20, -20);   
        self.textureCoord(1, 1);
        self.end()
        # left
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, -20); 
        self.textureCoord(0.3333, 0);
        self.position( -20, -20, -20); 
        self.textureCoord(0.3333, 1);
        self.position( -20, 20, 20);  
        self.textureCoord(0.5, 0);
        self.position( -20, -20, 20);   
        self.textureCoord(0.5, 1);
        self.end()
        # right
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( 20, 20, 20); 
        self.textureCoord(0.5, 0);
        self.position( 20, -20, 20); 
        self.textureCoord(0.5, 1);
        self.position( 20, 20, -20);  
        self.textureCoord(0.6667, 0);
        self.position( 20, -20, -20);   
        self.textureCoord(0.6667, 1);
        self.end()
        # up
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, -20); 
        self.textureCoord(0, 0);
        self.position( -20, 20, 20); 
        self.textureCoord(0, 1);
        self.position( 20, 20, -20);  
        self.textureCoord(0.1667, 0);
        self.position( 20, 20, 20);   
        self.textureCoord(0.1667, 1);
        self.end() 
        # down
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, -20, 20); 
        self.textureCoord(0.1667, 0);
        self.position( -20, -20, -20); 
        self.textureCoord(0.1667, 1);
        self.position( 20, -20, 20);  
        self.textureCoord(0.3333, 0);
        self.position( 20, -20, -20);   
        self.textureCoord(0.3333, 1);
        self.end()"""



