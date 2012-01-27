#!/usr/bin/env python
# This code is Public Domain and was written for Python-Ogre 1.0.
"""Python-Ogre Basic Tutorial 02: Cameras, Lights, and Shadows."""
 
import ogre.renderer.OGRE as ogre
import SampleFramework as sf
 
 
class TutorialApplication(sf.Application):
 
    def _createScene(self):
        sceneManager = self.sceneManager
        sceneManager.ambientLight = (0, 0, 0)
        sceneManager.shadowTechnique = ogre.SHADOWTYPE_STENCIL_ADDITIVE
 
        # Setup a mesh object.
        ent = sceneManager.createEntity('Ninja', 'ninja.mesh')
        ent.castShadows = True
        sceneManager.getRootSceneNode().createChildSceneNode().attachObject(ent)
 
        # Setup a ground plane.
        plane = ogre.Plane ((0, 1, 0), 0)
        meshManager = ogre.MeshManager.getSingleton ()
        meshManager.createPlane ('Ground', 'General', plane,
                                     1500, 1500, 20, 20, True, 1, 5, 5, (0, 0, 1))
        ent = sceneManager.createEntity('GroundEntity', 'Ground')
        sceneManager.getRootSceneNode().createChildSceneNode ().attachObject (ent)
        ent.setMaterialName ('Examples/Rockwall')
        ent.castShadows = False
 
        # Setup a point light.
        light = sceneManager.createLight ('PointLight')
        light.type = ogre.Light.LT_POINT
        light.position = (150, 300, 150)
        light.diffuseColour = (.5, .0, .0)    # Red
        light.specularColour = (.5, .0, .0)
 
        # Setup a distant directional light.
        light = sceneManager.createLight ('DirectionalLight')
        light.type = ogre.Light.LT_DIRECTIONAL
        light.diffuseColour = (.5, .5, .0)    # yellow
        light.specularColour = (.75, .75, .75)
        light.direction = (0, -1, 1)
 
        # Setup a spot light.
        light = sceneManager.createLight ('SpotLight')
        light.type = ogre.Light.LT_SPOTLIGHT
        light.diffuseColour = (0, 0, .5)    # Blue
        light.specularColour = (0, 0, .5)
        light.direction = (-1, -1, 0)
        light.position = (300, 300, 0)
        light.setSpotlightRange (ogre.Degree (35), ogre.Degree (50))
 
    def _createCamera (self):
        self.camera =  self.sceneManager.createCamera ('PlayerCam')
        self.camera.position = (0, 150, -500)
        self.camera.lookAt ((0, 0, 0))
        self.camera.nearClipDistance = 5
 
    def _createViewports (self):
        viewport = self.renderWindow.addViewport (self.camera)
        viewport.backGroundColor = (0, 0, 0)
        self.camera.aspectRatio = float (viewport.actualWidth) / float (viewport.actualHeight)
 
 
if __name__ == '__main__':
    ta = TutorialApplication ()
    ta.go ()
