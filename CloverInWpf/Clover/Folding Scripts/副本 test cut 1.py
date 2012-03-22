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



#----------------------------------------------------------------------------------------------------
edge = Edge(Vertex(-50, 50, 0), Vertex(50, -50, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)
faces = FindFacesByVertex(3)
RotateFaces(faces, edge, 180)

edge = Edge(Vertex(-50, 50, 0), Vertex(50, -50, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)
RotateFaces(faces, edge, 180)





#---------------------------------------------------------

edge = Edge(Vertex(0, 50, 0), Vertex(-50, 30, 0))
faces = FindFacesByVertex(0)
_CutFaces(faces, edge)

faces = FindFacesByVertex(0)
_RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()


edge = Edge(Vertex(0, 50, 0), Vertex(50, 30, 0))
faces = FindFacesByVertex(3)
_CutFaces(faces, edge)

faces = FindFacesByVertex(3)
_RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()

edge = Edge(Vertex(0, 50, 0), Vertex(0, -50, 0))
faces = FindFacesByVertex(2)
_CutFaces(faces, edge)

faces = FindFacesByVertex(6)
_RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()

edge = Edge(Vertex(-25, 40, 0), Vertex(-25, -50, 0))
faces = FindFacesByVertex(6)
_CutFaces(faces, edge)

faces = FindFacesByVertex(6)
_RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()



#edge = Edge(Vertex(0, 50, 0), Vertex(50, 30, 0))
#faces = FindFacesByVertex(3)
#CutFaces(faces, edge)

#faces = FindFacesByVertex(3)
#RotateFaces(faces, edge, 180)

#clover.UpdateFaceGroupTable()