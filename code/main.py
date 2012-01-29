import sys 
sys.path.insert(0,'..') 
import ogre.renderer.OGRE as ogre 
import SampleFramework as sf 

class tri(ogre.ManualObject): 
    #simplest possible manual object, for testing purposes 
    #takes 3 tuples A, B, C 
    def __init__(me, name, A, B, C): 
        me.A = A 
        me.B = B 
        me.C = C 
        ogre.ManualObject.__init__(me, name) 
        A, B, C = me.A, me.B, me.C 
        me.clear() 
        me.begin("default", ogre.RenderOperation.OT_TRIANGLE_STRIP) 
        me.position(A[0], A[1], A[2]) 
        me.normal(0, 0, 1) 
        me.position(B[0], B[1], B[2]) 
        me.position(C[0], C[1], C[2]) 
        me.position(A[0], A[1], A[2]) 
        me.position(0, 0, 100)
        me.end() 

class Paper(ogre.ManualObject):
    def __init__(self, name, vertex_list):
        ogre.ManualObject(self, name)
        self.clear()
        self.begin("default", ogre.RenderOperation.OT_TRIANGLE_STRIP)



class TutorialApplication(sf.Application): 

    def _createScene(self): 
        sceneManager = self.sceneManager 
        sceneManager.ambientLight = ogre.ColourValue (0.3,0.3,0.3) 
        self.ent = tri("triangle", (0,0,0), (0,100,0), (100,0,0) ) 
        node1 = sceneManager.getRootSceneNode().createChildSceneNode ("node1") 
        node1.attachObject (self.ent) 
        
    def __del__ ( self ):
        del self.ent
        
        
ta = TutorialApplication() 
ta.go() 
