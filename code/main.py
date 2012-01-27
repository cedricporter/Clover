
#!/usr/bin/env python 
# This code is Public Domain. 
"""Python-Ogre Intermediate Tutorial 03: Final code"""
 
import ogre.renderer.OGRE as ogre 
import ogre.gui.CEGUI as CEGUI
import ogre.io.OIS as OIS
import SampleFramework as sf
 
class MouseQueryListener(sf.FrameListener, OIS.MouseListener):
    """A FrameListener class that handles basic user input."""
 
    NINJA_MASK = 1 << 0
    ROBOT_MASK = 1 << 1
 
    def __init__(self, win, cam, sc, renderer):
       # Subclass any Python-Ogre class and you must call its constructor.
       sf.FrameListener.__init__(self, win, cam, True, True)
       OIS.MouseListener.__init__(self)
 
       self.sceneManager = sc
       self.ceguiRenderer = renderer
       self.camera = cam
 
       # Register as MouseListener (Basic tutorial 5)
       self.Mouse.setEventCallback(self)
 
       # Initialize our state values
       self.raySceneQuery = None
       self.leftMouseDown = False
       self.rightMouseDown = False
       self.robotCount = 0
       self.currentObject = None
       self.moveSpeed = 50
       self.rotateSpeed = 1/500.0
       self.robotMode = True
       self.debugText = "Robot Mode Enabled - Press Space to Toggle"
 
       self.raySceneQuery = self.sceneManager.createRayQuery(ogre.Ray())
 
    def frameStarted(self, evt):
       if not sf.FrameListener.frameStarted(self, evt):
           return False
 
       if self.Keyboard.isKeyDown(OIS.KC_SPACE) and self.timeUntilNextToggle <= 0:
           self.robotMode = not self.robotMode
           self.timeUntilNextToggle = 1
           if self.robotMode:
               type = "Robot"
           else:
               type = "Ninja"
           self.debugText = type + " Mode Enabled - Press Space to Toggle"
 
       # Find the current position, fire a Ray straight down
       # in order to determine the distance to the terrain
       # If we are too close, keep the distance to a certain amount
 
       camPos = self.camera.getPosition()
       ray = ogre.Ray((camPos.x, 5000.0, camPos.y), ogre.Vector3().NEGATIVE_UNIT_Y)
       self.raySceneQuery.setRay(ray)
       self.raySceneQuery.setSortByDistance(False)
 
       # Perform the scene query
       result = self.raySceneQuery.execute()
       for item in result:
           # Result of this query is a list of worldFragments and a list of movables
           # Find the terrain-fragment
 
           if item.worldFragment != None:
               terrainHeight = item.worldFragment.singleIntersection.y
               if (terrainHeight + 10.0) > camPos.y:
                   self.camera.setPosition(camPos.x, terrainHeight + 10.0, camPos.z)
       return True
 
    def mouseMoved(self, evt):
        CEGUI.System.getSingleton().injectMouseMove(evt.get_state().X.rel, evt.get_state().Y.rel)
        if self.leftMouseDown:
            # We are dragging the left mouse button
            # Drag the object if we selected one
            mousePos = CEGUI.MouseCursor.getSingleton().getPosition()
            mouseRay = self.camera.getCameraToViewportRay(mousePos.d_x / float(evt.get_state().width),
                                                          mousePos.d_y / float(evt.get_state().height))
            self.raySceneQuery.setRay(mouseRay)
            self.raySceneQuery.setSortByDistance(False)
 
            result = self.raySceneQuery.execute()
            for item in result:
                if item.worldFragment:
                    self.currentObject.setPosition(item.worldFragment.singleIntersection)
 
        elif self.rightMouseDown:
            self.camera.yaw(ogre.Degree(-evt.get_state().X.rel * self.rotateSpeed))
            self.camera.pitch(ogre.Degree(-evt.get_state().Y.rel * self.rotateSpeed))
        return True
 
    def mousePressed(self, evt, id):
        if id == OIS.MB_Left:
            self.onLeftPressed(evt)
            self.leftMouseDown = True
 
        elif id == OIS.MB_Right:
            CEGUI.MouseCursor.getSingleton().hide()
            self.rightMouseDown = True
        return True
 
    def mouseReleased(self, evt, id):
        if id == OIS.MB_Left:
            self.leftMouseDown = False
        elif id == OIS.MB_Right:
            CEGUI.MouseCursor.getSingleton().show()
            self.rightMouseDown = False
        return True
 
    def onLeftPressed(self, evt):
        if self.currentObject:
            self.currentObject.showBoundingBox(False)
 
 
        # Setup the ray scene query, use CEGUI's mouse position
        mousePos = CEGUI.MouseCursor.getSingleton().getPosition()
        mouseRay = self.camera.getCameraToViewportRay(mousePos.d_x / float(evt.get_state().width),
                                                      mousePos.d_y / float(evt.get_state().height))
        self.raySceneQuery.setRay(mouseRay)
        self.raySceneQuery.setSortByDistance(True)
        if self.robotMode:
            self.raySceneQuery.setQueryMask(self.ROBOT_MASK)
        else:
            self.raySceneQuery.setQueryMask(self.NINJA_MASK)
 
        # Execute query
        result = self.raySceneQuery.execute()
        if len(result) > 0:
            for item in result:
                if item.movable and item.movable.getName()[0:5] != "tile[":
                    self.currentObject = item.movable.getParentSceneNode()
                    break # We found an existing object
 
                elif item.worldFragment:
                    # We have the position we clicked on, create a new
                    # object and place it here
                    if self.robotMode:
                        name = "Robot" + str(self.robotCount)
                        ent = self.sceneManager.createEntity(name, "robot.mesh")
                        ent.setQueryFlags(self.ROBOT_MASK)
                    else:
                        name = "Ninja" + str(self.robotCount)
                        ent = self.sceneManager.createEntity(name, "ninja.mesh")
                        ent.setQueryFlags(self.NINJA_MASK)
 
                    self.robotCount += 1
                    self.currentObject = self.sceneManager.getRootSceneNode().createChildSceneNode(name + "Node", item.worldFragment.singleIntersection)
                    self.currentObject.attachObject(ent)
                    self.currentObject.setScale(0.1, 0.1, 0.1)
        if self.currentObject:
            self.currentObject.showBoundingBox(True)
 
