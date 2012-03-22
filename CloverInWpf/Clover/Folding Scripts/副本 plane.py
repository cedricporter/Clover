#
# 2B飞机 By ET
#

# 先在中间画条折线
edgeMiddle = Edge(Vertex(-50, 0, 0), Vertex(50, 0,0))
faces = FindFacesByVertex(0)
CutFaces(faces, edgeMiddle)

for v1, v2, v3, sign in [(3, 0, 7, 1)]:
    # 左上角往回折
    edge = Edge(Vertex(50, sign * 10, 0), Vertex(10, sign * 50,0))
    faces = FindFacesByVertex(v1)
    CutFaces(faces, edge)

    faces = FindFacesByVertex(v1)
    RotateFaces(faces, edge, 180)

    # 上面1/4反折
    edge = Edge(Vertex(-50, sign * 30, 0), Vertex(30, sign * 30,0))
    face = FindFacesByVertex(v3)
    CutFaces(face, edge)

    # 翅膀往回折
    faces = FindFacesByVertex(v3)
    RotateFaces(faces, edge, sign * 90)
    
for v1, v2, v3, sign in [(2, 1, 11, -1)]:
    # 左上角往回折
    edge = Edge(Vertex(50, sign * 10, 0), Vertex(10, sign * 50,0))
    faces = FindFacesByVertex(v1)
    CutFaces(faces, edge)

    faces = FindFacesByVertex(v1)
    RotateFaces(faces, edge, 180)

    # 上面1/4反折
    edge = Edge(Vertex(-50, sign * 30, 0), Vertex(30, sign * 30,0))
    face = FindFacesByVertex(v3)
    CutFaces(face, edge)

    # 翅膀往回折
    faces = FindFacesByVertex(v3)
    RotateFaces(faces, edge, sign * 90)
    
# 对折
faces = FindFacesByVertex(9)
RotateFaces(faces, edgeMiddle, -180)
