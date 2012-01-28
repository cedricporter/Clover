import SampleFramework as sf
import ogre.renderer.OGRE as ogre
import ogre.io.OIS as OIS
import ogre.gui.CEGUI as CEGUI

def convertButton(oisID):
    if oisID == OIS.MB_Left:
        return CEGUI.LeftButton
    elif oisID == OIS.MB_Right:
        return CEGUI.RightButton
    elif oisID == OIS.MB_Middle:
        return CEGUI.MiddleButton
    else:
        return CEGUI.LeftButton

 
class TutorialListener(sf.FrameListener, OIS.MouseListener, OIS.KeyListener):
 
    def __init__(self, renderWindow, camera):
        sf.FrameListener.__init__(self, renderWindow, camera, True, True)
        OIS.MouseListener.__init__(self)
        OIS.KeyListener.__init__(self)
        self.cont = True
        self.Mouse.setEventCallback(self)
        self.Keyboard.setEventCallback(self)
 
    def frameStarted(self, evt):
        self.Keyboard.capture()
        self.Mouse.capture()
        return self.cont and not self.Keyboard.isKeyDown(OIS.KC_ESCAPE)
 
    def quit(self, evt):
        self.cont = False
        return True
 
    # MouseListener
    def mouseMoved(self, evt):
        CEGUI.System.getSingleton().injectMouseMove(evt.get_state().X.rel, evt.get_state().Y.rel)
        return True
 
    def mousePressed(self, evt, id):
        CEGUI.System.getSingleton().injectMouseButtonDown(convertButton(id))
        return True
 
    def mouseReleased(self, evt, id):
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
        return True
 
class TutorialApplication(sf.Application):
 
    def __del__(self):
        if self.system:
            del self.system
        if self.renderer:
            del self.renderer
 
    def _createScene(self):
        ## setup GUI system
        self.renderer = CEGUI.OgreRenderer.bootstrapSystem()
        self.system = CEGUI.System.getSingleton()
        ## Select the skin for the CEGUI to use
        CEGUI.SchemeManager.getSingleton().create("TaharezLookSkin.scheme")
        self.system.setDefaultMouseCursor("TaharezLook", "MouseArrow")
        self.system.setDefaultFont("BlueHighway-12")
        # Do not add this to the program
        sheet = CEGUI.WindowManager.getSingleton().loadWindowLayout("ogregui.layout")
        self.system.setGUISheet(sheet)
        

 
    def _createFrameListener(self):
        self.frameListener = TutorialListener(self.renderWindow, self.camera)
        self.frameListener.showDebugOverlay(True)
        self.root.addFrameListener(self.frameListener)
 
if __name__ == '__main__':
    try:
        ta = TutorialApplication()
        ta.go()
    except ogre.OgreException, e:
        print e

