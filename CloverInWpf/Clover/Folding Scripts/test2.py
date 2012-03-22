vertex = GetVertex(0)
face = FindFacesByVertex(0)[0]
edge = Edge(Vertex(-50, 0, 0), Vertex(50, 0, 0))

CutFace2(face, edge)

faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 90)

clover.ShadowSystem.Undo()

edge = Edge(Vertex(0, 0, 0), Vertex(0, -50, 0))
CutFace2(FindFacesByVertex(0)[0], edge)
CutFace2(FindFacesByVertex(1)[0], edge)

faces = FindFacesByVertex(4)
RotateFaces(faces, edge, 90)





##############


edgeMiddle = Edge(Vertex(-50, 0, 0), Vertex(50, 0,0))
face = FindFacesByVertex(0)
CutFaces(face, edgeMiddle)

edge = Edge(Vertex(50, 10, 0), Vertex(10, 50,0))
face = clover.FindFacesByVertex(3)
CutFaces(face, edge)

faces = FindFacesByVertex(3)
RotateFaces(faces, edge, 180)


edge = Edge(Vertex(-50, 30, 0), Vertex(30, 30,0))
face = FindFacesByVertex(7)
CutFaces(face, edge)

faces = FindFacesByVertex(7)
RotateFaces(faces, edge, 90)