
import sys 
sys.path.insert(0,'..') 
import ogre.renderer.OGRE as ogre 
import SampleFramework as sf 
import ogre.io.OIS as OIS
import ogre.gui.CEGUI as CEGUI

import CloverListener
import CubeNav
from SimplePicker import SimplePicker

# 
class tri(ogre.ManualObject): 
    #simplest possible manual object, for testing purposes 
    #takes 3 tuples A, B, C 
    def __init__(me, name, A, B, C):
        me.A = A 
        me.B = B 
        me.C = C 
        ogre.ManualObject.__init__(me, name) 
        A, B, C = me.A, me.B, me.C
        me.begin("default", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        me.position(A[0], A[1], A[2]) 
        me.normal(0, 0, 1)
        me.position(B[0], B[1], B[2]) 
        me.position(C[0], C[1], C[2]) 
        me.position(A[0], A[1], A[2]) 
        me.end()

##
# @brief    Clover Application Abstract
class CloverApplication(sf.Application): 

    def _createScene(self): 
        sceneManager = self.sceneManager 
        sceneManager.ambientLight = ogre.ColourValue (0.3,0.3,0.3) 

        # every clover scene node must attach to the root scene node
        self.cloverRoot = sceneManager.getRootSceneNode().createChildSceneNode("cloverRoot")
        # draw triangle
        self.ent = tri("triangle", (0,0,0), (0,100,0), (100,0,0) )
        #node1 = self.cloverRoot.createChildSceneNode ("node1") 
        self.cloverRoot.attachObject (self.ent)
        # draw a cube navigator
        self.cubeNav = CubeNav.CubeNavigator(self.cloverRoot)
        cubeNavNode = sceneManager.getRootSceneNode().createChildSceneNode("cubeNavNode")
        cubeNavNode.attachObject(self.cubeNav)
        cubeNavNode.setPosition(100, -100, -100)
        direction = cubeNavNode.getPosition() - self.camera.getPosition()
        cubeNavNode.setDirection(direction)
        # initialise CEGUI Renderer
        self.CEGUIRenderer = CEGUI.OgreRenderer.bootstrapSystem()
        self.CEGUISystem = CEGUI.System.getSingleton()
        # load TaharezLook scheme
        CEGUI.SchemeManager.getSingleton().create("VanillaSkin.scheme")
        # load font and setup default if not loaded via scheme
        CEGUI.FontManager.getSingleton().create("DejaVuSans-10.font")
        self.CEGUISystem.setDefaultMouseCursor("Vanilla-Images", "MouseArrow")
        self.CEGUISystem.setDefaultFont("DejaVuSans-10")
        # load the layout
        self.CEGUISystem.setGUISheet(
        CEGUI.WindowManager.getSingleton().loadWindowLayout("Kidle1.layout"))
    
    def _createFrameListener(self):
        #self.frameListener = CloverListener.CloverListener(self.renderWindow, self.camera,
        #                                      self.sceneManager, self.cubeNav)
        # add by kid ======>>
        self.frameListener = CloverListener.CloverListener(self.renderWindow,
                                             self.camera, self.sceneManager, 
                                             self.cubeNav, self.simplePicker)
        # <<====== add by kid
        self.frameListener.showDebugOverlay(True)
        self.root.addFrameListener(self.frameListener)

    def _createCamera(self):
        sf.Application._createCamera(self)
        self.camera.setAutoAspectRatio(True)
        self.camera.setFOVy(0.4)
        
    # add by kid ======>>
    def _createViewports(self):
        sf.Application._createViewports(self)
        self.viewport.BackgroundColour = ogre.ColourValue(1.0,1.0,1.0)
    # <<====== add by kid
    
    # add by kid ======>>
    def __init__(self):
        sf.Application.__init__(self)
        self.simplePicker = SimplePicker()
    # <<====== add by kid
    
    def __del__ ( self ):
        del self.ent
        del self.cubeNav
        del self.frameListener
        if self.CEGUISystem:
            del self.system
        if self.CEGUIRenderer:
            del self.renderer

