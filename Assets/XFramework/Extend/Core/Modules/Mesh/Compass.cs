using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFramework.Draw
{
    public class Compass : MonoBehaviour
    {
        private Material shapesMaterial;
        private Vector3 pos;
        private List<int> temp;

        private void Start()
        {
            shapesMaterial = new Material(Shader.Find("RunTimeHandles/Shape"));
            shapesMaterial.color = Color.white;

            temp = new List<int>();
            temp.Add(1);
            temp.Add(2);
            temp.Add(3);
            temp.Add(4);
        }

        private void OnPostRender()
        {
            pos = transform.position + transform.forward * 10 + transform.up * 5 + transform.right * 7;
            shapesMaterial.SetPass(0);
            Graphics.DrawMeshNow(GLDraw.CreateArrow(Color.red, 1), pos, Quaternion.Euler(90, 0, 0));
            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            GL.Vertex(pos);
            GL.Vertex(pos - Vector3.forward * 1);

            GL.End();
        }
    }
}