""" 
A basic mouse picker, the superclass of all other tools.

Create by kid at 2012/2/3 
"""

class BasicPicker:
    
    #
    def onMove(self):
        print "moving.."
    
    #
    def onPress(self):
        pass
    
    #
    def onRelease(self):
        pass
    
    # 
    def _onFaceOvered(self):
        pass
    #
    def _onEdgeOvered(self):
        pass
    #
    def _onVertexOvered(self):
        pass
    #
    def _onFaceSelected(self):
        pass
    #
    def _onEdgeSelected(self):
        pass
    #
    def _onVertexSelected(self):
        pass
    
    #
    def _hitTest(self):
        """
        Test if the mouse is above any clover elements.
        """
        pass
    
    # constructor
    def __init__(self):
        self.overedElement = None
        self.selectedElement = None
    
    

