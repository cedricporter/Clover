# Create by kid at 31.1.2012
# a cube navigator

import ogre.renderer.OGRE as ogre
import ogre.gui.CEGUI as CEGUI

class CubeNavigator(ogre.ManualObject):
    # when clicked, set focus
    def onPress(self):
        self.focus = True
        
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
            self.lastMousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        mousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        #self.getParentSceneNode().yaw((mousePos.d_x - self.lastMousePos.d_x) / 100.0)
        #self.getParentSceneNode().pitch((mousePos.d_y - self.lastMousePos.d_y) / 100.0)
        quad = ogre.Quaternion((mousePos.d_y - self.lastMousePos.d_y) / 100.0,
                               ogre.Vector3(1,0,0))
        
        self.getParentSceneNode().rotate(quad * self.lastOrientation)
        self.lastMousePos = mousePos
    
        #quad = self.getParentSceneNode().getOrientation()
        #print quad
        
    # To Create a cube object
    def __init__(self):
        # initialize
        ogre.ManualObject.__init__(self, "CubeNav")
        self.lastMousePos = None
        self.lastOrientation = None
        self.focus = False
        self.initialized = False
        # create materials
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
        self.end()



