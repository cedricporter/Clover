
#### New
edge = Edge(Vertex(-70710.6781186547,-70710.6781186547,0), Vertex(70710.6781186547,70710.6781186547,0))
faces = clover.FindFacesByID(0)
CutFaces(faces, edge)
faces = clover.FindFacesByID(2)
RotateFaces(faces, edge, 180)
faceWithFoldLine = List[int]([0])
faceWithoutFoldLine = List[int]([2])
fixFace = List[int]([1])
clover.UpdateTableAfterFoldUp(faceWithFoldLine, faceWithoutFoldLine, fixFace, True)
clover.AntiOverlap()

#### New
edge = Edge(Vertex(-99950,0,0), Vertex(100050,0,0))
faces = clover.FindFacesByID(1)
CutFaces(faces, edge)
faces = clover.FindFacesByID(2)
CutFaces(faces, edge)
faces = clover.FindFacesByID(3)
RotateFaces(faces, edge, 180)
faces = clover.FindFacesByID(6)
RotateFaces(faces, edge, 180)


faceWithFoldLine = List[int]([1,2])
faceWithoutFoldLine = List[int]([3,6])
fixFace = List[int]([4,5])
clover.UpdateTableAfterFoldUp(faceWithFoldLine, faceWithoutFoldLine, fixFace, True)
clover.AntiOverlap()
ceilingFace = clover.FindFacesByID(2)
floorFace = clover.FindFacesByID(1)
clover.UpdateTableAfterTucking(ceilingFace, floorFace)