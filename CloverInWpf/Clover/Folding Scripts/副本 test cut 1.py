edge = Edge(Vertex(-50, 0, 0), Vertex(50, 0, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)
faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 180)


edge = Edge(Vertex(0, 0, 0), Vertex(-48, -50, 0))
faces = FindFacesByVertex(4)
CutFaces(faces, edge)
faces = FindFacesByVertex(4)
RotateFaces(faces, edge, 180)

