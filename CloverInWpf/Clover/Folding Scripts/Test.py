vertex = GetVertex(0)
face = FindFacesByVertex(0)[0]
edge = GetFoldingLine(face, vertex, Vertex(0, 0))

CutFace2(face, edge)

faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 90)


edge = GetFoldingLine(face, vertex, Vertex(0, 0))