import sys 
sys.path.insert(0,'..') 
import ogre.renderer.OGRE as ogre 
import SampleFramework as sf 
import ogre.io.OIS as OIS
import ogre.gui.CEGUI as CEGUI

import CubeNav

# Convert OIS mouse event to CEGUI mouse event
def convertButton(oisID):
    if oisID == OIS.MB_Left:
        return CEGUI.LeftButton
    elif oisID == OIS.MB_Right:
        return CEGUI.RightButton
    elif oisID == OIS.MB_Middle:
        return CEGUI.MiddleButton
    else:
        return CEGUI.LeftButton

# Listener class
class TutorialListener(sf.FrameListener, OIS.MouseListener, OIS.KeyListener):
 
    #def __init__(self, renderWindow, camera):
    # add by kid ====>
    def __init__(self, renderWindow, camera, sceneManager, cubeNav):
    # <==== add by kid
        sf.FrameListener.__init__(self, renderWindow, camera, True, True)
        OIS.MouseListener.__init__(self)
        OIS.KeyListener.__init__(self)
        self.cont = True
        self.Mouse.setEventCallback(self)
        self.Keyboard.setEventCallback(self)
        # add by kid ====>
        self.sceneManager = sceneManager
        self.raySceneQuery = self.sceneManager.createRayQuery(ogre.Ray())
        self.cubeNav = cubeNav
        # <==== add by kid
 
    def frameStarted(self, evt):
        self.Keyboard.capture()
        self.Mouse.capture()
        return self.cont and not self.Keyboard.isKeyDown(OIS.KC_ESCAPE)
 
    def quit(self, evt):
        self.cont = False
        return True
 
    # MouseListener
    def mouseMoved(self, evt):
        # add by kid ====>
        self.cubeNav.onMove()
        # <==== add by kid
        CEGUI.System.getSingleton().injectMouseMove(evt.get_state().X.rel, evt.get_state().Y.rel)
        return True
 
    def mousePressed(self, evt, id):
        # add by kid ====>
        # pick a 3d object
        
        # setup the ray scene query, use CEGUI's mouse position 
        mousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        mouseRay = self.camera.getCameraToViewportRay(
            mousePos.d_x / float(evt.get_state().width),
            mousePos.d_y / float(evt.get_state().height))
        self.raySceneQuery.setRay(mouseRay)
        self.raySceneQuery.setSortByDistance(True)
        # execute query
        result = self.raySceneQuery.execute()
        if len(result) > 0:
            for item in result:
                print item.movable.getName()
                if item.movable.getName() == "CubeNav":
                    self.cubeNav.onPress()
                    break
        # <==== add by kid
        CEGUI.System.getSingleton().injectMouseButtonDown(convertButton(id))
        return True
 
    def mouseReleased(self, evt, id):
        # add by kid ====>
        self.cubeNav.onRelease()
        # <==== add by kid
        CEGUI.System.getSingleton().injectMouseButtonUp(convertButton(id))
        return True
 
    # KeyListener
    def keyPressed(self, evt):
        ceguiSystem = CEGUI.System.getSingleton()
        ceguiSystem.injectKeyDown(evt.key)
        ceguiSystem.injectChar(evt.text)  
        return True
 
    def keyReleased(self, evt):
        CEGUI.System.getSingleton().injectKeyUp(evt.key)

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

class TutorialApplication(sf.Application): 

    def _createScene(self): 
        sceneManager = self.sceneManager 
        sceneManager.ambientLight = ogre.ColourValue (0.3,0.3,0.3) 

        # draw triangle
        self.ent = tri("triangle", (0,0,0), (0,100,0), (100,0,0) )
        node1 = sceneManager.getRootSceneNode().createChildSceneNode ("node1") 
        node1.attachObject (self.ent)

        # add by kid ====>
        # draw a cube navigator
        self.cubeNav = CubeNav.CubeNavigator()
        cubeNavNode = sceneManager.getRootSceneNode().createChildSceneNode("cubeNavNode")
        cubeNavNode.attachObject(self.cubeNav)
        cubeNavNode.setPosition(150, -150, 0)
        direction = cubeNavNode.getPosition() - self.camera.getPosition()
        cubeNavNode.setDirection(direction)
        # <==== add by kid

        # initiaslise CEGUI Renderer
        self.CEGUIRenderer = CEGUI.OgreRenderer.bootstrapSystem()
        self.CEGUIRenderer = CEGUI.System.getSingleton()
        # load TaharezLook scheme
        CEGUI.SchemeManager.getSingleton().create("VanillaSkin.scheme")
        # load font and setup default if not loaded via scheme
        CEGUI.FontManager.getSingleton().create("DejaVuSans-10.font")
        CEGUI.System.getSingleton().setDefaultMouseCursor("Vanilla-Images", "MouseArrow")
        CEGUI.System.getSingleton().setDefaultFont("DejaVuSans-10")
        # load the layout
        CEGUI.System.getSingleton().setGUISheet(
        CEGUI.WindowManager.getSingleton().loadWindowLayout("Kidle1.layout"))

    def _createFrameListener(self):
        #self.frameListener = TutorialListener(self.renderWindow, self.camera)
        # add by kid ====>
        self.frameListener = TutorialListener(self.renderWindow, self.camera,
                                              self.sceneManager, self.cubeNav)
        # <==== add by kid
        self.frameListener.showDebugOverlay(True)
        self.root.addFrameListener(self.frameListener)
        
    def __del__ ( self ):
        del self.ent
        if self.system:
            del self.system
        if self.renderer:
            del self.renderer

        
        
ta = TutorialApplication() 
ta.go() 
