# Create by kid at 31.1.2012
# a cube navigator

import ogre.renderer.OGRE as ogre
import ogre.gui.CEGUI as CEGUI
import math

class CubeNavigator(ogre.ManualObject):
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
        
    # when focused, present navigation
    def onMove(self):
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
        ogreMouseVec.normalise()
        rotateAxis = ogreMouseVec.crossProduct(ogre.Vector3(0, 0, -1))
        # determine the rotate degree
        rotateDegree = math.sqrt(math.pow(mouseOffset.d_x, 2.0)
                                 + math.pow(mouseOffset.d_y, 2.0)) / 100.0
        # use the axis and degree to create quaternion
        quat = ogre.Quaternion(rotateDegree, rotateAxis)
        quat = self.lastOrientation * quat
        self.getParentSceneNode().setOrientation(quat)
        #print self.getParentSceneNode().getOrientation() * ogre.Vector3(0,0,-1)
        #self.getParentSceneNode().rotate(self.lastOrientation)
        #self.lastMousePos = mousePos'''
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
        # front
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
        self.end()
        
        '''# create materials
        material = ogre.MaterialManager.getSingleton().create("ft", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("ft.png")
        material = ogre.MaterialManager.getSingleton().create("bk", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("bk.png")
        material = ogre.MaterialManager.getSingleton().create("lt", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("lt.png")
        material = ogre.MaterialManager.getSingleton().create("rt", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("rt.png")
        material = ogre.MaterialManager.getSingleton().create("up", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("up.png")
        material = ogre.MaterialManager.getSingleton().create("dn", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("dn.png") 
        # front
        self.clear()
        self.begin("ft", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, 20); 
        self.textureCoord(0, 0);
        self.position( -20, -20, 20); 
        self.textureCoord(0, 1);
        self.position( 20, 20, 20);  
        self.textureCoord(1, 0);
        self.position( 20, -20, 20);   
        self.textureCoord(1, 1);
        self.end()
        # back
        self.begin("bk", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( 20, 20, -20); 
        self.textureCoord(0, 0);
        self.position( 20, -20, -20); 
        self.textureCoord(0, 1);
        self.position( -20, 20, -20);  
        self.textureCoord(1, 0);
        self.position( -20, -20, -20);   
        self.textureCoord(1, 1);
        self.end()
        # left
        self.begin("lt", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, -20); 
        self.textureCoord(0, 0);
        self.position( -20, -20, -20); 
        self.textureCoord(0, 1);
        self.position( -20, 20, 20);  
        self.textureCoord(1, 0);
        self.position( -20, -20, 20);   
        self.textureCoord(1, 1);
        self.end()
        # right
        self.begin("rt", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( 20, 20, 20); 
        self.textureCoord(0, 0);
        self.position( 20, -20, 20); 
        self.textureCoord(0, 1);
        self.position( 20, 20, -20);  
        self.textureCoord(1, 0);
        self.position( 20, -20, -20);   
        self.textureCoord(1, 1);
        self.end()
        # up
        self.begin("up", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, -20); 
        self.textureCoord(0, 0);
        self.position( -20, 20, 20); 
        self.textureCoord(0, 1);
        self.position( 20, 20, -20);  
        self.textureCoord(1, 0);
        self.position( 20, 20, 20);   
        self.textureCoord(1, 1);
        self.end() 
        # down
        self.begin("dn", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, -20, 20); 
        self.textureCoord(0, 0);
        self.position( -20, -20, -20); 
        self.textureCoord(0, 1);
        self.position( 20, -20, 20);  
        self.textureCoord(1, 0);
        self.position( 20, -20, -20);   
        self.textureCoord(1, 1);
        self.end()'''



