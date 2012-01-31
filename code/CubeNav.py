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


