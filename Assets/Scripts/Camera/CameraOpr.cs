using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOpr : MonoBehaviour
{

    private Camera mainCamera;

    [Range(0, 2000)]
    //基础移动速度
    private float m_translateSpeed = 200f;

    //相机最低、最高高度
    private float m_cameraMinimuxHeight = 3.0f;
    private float m_cameraMaximumHeight = 12000.0f;

    //相机旋转速度
    private float m_rotateSpeed = 90.0f;
    
    //相机位置变化标志
    private bool m_isPosChanged = false;
    private bool m_isVerticalChange = false;
    
    // 镜头角度
    private float m_targetAngel = 0;

    //相机位置变化方向向量
    private Vector3 m_velocity = Vector3.zero;

    private Vector3 m_detalVector;

    //射线碰撞信息
    RaycastHit m_raycastHit = new RaycastHit();

    private float m_xLimitMax;
    private float m_zLimitMax;
    private float m_xLimitMin;
    private float m_zLimitMin;

    //是否给相机加上碰撞
    private bool currentUseCollider = false;
    public bool nextUseCollider = false;

    [SerializeField]
    private float terrainHeight = 0;

    void Start()
    {
        //获取相机和地表对象
        mainCamera = Camera.main;
        GetMoveLimit();
    }

    void GetMoveLimit()
    {
        m_xLimitMax = 56000f + 10000f;
        m_zLimitMax = 56000f + 10000f;
        m_xLimitMin = 0f - 10000f;
        m_zLimitMin = 0f - 10000f;
    }

    void Update()
    {
        m_velocity = Vector3.zero;
        m_isPosChanged = false;

        //计算相机在三个方向分量上的变化
        CameraVertical();
        CameraHorizontal();
        CameraLongitudinal();

        CameraPushAndPull();

        //改变镜头位置
        if (m_isPosChanged)
        {
            //获取不同条件组合下的速度变化倍率
            float cameraSpeed = ComputeCameraSpeed();

            //设置相机位置
            m_detalVector = m_velocity * cameraSpeed * Time.deltaTime;
            CameraOverstepProcess();
            mainCamera.transform.Translate(m_detalVector,Terrain.activeTerrain.transform);
            
            //限制镜头高度
            CameraHeightLimit();
        }

        //控制镜头的旋转
        CameraRotate();

    }

    //镜头旋转
    void CameraRotate()
    {
        if (Input.GetMouseButton(1))
        {
            //镜头旋转可以360度任意旋转 
            mainCamera.transform.RotateAround(mainCamera.transform.position, Vector3.up, Input.GetAxis("Mouse X") * m_rotateSpeed * Time.deltaTime);

            //镜头的俯仰在（-10，60）之间
            float eulerAngles_x = mainCamera.transform.eulerAngles.x;
            float targetAngel = eulerAngles_x - Input.GetAxis("Mouse Y") * m_rotateSpeed * Time.deltaTime;

            //if ((0f < targetAngel && 60f > targetAngel) || (350f < targetAngel && 360f > targetAngel))
            //{
                mainCamera.transform.RotateAround(mainCamera.transform.position, mainCamera.transform.right, -Input.GetAxis("Mouse Y") * m_rotateSpeed * Time.deltaTime);
            //}
        }
        else if (m_isVerticalChange)
        {
            float camera2surfaceHeight = GetCamera2SurfaceHeight();

            m_targetAngel = 0.0241379f * camera2surfaceHeight - 1.206896f;
            if (0 < m_targetAngel && m_targetAngel < 60)
            {
                float eulerAngles_x = mainCamera.transform.eulerAngles.x;
                float detalAngel = m_targetAngel - eulerAngles_x;
                mainCamera.transform.RotateAround(mainCamera.transform.position, mainCamera.transform.right, detalAngel * Time.deltaTime);
            }
        }
    }

    //镜头左右方向水平移动
    void CameraHorizontal()
    {
        if (Input.GetKey(KeyCode.A)) //A键左移
        {
            m_velocity -= mainCamera.transform.right;
            m_isPosChanged = true;
        }
        if (Input.GetKey(KeyCode.D)) //D键右移
        {
            m_velocity += mainCamera.transform.right;
            m_isPosChanged = true;
        }
    }

    //镜头前后方向水平移动
    void CameraLongitudinal()
    {
        if (Input.GetKey(KeyCode.W)) //W键前移
        {

            m_velocity += Vector3.Normalize(mainCamera.transform.forward + mainCamera.transform.up * (Mathf.Tan(mainCamera.transform.eulerAngles.x * Mathf.Deg2Rad)));
            m_isPosChanged = true;
        }

        if (Input.GetKey(KeyCode.S)) //S键后移
        {

            m_velocity -= Vector3.Normalize(mainCamera.transform.forward + mainCamera.transform.up * (Mathf.Tan(mainCamera.transform.eulerAngles.x * Mathf.Deg2Rad)));
            m_isPosChanged = true;

        }
    }

    //镜头垂直方向移动
    void CameraVertical()
    {
        m_isVerticalChange = false;
        if (Input.GetKey(KeyCode.E)) //E键上移
        {
            m_velocity += Vector3.up;
            m_isPosChanged = true;
            m_isVerticalChange = true;
        }
        if (Input.GetKey(KeyCode.Q)) //Q键向下移
        {
            m_velocity -= Vector3.up;
            m_isPosChanged = true;
            m_isVerticalChange = true;
        }
    }

    //镜头高度限制在(m_cameraMinimuxHeight,m_cameraMaximumHeight)之间
    void CameraHeightLimit()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraHeight = cameraPos.y;
        //相机绝对高度大于500才开始判断
        if (cameraHeight < 3000)
        {
            if (Physics.Raycast(cameraPos, Vector3.down, out m_raycastHit, 320f, LayerMask.GetMask("Terrain")))
            {
                //距离地面距离小于最下高度，则将相机抬升到最下高度
                if (m_raycastHit.point.y + m_cameraMinimuxHeight > cameraHeight)
                {
                    cameraPos.y = m_raycastHit.point.y + m_cameraMinimuxHeight;
                    mainCamera.transform.position = cameraPos;
                }

                Vector3 cameraVector = mainCamera.transform.forward;
                cameraVector.y = 0;
                cameraVector = Vector3.Normalize(cameraVector);
                cameraVector.y = 0.57735f;

                RaycastHit raycastHit = new RaycastHit();
                if (Physics.Raycast(cameraPos, cameraVector, out raycastHit, 2 * m_cameraMinimuxHeight, LayerMask.GetMask("Terrain", "Table")))
                {
                    cameraPos.y = raycastHit.point.y + m_cameraMinimuxHeight;
                    mainCamera.transform.position = cameraPos;
                }
            }
            else
            {
                //若向下碰撞失败，则相机可能在地形的下侧，做向上碰撞
                RaycastHit raycastHit = new RaycastHit();
                if (Physics.Raycast(cameraPos, Vector3.up, out raycastHit, 2500f, LayerMask.GetMask("Terrain", "Table")))
                {
                    cameraPos.y = m_raycastHit.point.y + m_cameraMinimuxHeight;
                    mainCamera.transform.position = cameraPos;
                }
                else
                {
                    //上下皆无碰撞，则相机可能在地图外侧，
                    if (m_raycastHit.point.y + m_cameraMinimuxHeight > cameraHeight)
                    {
                        cameraPos.y = m_raycastHit.point.y + m_cameraMinimuxHeight;
                        mainCamera.transform.position = cameraPos;
                    }
                }
            }
        }
        //若高于最高高度，将相机设回最高高度
        else if(cameraHeight > m_cameraMaximumHeight)
        {
            cameraPos.y = m_cameraMaximumHeight;
            mainCamera.transform.position = cameraPos;
        }
    }

    float ComputeCameraSpeed()
    {
        float cameraSpeed = Mathf.Sqrt(Mathf.Abs(transform.position.y)) * 5f;
        //左shift键 加速一倍
        if (Input.GetKey(KeyCode.LeftShift)) cameraSpeed *= 2;
        //滚轮滚动加速
        cameraSpeed *= (1 + 10 * Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")));
        //根据高度调节速度
        float Height2Surface = GetCamera2SurfaceHeight();
        if (Height2Surface != -1)
        {
            float multipleByHeight2Surface = 0.004545454f * Height2Surface + 0.9090909f;
            cameraSpeed *= multipleByHeight2Surface;
        }
        return cameraSpeed;
    }

    void CameraOverstepProcess()
    {

        Vector3 camPos = mainCamera.transform.position;

        float targetXPos = camPos.x + m_detalVector.x;

        if (targetXPos > m_xLimitMax || m_xLimitMin > targetXPos)
        {
            m_detalVector.x = 0;
        }

        float targetZPos = camPos.z + m_detalVector.z;

        if (targetZPos > m_zLimitMax || m_zLimitMin > targetZPos)
        {
            m_detalVector.z = 0;
        }

    }

    //获取镜头距离地表的高度
    //-1：碰撞失败，镜头在地图之外
    //其他值：镜头距离地表的高度
    float GetCamera2SurfaceHeight()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Physics.Raycast(cameraPos, Vector3.down, out m_raycastHit, m_cameraMaximumHeight + 2f, LayerMask.GetMask("Terrain", "Table"));
        float height = cameraPos.y - m_raycastHit.point.y;
        return height;
    }

    private int m_time = 0;
    private Vector3 m_mouseVector;

    //滚轮控制相机推拉
    void CameraPushAndPull()
    {
        float srcollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (srcollWheel == 0 && m_time == 0)
        {
            return;
        }
        else if (srcollWheel != 0)
        {
            m_time = 30;

            // 滚轮前推
            if (Mathf.Sign(srcollWheel) > 0)
            {
                m_mouseVector = mainCamera.ScreenPointToRay(Input.mousePosition).direction;   //获取相机到鼠标点的方向向量
                m_mouseVector.y *= (50f / m_time);
                m_mouseVector *= (40f / m_time);

            }
            else
            {
                m_mouseVector = mainCamera.transform.forward;
                m_mouseVector.y *= (150f / m_time);
            }
            m_mouseVector *= Mathf.Sign(srcollWheel);
        }

        m_isPosChanged = true;
        m_isVerticalChange = true;

        m_velocity += m_mouseVector;

        m_time--;
    }
}