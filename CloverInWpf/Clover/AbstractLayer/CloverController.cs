using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class CloverController
    {
        FaceLayer faceLayer;
        EdgeLayer edgeLayer;
        VertexLayer vertexLayer; 

        public void Initialize(float width, float height)
        {
            // Create 4 original vertices
            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex( -width / 2, height / 2, 0 );
            vertices[1] = new Vertex( width / 2, height / 2, 0 );
            vertices[2] = new Vertex( width / 2, -height / 2, 0 );
            vertices[3] = new Vertex( -width / 2, -height / 2, 0 );

            // add to vertex layer
            foreach (Vertex v in vertices)
            {
                vertexLayer.InsertVertex( v );
            }

            Edge[] edges = new Edge[4];
            for (int i = 0; i < 4; i++)
            {
                edges[ i ] = new Edge( vertices[ i ], vertices[ i + 1 < 4 ? i + 1 : 0] );
            }




        }

        public CloverController()
        {
            faceLayer = new FaceLayer( this );
            edgeLayer = new EdgeLayer( this );
            vertexLayer = new VertexLayer( this );

        }
    }
}
