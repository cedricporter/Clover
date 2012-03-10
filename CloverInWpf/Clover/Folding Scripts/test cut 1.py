edge = Edge(Vertex(-50, 50, 0), Vertex(50, -20, 0))
faces = FindFacesByVertex(0)

CutFaces(faces, edge)

faces = FindFacesByVertex(1)
RotateFaces(faces, edge, 30)
RotateFaces(faces, edge, 30)
