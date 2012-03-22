"""
A simple mouse picker, can select and high light elements.

Create by kid at 2010/2/3
"""
from BasicPicker import BasicPicker

class SimplePicker(BasicPicker):
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
    
    def onMove(self):
        BasicPicker.onMove(self)