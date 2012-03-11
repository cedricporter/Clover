edge = Edge(Vertex(-50, 50, 0), Vertex(50, -20, 0))
faces = FindFacesByVertex(0)

CutFaces(faces, edge)

faces = FindFacesByVertex(1)
RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()

#----------------------------------------------------------------


edge = Edge(Vertex(-50, 50, 0), Vertex(50, -20, 0))
faces = FindFacesByVertex(0)

CutFaces(faces, edge)

faces = FindFacesByVertex(1)
RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()

edge = Edge(Vertex(0, 50, 0), Vertex(50, 20, 0))
faces = FindFacesByVertex(3)

CutFaces(faces, edge)

faces = FindFacesByVertex(3)
RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()