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
            /// Create 4 original vertices
            vertexLayer.InsertVertex( new Vertex( -width / 2, height / 2, 0 ) );
            vertexLayer.InsertVertex( new Vertex( width / 2, height / 2, 0 ) );
            vertexLayer.InsertVertex( new Vertex(width / 2, -height / 2, 0) );
            vertexLayer.InsertVertex( new Vertex( -width / 2, -height / 2, 0 ) );

            


        }

        CloverController()
        {
            faceLayer = new FaceLayer( this );
            edgeLayer = new EdgeLayer( this );
            vertexLayer = new VertexLayer( this );

        }
    }
}