class TutorialApplication(sf.Application): 
    """Application class.""" 
 
    def _chooseSceneManager(self):
        self.sceneManager = self.root.createSceneManager(ogre.ST_EXTERIOR_CLOSE, 'TerrainSM')
 
    def _createScene(self):
        ## CEGUI setup (see Basic Tutorial 7 about this)
        #self.ceguiRenderer = CEGUI.OgreCEGUIRenderer(self.renderWindow, ogre.RENDER_QUEUE_OVERLAY, False, 3000, self.sceneManager)
        #self.ceguiSystem = CEGUI.System(self.ceguiRenderer)
        # initiaslise OgreCEGUI Renderer
        if CEGUI.Version__.startswith ("0.6"):
            self.CEGUIRenderer = CEGUI.OgreRenderer(self.renderWindow,
                    ogre.RENDER_QUEUE_OVERLAY, False, 3000, self.sceneManager)
            self.CEGUIsystem = CEGUI.System(self.GUIRenderer)
        else:
            self.CEGUIRenderer = CEGUI.OgreRenderer.bootstrapSystem()
            self.CEGUIsystem = CEGUI.System.getSingleton()
 
        self.sceneManager.setAmbientLight((0.5, 0.5, 0.5))
        self.sceneManager.setSkyDome(True, "Examples/CloudySky", 5, 8)
 
        # World geometry (Basic tutorial 3)
        self.sceneManager.setWorldGeometry("terrain.cfg")
 
        # Set camera lookpoint
        self.camera.setPosition(40, 100, 580)
        self.camera.pitch(ogre.Degree(-30))
        self.camera.yaw(ogre.Degree(-45))
 
        # Show the mouse cursor
        CEGUI.SchemeManager.getSingleton().loadScheme("TaharezLookSkin.scheme")
        CEGUI.MouseCursor.getSingleton().setImage("TaharezLook", "MouseArrow")
 
    def _createFrameListener(self):
       self.frameListener = MouseQueryListener(self.renderWindow,
                                               self.camera,
                                               self.sceneManager,
                                               self.ceguiRenderer
                                               )
       self.root.addFrameListener(self.frameListener)
       self.frameListener.showDebugOverlay(True)
 
if __name__ == '__main__': 
    ta = TutorialApplication() 
    ta.go()
