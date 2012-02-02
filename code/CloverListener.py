import sys 
sys.path.insert(0,'..') 
import ogre.renderer.OGRE as ogre 
import SampleFramework as sf 
import ogre.io.OIS as OIS
import ogre.gui.CEGUI as CEGUI

import CubeNav
import CloverApplication

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
class CloverListener(sf.FrameListener, OIS.MouseListener, OIS.KeyListener):
 
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


        
