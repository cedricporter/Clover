sign = 1
j = 0
for i in range(-40, 50, 10): 
    edge = Edge(Vertex(i + 10 * j, -55, j * 10), Vertex(i + 10 * j, 55, j * 10))
    faces = FindFacesByVertex(2)
    CutFaces(faces, edge)

    faces = FindFacesByVertex(2)
    RotateFaces(faces, edge, sign * 90)
    sign *= -1
    if i == -1: j += 1


x, z = -40, 0

for i in range(10):
    if sign: x += 10
    else: z += 10
    
    x += 10 * (1 - sign)
    z += 10 * sign
    
    sign = not sign
    
    
sign = 1
x,z = -40, 0
for i in range(9):
    print "=>", x, z, sign
    
    edge = Edge(Vertex(x, -55, z), Vertex(x, 55, z))
    
    faces = FindFacesByVertex(2)
    CutFaces(faces, edge)

    faces = FindFacesByVertex(2)
    RotateFaces(faces, edge, (2 * sign - 1) * 90)

    x += 10 * (1 - sign)
    z += 10 * sign

    sign = not sign