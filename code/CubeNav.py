# Create by kid at 31.1.2012
# To create a cube navigator

import ogre.renderer.OGRE as ogre

class CubeNavigator(ogre.ManualObject):
    def __init__(self):
        ogre.ManualObject.__init__(self, "CubeNav")
        # create materials
        material = ogre.MaterialManager.getSingleton().create("ft", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.createTextureUnitState("ft.png")
        #ipass.setCullingMode(ogre.CULL_FRONT)
        # front
        self.clear()
        self.begin("ft", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position( -20, 20, -20); 
        self.textureCoord(0, 0);
        self.position( -20, -20, -20); 
        self.textureCoord(0, 1);
        self.position( 20, 20, -20);  
        self.textureCoord(1, 0);
        self.position( 20, -20, -20);   
        self.textureCoord(1, 1);
        self.end()



