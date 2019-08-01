// ==========================================
// 描述： 
// 作者： HAK
// 时间： 2018-10-31 13:13:54
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using XFramework;
using UnityEngine;
using XFramework.Mathematics;

public class MeshTest : ProcedureBase
{
    List<Vector3> positions = new List<Vector3>();

    /// <summary>
    /// 扇形区域横向角
    /// </summary>
    public float alpha = 30;
    /// <summary>
    /// 扇形区域纵向角
    /// </summary>
    public float theta = 30;
    /// <summary>
    /// 矩形高度
    /// </summary>
    public float polygonHeight = 500;
    /// <summary>
    /// 走廊宽度
    /// </summary>
    public float AirSpaceWidth = 600;
    /// <summary>
    /// 走廊高度
    /// </summary>
    public float AirSpaceHeight = 600;
    public float pointHeight = 100;

    public Color cylinderColor = new Color(0, 0, 1, 0.9f);
    public Color polygonColor = new Color(1, 0, 0, 0.9f);
    public Color airCorridorSpaceColor = new Color(0, 1, 0, 0.9f);
    public Color sectorColor = new Color(1, 0.92f, 0.016f, 0.9f);

    public override void Init()
    {
        Utility.CreatPrimitiveType(PrimitiveType.Plane).transform.localScale = new Vector3(5000, 1, 5000);
        Camera.main.transform.position = new Vector3(-1630, 33323, -28800);
        Camera.main.transform.eulerAngles = new Vector3(60, 0, 0); 
    }

    public override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 worldHitPos = hit.point + new Vector3(0, pointHeight, 0);
                positions.Add(worldHitPos);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Revert();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            //SetFalse(--id);
        }

        if (positions == null || positions.Count == 0)
        {
            return;
        }

        //圆柱
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreateCylinder();
            Revert();
        }
        //多边形
        if (Input.GetKeyDown(KeyCode.X) && positions.Count > 2)
        {
            CreatePolygon();
            Revert();
        }
        //空中走廊
        if (Input.GetKeyDown(KeyCode.C) && positions.Count > 1)
        {
            CreateAirCorridorSpace();
            Revert();
        }
        //扇形区域
        if (Input.GetKeyDown(KeyCode.V) && positions.Count > 1)
        {
            CreatSector_2();
            Revert();
        }
        //半圆防空区域
        if (Input.GetKeyDown(KeyCode.B))
        {
            CreateHemisphere();
            Revert();
        }
        //两个圆柱，近距离等待空域
        if (Input.GetKeyDown(KeyCode.N))
        {
            CreatDoubleCylinder();
            Revert();
        }
        //杀伤盒 两个矩形 
        if (Input.GetKeyDown(KeyCode.M) && positions.Count > 2)
        {
            CreatKillBox();
            Revert();
        }
    }

    public override void OnEnter()
    {
        MonoEvent.Instance.ONGUI += OnGUI;
    }

    public override void OnExit()
    {
        MonoEvent.Instance.ONGUI -= OnGUI;
    }

    public void OnGUI()
    {
        GUIStyle style = new GUIStyle
        {
            padding = new RectOffset(10, 10, 10, 10),
            fontSize = 15,
            fontStyle = FontStyle.Normal,
        };
        GUI.Label(new Rect(0, 0, 200, 80), 
            "1.鼠标左键点击设置关键点\n" +
            "2.R:清空之前设置的点\n" +
            "3.Z:创建一个圆柱\n" +
            "4.X:创建一个多边形\n" +
            "5.C:创建一条空中走廊\n" +
            "6.V:创建一个扇形区域\n" +
            "7.B:创建一个半球\n" +
            "8.V:创建两个圆柱\n", style);
    }

    private void CreateCylinder()
    {
        Game.MeshModule.CreateCylinder(positions[0], 2500, 1500, cylinderColor);
    }

    private void CreatePolygon()
    {
        Game.MeshModule.CreatePolygon(positions, 1000f, polygonColor);
    }

    private void CreateAirCorridorSpace()
    {
        Game.MeshModule.CreateAirCorridorSpace(positions, AirSpaceWidth, AirSpaceHeight, airCorridorSpaceColor);
    }

    private void CreatSector_2()
    {
        Vector3[] points = PhysicsMath.GetSectorPoints_2(positions[0], new Vector3(positions[1].x, positions[0].y, positions[1].z), alpha, theta);
        Game.MeshModule.CreateSector(positions[0], positions[1], alpha, theta, sectorColor);
    }

    private void CreateHemisphere()
    {
        Game.MeshModule.CreateHemisphere(positions[0] - Vector3.up * 1500, 7000, new Color(0.784f, 0.784f, 1));
    }

    private void CreatKillBox()
    {
        Game.MeshModule.CreateKillBox(positions, 3400, 6000, new Color(0.706f, 0.235f, 1));
    }

    private void CreatDoubleCylinder()
    {
        Game.MeshModule.DoubleCylinder(positions[0], 2500, 1500, 1000, new Color(1, 0.392f, 0));
    }

    /// <summary>
    /// 清空所有点
    /// </summary>
    private void Revert()
    {
        positions.Clear();
        positions = new List<Vector3>();
    }
}
