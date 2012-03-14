import math 

def tan(v):
    return math.tan(v * math.pi / 180.0)

def Update():
    clover.UpdateFaceGroupTable()
    clover.RenderController.UpdateAll()
    
# 对折
edge = Edge(Vertex(-50, 50, 0), Vertex(50, -50, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)
faces = FindFacesByVertex(3)
RotateFaces(faces, edge, 180)
#Update()

# 再对折
edge = Edge(Vertex(0, 0, 0), Vertex(-50, 0, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)
faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 180)
#Update()

# 再对折
edge = Edge(Vertex(0, 0, 0), Vertex(0, -50, 0))
faces = FindFacesByVertex(2)
CutFaces(faces, edge)
faces = FindFacesByVertex(2)
RotateFaces(faces, edge, 180)
#Update()

a = tan(22.5) * 50 - 50

# 中间步骤
edge = Edge(Vertex(0, a, 0), Vertex(-50, -50, 0))
for v in [7, 8]:
    faces = FindFacesByVertex(v)
    CutFaces(faces, edge)
    faces = FindFacesByVertex(v)
    RotateFaces(faces, edge, 180)

edge = Edge(Vertex(a, 0, 0), Vertex(-50, -50, 0))
for v in [4, 6]:
    faces = FindFacesByVertex(v)
    CutFaces(faces, edge)
    faces = FindFacesByVertex(v)
    RotateFaces(faces, edge, 180)

edge = Edge(Vertex(a, 0, 0), Vertex(0, a, 0))
faces = FindFacesByVertex(5)
CutFaces(faces, edge)
faces = FindFacesByVertex(2)
RotateFaces(faces, edge, 180)


