"""
a cube navigator

Create by kid at 2012/1/31
"""

import ogre.renderer.OGRE as ogre
import ogre.gui.CEGUI as CEGUI
import math


"""   cube by kid..
      4 ______7
      /|      /
   0 /_|___3 /|
     | |____|_|
     | /5   | /6
    1|/____2|/    
"""
# point list
PL = [[-20, 20, 20], [-20, -20, 20], [20, -20, 20], [20, 20, 20],
      [-20, 20, -20], [-20, -20, -20], [20, -20, -20], [20, 20, -20]]
# index list /ft/bk/lt/rt/up/dn
IL = [[0,1,3], [3,1,2],
      [7,6,4], [4,6,5],
      [4,5,0], [0,5,1],
      [3,2,7], [7,2,6],
      [4,0,7], [7,0,3],
      [1,5,2], [2,5,6]]
# height light index list
HIL = [[0,1,3,2], [7,6,4,5], [4,5,0,1],
       [3,2,7,6], [4,0,7,3], [1,5,2,6]]
# face name
#FL = ['ft','ft','bk','bk','lt','lt','rt','rt','up','up','dn','dn']
# texture coordinates list
TC = [[[0.6667, 0], [0.6667, 1], [0.8333, 0]],
      [[0.8333, 0],[0.6667, 1], [0.8333, 1]],
      [[0.833, 0], [0.833, 1], [1, 0]], 
      [[1, 0], [0.833, 1], [1, 1]],
      [[0.3333, 0], [0.3333, 1], [0.5, 0]], 
      [[0.5, 0], [0.3333, 1], [0.5, 1]],
      [[0.5, 0], [0.5, 1], [0.6667, 0]], 
      [[0.6667, 0], [0.5, 1], [0.6667, 1]],
      [[0, 0], [0, 1], [0.1667, 0]], 
      [[0.1667, 0], [0, 1], [0.1667, 1]],
      [[0.1667, 0], [0.1667, 1], [0.3333, 0]], 
      [[0.3333, 0], [0.1667, 1], [0.3333, 1]]]

class CubeNavigator(ogre.ManualObject):
    # when over one of the six face, height light the face
    def onMove(self, colPoint, camera):
        # first determine which face the mouse is on
        rotMatrix = self.getParentSceneNode().getLocalAxes()
        worldPos = self.getParentNode().getPosition()
        # calculate the distance between collition point and all vertices
        posList = []
        for i in range(0, 8):
            tV = ogre.Vector3(PL[i][0],PL[i][1],PL[i][2])
            tV = rotMatrix * tV + worldPos
            posList.append([colPoint.distance(tV), i])
        posList.sort()
        # use a check sum to verify wich face is hited
        self.overedFace = None
        checkSum = math.sqrt(posList[0][1])+math.sqrt(posList[1][1])+math.sqrt(posList[2][1])
        for i in range(0,12):
            if math.sqrt(IL[i][0])+math.sqrt(IL[i][1])+math.sqrt(IL[i][2]) == checkSum:
                self.overedFace = i / 2
                #print self.overedFace
                break
        # height light
        if self.heightLight.getParentSceneNode() == None:
            self.getParentSceneNode().attachObject(self.heightLight)
        if self.overedFace != None:
            self.beginUpdate(1)
            #self.begin("CubeHeightLight", ogre.RenderOperation.OT_TRIANGLE_STRIP)
            for vertex in range(0, 4):
                self.position(PL[HIL[self.overedFace][vertex]][0], 
                              PL[HIL[self.overedFace][vertex]][1], 
                              PL[HIL[self.overedFace][vertex]][2])
                #self.colour(1,1,0.3)
            self.end()
        else:
            self.beginUpdate(1)
            self.position(0,0,0)
            self.end()
        
        """self.getParentSceneNode().getOrientation().ToRotationMatrix(rotMat)
        viewMat = camera.getViewMatrix()
        projMat = camera.getProjectionMatrixWithRSDepth()
        viewPort = camera.getViewport()
        wp = rotMat*ogre.Vector3(-20,20,20) + self.getParentNode().getPosition()
        #print viewPort.getActualLeft(), viewPort.getActualTop(), viewPort.getActualWidth(), viewPort.getActualHeight()
        pos3D = projMat*viewMat*ogre.Vector4(colPoint.x,colPoint.y,colPoint.z,0)
        print pos3D
        #print rotMat*ogre.Vector3(-20,20,20) + self.getParentNode().getPosition()
        #print colPoint
        pos2D = [0, 0]
        for i in range(0,2):
            pos2D[i] = pos3D[i] / pos3D[3]
        pos2D[0] = (1 + pos2D[0]) * viewPort.getActualWidth() / 2
        pos2D[1] = (1 + pos2D[1]) * viewPort.getActualHeight() / 2
        print pos2D"""

    
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
        
    # when draged, present navigation
    def onDrag(self):
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
        #ogreMouseVec.normalise()
        rotateAxis = ogreMouseVec.crossProduct(ogre.Vector3(0, 0, -1))
        rotateAxis.normalise()
        # add by kid ======>>
        #rotMat = ogre.Matrix3()
        #self.lastOrientation.ToRotationMatrix(rotMat)
        #rotateAxis =  rotMat * rotateAxis
        #print rotateAxis
        # <<====== add by kid
        # determine the rotate degree
        rotateDegree = math.sqrt(math.pow(mouseOffset.d_x, 2.0)
                                 + math.pow(mouseOffset.d_y, 2.0)) / 100.0
        # use the axis and degree to create quaternion
        quat = ogre.Quaternion(rotateDegree, rotateAxis)
        quat = self.lastOrientation * quat
        self.getParentSceneNode().setOrientation(quat)        
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
        self.overedFace = None
        self.heightLight = ogre.ManualObject("CubeNavHeightLight")
        #self.setUseIdentityProjection(True)
        #self.setUseIdentityView(True)
        # create materials
        material = ogre.MaterialManager.getSingleton().create("CubeNavMat", "General")
        ipass = material.getTechnique(0).getPass(0)
        ipass.setLightingEnabled(False)
        ipass.setDepthCheckEnabled(False)
        ipass.createTextureUnitState("CubeNavTex.png")
        #for height light
        material = ogre.MaterialManager.getSingleton().create("CubeHeightLight", "General")
        material.setDepthCheckEnabled(False)
        material.setLightingEnabled(False)
        material.setSceneBlending (ogre.SBT_MODULATE)
        # sequence: ft/bk/lt/rt/up/dn
        self.setDynamic(True)
        self.begin("CubeNavMat", ogre.RenderOperation.OT_TRIANGLE_LIST)
        for face in range(0, 12):
            for vertex in range(0, 3):
                self.position(PL[IL[face][vertex]][0], PL[IL[face][vertex]][1], PL[IL[face][vertex]][2])
                self.textureCoord(TC[face][vertex][0], TC[face][vertex][1])     
        self.end()
        #for height light
        self.begin("CubeHeightLight", ogre.RenderOperation.OT_TRIANGLE_STRIP)
        self.position(0,0,0); 
        self.colour(1,1,0.3)  #this value decide the color
        self.end()
        
        """# front
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
        self.end()"""



