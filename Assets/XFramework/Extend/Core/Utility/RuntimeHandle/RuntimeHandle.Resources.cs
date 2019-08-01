using UnityEngine;

namespace XFramework.Draw
{
    public partial class RuntimeHandle
    {
        protected class Resources
        {
            public Material lineMat;
            public Material quadeMat;
            public Material shapMatRed;
            public Material shapMatBlue;
            public Material shapMatGreen;
            public Material shapMatSelected;

            public Mesh arrowMesh;
            public Mesh cubeMesh;

            public Resources(Color selectedColor)
            {
                lineMat = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                lineMat.color = Color.white;

                quadeMat = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                quadeMat.color = Color.white;

                shapMatRed = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                shapMatRed.color = Color.red;
                shapMatBlue = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                shapMatBlue.color = Color.blue;
                shapMatGreen = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                shapMatGreen.color = Color.green;
                shapMatSelected = new Material(Shader.Find("RunTimeHandles/VertexColor"));
                shapMatSelected.color = selectedColor;

                arrowMesh = GLDraw.CreateArrow(Color.white, 1);
                cubeMesh = GLDraw.CreateCube(Color.white, Vector3.zero, 1);
            }
        }
    }
}