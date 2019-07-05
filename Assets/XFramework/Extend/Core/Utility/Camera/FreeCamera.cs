using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    /// <summary>
    /// 旋转速度
    /// </summary>
    public int rotateSpeed = 500;
    /// <summary>
    /// 移动速度
    /// </summary>
    public float moveSpeed = 50;
    /// <summary>
    /// 按住Shift加速倍数
    /// </summary>
    private int shiftRate;

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            // 转相机朝向
            transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);

            float targetAngeTo = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            transform.RotateAround(transform.position, transform.right, targetAngeTo);

            // 倍速
            if (Input.GetKey(KeyCode.LeftShift))
                shiftRate = 2;
            else
                shiftRate = 1;

            float acceleration = Input.GetAxis("Mouse ScrollWheel");
            moveSpeed += acceleration * moveSpeed;

            // 移动
            transform.Translate(shiftRate * GetDirection() * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// 获取相机移动与方向
    /// </summary>
    private Vector3 GetDirection()
    {
        Vector3 dir = Vector3.zero;

        // 获取按键输入
        if (Input.GetKey(KeyCode.W))
        {
            dir += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dir -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dir -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dir += transform.right;
        }
        if (Input.GetKey(KeyCode.E))
        {
            dir += transform.up;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            dir -= transform.up;
        }

        return dir;
    }
}
