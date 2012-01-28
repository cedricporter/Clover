import sys 
sys.path.insert(0,'..') 
import ogre.renderer.OGRE as ogre 
import SampleFramework as sf 

class tri(ogre.ManualObject): 
    #simplest possible manual object, for testing purposes 
    #takes 3 tuples A, B, C 
    def __init__(self, name, A, B, C): 
        self.A = A 
        self.B = B 
        self.C = C 
        ogre.ManualObject.__init__(self, name) 
        A, B, C = self.A, self.B, self.C 
        self.clear() 
        self.begin("default", ogre.RenderOperation.OT_TRIANGLE_STRIP) 
        self.position(A[0], A[1], A[2]) 
        self.normal(0, 0, 1) 
        self.position(B[0], B[1], B[2]) 
        self.position(C[0], C[1], C[2]) 
        self.position(A[0], A[1], A[2]) 
        self.end() 

class TutorialApplication(sf.Application): 

    def _createScene(self): 
        sceneManager = self.sceneManager 
        sceneManager.ambientLight = (0.3,0.3,0.3) 
        self.ent = tri("triangle", (0,0,0), (0,100,0), (100,0,0) ) 
        node1 = sceneManager.getRootSceneNode().createChildSceneNode ("node1") 
        node1.attachObject (self.ent) 
        
    def __del__ ( self ):
        del self.ent
        
        
ta = TutorialApplication() 
ta.go() 
