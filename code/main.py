
import ogre.renderer.OGRE as ogre
import ogre.io.OIS as OIS
import ogre.gui.CEGUI as CEGUI
 
class ExitListener(ogre.FrameListener):
 
    def __init__(self, keyboard):
        ogre.FrameListener.__init__(self)
        self.keyboard = keyboard
 
    def frameStarted(self, evt):
        self.keyboard.capture()
        return not self.keyboard.isKeyDown(OIS.KC_ESCAPE)
 
    def __del__(self):
        del self.renderer
        del self.system
        del self.exitListener
        del self.root
 
class Application(object):
 
    def go(self):
        self.createRoot()
        self.defineResources()
        self.setupRenderSystem()
        self.createRenderWindow()
        self.initializeResourceGroups()
        self.setupScene()
        self.setupInputSystem()
        self.setupCEGUI()
        self.createFrameListener()
        self.startRenderLoop()
        self.cleanUp()
 
    # The Root constructor for the ogre
    def createRoot(self):
        self.root = ogre.Root()
 
    # Here the resources are read from the resources.cfg
    def defineResources(self):
        cf = ogre.ConfigFile()
        cf.load("resources.cfg")
 
        seci = cf.getSectionIterator()
        while seci.hasMoreElements():
            secName = seci.peekNextKey()
            settings = seci.getNext()
 
            for item in settings:
                typeName = item.key
                archName = item.value
                ogre.ResourceGroupManager.getSingleton().addResourceLocation(archName, typeName, secName)
 
    # Create and configure the rendering system (either DirectX or OpenGL) here
    def setupRenderSystem(self):
        if not self.root.restoreConfig() and not self.root.showConfigDialog():
            raise Exception("User canceled the config dialog -> Application.setupRenderSystem()")
 
 
    # Create the render window
    def createRenderWindow(self):
        self.root.initialise(True, "Tutorial Render Window")
 
    # Initialize the resources here (which were read from resources.cfg in defineResources()
    def initializeResourceGroups(self):
        ogre.TextureManager.getSingleton().setDefaultNumMipmaps(5)
        ogre.ResourceGroupManager.getSingleton().initialiseAllResourceGroups()
 
    # Now, create a scene here. Three things that MUST BE done are sceneManager, camera and
    # viewport initializations
    def setupScene(self):
        sceneManager = self.root.createSceneManager(ogre.ST_GENERIC, "Default SceneManager")
        camera = sceneManager.createCamera("Camera")
        viewPort = self.root.getAutoCreatedWindow().addViewport(camera)
 
    # here setup the input system (OIS is the one preferred with Ogre3D)
    def setupInputSystem(self):
        windowHandle = 0
        renderWindow = self.root.getAutoCreatedWindow()
        windowHandle = renderWindow.getCustomAttributeInt("WINDOW")
        paramList = [("WINDOW", str(windowHandle))]
        self.inputManager = OIS.createPythonInputSystem(paramList)
 
        # Now InputManager is initialized for use. Keyboard and Mouse objects
        # must still be initialized separately
        try:
            self.keyboard = self.inputManager.createInputObjectKeyboard(OIS.OISKeyboard, False)
            self.mouse = self.inputManager.createInputObjectMouse(OIS.OISMouse, False)
        except Exception, e:
            raise e
 
 
    # CEGUI library is used for creating graphical user interfaces (options menus, etc)
    def setupCEGUI(self):
        sceneManager = self.root.getSceneManager("Default SceneManager")
        renderWindow = self.root.getAutoCreatedWindow()
 
        # CEGUI setup
        # The newer version of CEGUI has different syntax, so this tutorial code results 
        # in runnable program when used
        if CEGUI.Version__.startswith("0.6"):
            self.renderer = CEGUI.OgreCEGUIRenderer(renderWindow, ogre.RENDER_QUEUE_OVERLAY, False, 3000, sceneManager)
            self.system = CEGUI.System(self.renderer)
        else:
            self.renderer = CEGUI.OgreRenderer.bootstrapSystem()
            self.system = CEGUI.System.getSingleton()
 
    # Create the frame listeners
    def createFrameListener(self):
        self.exitListener = ExitListener(self.keyboard)
        self.root.addFrameListener(self.exitListener)
 
    # This is the rendering loop
    def startRenderLoop(self):
        self.root.startRendering()
 
    # In the end, clean everything up (= delete)
    def cleanUp(self):
        self.inputManager.destroyInputObjectKeyboard(self.keyboard)
        self.inputManager.destroyInputObjectMouse(self.mouse)
        OIS.InputManager.destroyInputSystem(self.inputManager)
        self.inputManager = None
 
if __name__ == '__main__':
    try:
        ta = Application()
        ta.go()
    except ogre.OgreException, e:
        print e
