using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace XFramework.Mathematics
{
    /// <summary>
    /// 和2d数学相关的算法
    /// </summary>
    public static class Math2d
    {
        #region 线段相关

        /// <summary>
        /// 判断两线段的是否相交
        /// </summary>
        /// <param name="intersection">交点</param>
        /// <param name="point_0"></param>
        /// <param name="point_1"></param>
        /// <param name="point_2"></param>
        /// <param name="point_3"></param>
        /// <returns></returns>
        public static bool SegmentIntersection(out Vector2 intersection, Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1)
        {
            // 判断相交性
            Vector3 vec_0 = q1 - q0;   // 线段q的方向
            Vector3 vec_1 = q0 - p0;   // 线段q到p两个端点连线的方向
            Vector3 vec_2 = q0 - p1;
            if (Vector3.Dot(Vector3.Cross(vec_0, vec_1), Vector3.Cross(vec_0, vec_2)) > 0)
            {
                intersection = Vector2.zero;
                return false;
            }




            intersection = Vector2.zero;
            return false;
        }

        #endregion

        #region 二维数组

        /// <summary>
        /// 高斯核的静态存储
        /// </summary>
        private static Dictionary<int, float[,]> gaussianCoreDic = new Dictionary<int, float[,]>();

        /// <summary>
        /// 双线性差值
        /// </summary>
        /// <param name="array_In"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static async Task<float[,]> ZoomBilinearInterpAsync(float[,] array_In, int newWidth, int newHeight)
        {
            int originalHeight = array_In.GetLength(0);
            int originalWidth = array_In.GetLength(1);

            float scaleX = ((float)newHeight) / ((float)originalHeight);
            float scaleY = ((float)newWidth) / ((float)originalWidth);

            float[,] array_Out = new float[newHeight, newWidth];
            float u = 0, v = 0, x = 0, y = 0;
            int m = 0, n = 0;
            int i, j;
            await Task.Run(async () =>
            {
                for (i = 0; i < newHeight; ++i)
                {
                    await Task.Run(() =>
                    {
                        for (j = 0; j < newWidth; ++j)
                        {
                            x = i / scaleX;
                            y = j / scaleY;

                            m = Mathf.FloorToInt(x);
                            n = Mathf.FloorToInt(y);

                            u = x - m;
                            v = y - n;

                            if (m < originalHeight - 1 && n < originalWidth - 1)
                            {

                                array_Out[i, j] = (1.0f - v) * (1.0f - u) * array_In[m, n] + (1 - v) * u * array_In[m, n + 1]
                                                       + v * (1.0f - u) * array_In[m + 1, n] + v * u * array_In[m + 1, n + 1];
                            }
                            else
                            {
                                array_Out[i, j] = array_In[m, n];
                            }
                        }

                    });
                    //Debug.Log(string.Format("i:{0}", i));
                }
            }
            );

            return array_Out;
        }

        /// <summary>
        /// 双线性差值
        /// </summary>
        /// <param name="array"></param>
        /// <param name="length_0"></param>
        /// <param name="length_1"></param>
        /// <returns></returns>
        public static async Task<float[,]> BilinearInterp(float[,] array, int length_0, int length_1)
        {
            float[,] _out = new float[length_0, length_1];
            int original_0 = array.GetLength(0);
            int original_1 = array.GetLength(1);

            float ReScale_0 = original_0 / ((float)length_0);  // 倍数的倒数
            float ReScale_1 = original_1 / ((float)length_1);

            float index_0;
            float index_1;
            int inde_0;
            int inde_1;
            float s_leftUp;
            float s_rightUp;
            float s_rightDown;
            float s_leftDown;

            await Task.Run(async () =>
            {
                for (int i = 0; i < length_0; i++)
                {
                    await Task.Run(() =>
                    {
                        for (int j = 0; j < length_1; j++)
                        {
                            index_0 = i * ReScale_0;
                            index_1 = j * ReScale_1;
                            inde_0 = Mathf.FloorToInt(index_0);
                            inde_1 = Mathf.FloorToInt(index_1);
                            s_leftUp = (index_0 - inde_0) * (index_1 - inde_1);
                            s_rightUp = (inde_0 + 1 - index_0) * (index_1 - inde_1);
                            s_rightDown = (inde_0 + 1 - index_0) * (inde_1 + 1 - index_1);
                            s_leftDown = (index_0 - inde_0) * (inde_1 + 1 - index_1);
                            _out[i, j] = array[inde_0, inde_1] * s_rightDown + array[inde_0 + 1, inde_1] * s_leftDown + array[inde_0 + 1, inde_1 + 1] * s_leftUp + array[inde_0, inde_1 + 1] * s_rightUp;
                        }
                    });
                }
            });

            return _out;
        }

        /// <summary>
        /// 对二维数组做高斯模糊
        /// </summary>
        /// <param name="array">要处理的数组</param>
        /// <param name="dev"></param>
        /// <param name="r">高斯核扩展半径</param>
        /// <param name="isCircle">改变形状是否是圆</param>
        public async static Task GaussianBlurAsync(float[,] array, float dev, int r = 1, bool isCircle = true)
        {
            // 构造或者从字典中拿取半径为r的高斯核
            int length = r * 2 + 1;
            float[,] gaussianCore;
            if (gaussianCoreDic.ContainsKey(r) && dev == 1.5)
            {
                gaussianCore = gaussianCoreDic[r];
            }
            else
            {
                gaussianCore = new float[length, length];
                float k = 1 / (2 * Mathf.PI * dev * dev);
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        float pow = -((j - r) * (j - r) + (i - r) * (i - r)) / (2 * dev * dev);
                        gaussianCore[i, j] = k * Mathf.Pow(2.71828f, pow);
                    }
                }

                // 使权值和为1
                float sum = 0;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        sum += gaussianCore[i, j];
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        gaussianCore[i, j] /= sum;
                    }
                }

                if (dev == 1.5 && !gaussianCoreDic.ContainsKey(r))
                    gaussianCoreDic.Add(r, gaussianCore);
            }

            // 对二维数组进行高斯模糊处理
            int circleR = array.GetLength(0) / 2;

            for (int i = r, length_0 = array.GetLength(0) - r; i < length_0; i++)
            {
                await Task.Run(() =>
                {
                    for (int j = r, length_1 = array.GetLength(1) - r; j < length_1; j++)
                    {
                        if (isCircle && (i - circleR) * (i - circleR) + (j - circleR) * (j - circleR) > (circleR - r) * (circleR - r))
                            continue;

                        // 用高斯核处理一个值
                        float value = 0;
                        for (int u = 0; u < length; u++)
                        {
                            for (int v = 0; v < length; v++)
                            {
                                if ((i + u - r) >= array.GetLength(0) || (i + u - r) < 0 || (j + v - r) >= array.GetLength(1) || (j + v - r) < 0)
                                    Debug.LogError("滴嘟滴嘟的报错");
                                else
                                    value += gaussianCore[u, v] * array[i + u - r, j + v - r];
                            }
                        }
                        array[i, j] = value;
                    }
                });
            }
        }

        /// <summary>
        /// 对二维数组做高斯模糊
        /// </summary>
        /// <param name="array">要处理的数组</param>
        /// <param name="dev"></param>
        /// <param name="r">高斯核扩展半径</param>
        /// <param name="isCircle">改变形状是否是圆</param>
        public static void GaussianBlur(float[,] array, float dev, int r = 1, bool isCircle = true)
        {
            // 构造或者从字典中拿取半径为r的高斯核
            int length = r * 2 + 1;
            float[,] gaussianCore;
            if (gaussianCoreDic.ContainsKey(r) && dev == 1.5)
            {
                gaussianCore = gaussianCoreDic[r];
            }
            else
            {
                gaussianCore = new float[length, length];
                float k = 1 / (2 * Mathf.PI * dev * dev);
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        float pow = -((j - r) * (j - r) + (i - r) * (i - r)) / (2 * dev * dev);
                        gaussianCore[i, j] = k * Mathf.Pow(2.71828f, pow);
                    }
                }

                // 使权值和为1
                float sum = 0;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        sum += gaussianCore[i, j];
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        gaussianCore[i, j] /= sum;
                    }
                }

                if (dev == 1.5 && !gaussianCoreDic.ContainsKey(r))
                    gaussianCoreDic.Add(r, gaussianCore);
            }

            // 对二维数组进行高斯模糊处理
            int circleR = array.GetLength(0) / 2;

            for (int i = r, length_0 = array.GetLength(0) - r; i < length_0; i++)
            {
                for (int j = r, length_1 = array.GetLength(1) - r; j < length_1; j++)
                {
                    // 限制圆
                    if (isCircle && (i - circleR) * (i - circleR) + (j - circleR) * (j - circleR) > (circleR - r) * (circleR - r))
                        continue;

                    // 对边缘的处理
                    int u = 0, v = 0, tempLength0 = length, tempLength1 = length;
                    //if (i < r)
                    //{
                    //    u += r - i;
                    //}
                    //else if (i >= array.GetLength(0) - r)
                    //{
                    //    tempLength0 -= i + r + 1 - array.GetLength(0) + 1;
                    //}

                    //if (j < r)
                    //{
                    //    v += r - j;
                    //}
                    //else if (j >= array.GetLength(1) - r)
                    //{
                    //    tempLength1 -= j + r + 1 - array.GetLength(1);
                    //}

                    // 用高斯核处理一个值
                    float value = 0;
                    for (u = 0; u < tempLength0; u++)
                    {
                        for (v = 0; v < tempLength1; v++)
                        {
                            if ((i + u - r) >= array.GetLength(0) || (i + u - r) < 0 || (j + v - r) >= array.GetLength(1) || (j + v - r) < 0)
                                Debug.LogError("滴嘟滴嘟的报错" + i + " as" + j);
                            else
                                value += gaussianCore[u, v] * array[i + u - r, j + v - r];
                        }
                    }
                    array[i, j] = value;
                }
            }
        }

        #endregion

        #region 抛物线相关公式

        /// <summary>
        /// 求抛物线上某点的高度
        /// </summary>
        /// <param name="angle">角度</param>
        /// <param name="v">速度</param>
        /// <param name="x">x轴的量</param>
        /// <returns></returns>
        public static float ParabolaGetHeight(float angle, float v, float x)
        {
            float vy = v * Mathf.Sin(Mathf.Deg2Rad * angle);
            float vxz = v * Mathf.Cos(Mathf.Deg2Rad * angle);

            float y = -0.5f * 9.8f * (x * x / (vxz * vxz)) + vy * x / vxz;
            return y;
        }

        /// <summary>
        /// 根据俯仰角和速度求最远距离
        /// </summary>
        public static float ParabolaGetMaxDis(float v, float angle)
        {
            return 2 * v * v * Mathf.Sin(angle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad) / (9.8f);
        }

        /// <summary>
        /// 通过起始点和速度反推俯仰角
        /// </summary>
        public static float ParabolaGetAngle(Vector3 start, Vector3 end, float v)
        {
            float tempRad = Mathf.Asin(9.8f * (start - end).magnitude / (v * v));
            return tempRad * Mathf.Rad2Deg / 2;
        }

        /// <summary>
        /// 求给定高度的距离的较大值
        /// </summary>
        /// <param name="v">速度</param>
        /// <param name="angle">角度</param>
        /// <param name="height">竖直方向上的高度</param>
        /// <returns></returns>
        public static float ParabolaGetDis(float v, float angle, float height)
        {
            float vy = v * Mathf.Sin(Mathf.Deg2Rad * angle);

            if (vy * vy + 2 * -9.8f * height < 0) return 0;
            float res1, res2;
            res1 = (-vy + Mathf.Sqrt(vy * vy + 2 * -9.8f * height)) / -9.8f;
            res2 = (-vy - Mathf.Sqrt(vy * vy + 2 * -9.8f * height)) / -9.8f;

            res1 = res1 > res2 ? res1 : res2;       // t 取较大值
            res1 = res1 * v * Mathf.Cos(Mathf.Deg2Rad * angle);     // x = vxz * t

            return res1;
        }

        #endregion

        #region 多边形相关

        /// <summary>
        /// 判断点是否在多边形区域内
        /// </summary>
        /// <param name="p">待判断的点，格式：{ x: X坐标, y: Y坐标 }</param>
        /// <param name="poly">多边形顶点，数组成员的格式同</param>
        /// <returns>true:在多边形内，凹点   false：在多边形外，凸点</returns>
        public static bool IsPointInsidePolygon(Vector2 p, Vector2[] poly, bool containEdge = true)
        {
            float px = p.x;
            float py = p.y;
            double sum = 0;

            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i, i++)
            {
                float sx = poly[i].x;
                float sy = poly[i].y;
                float tx = poly[j].x;
                float ty = poly[j].y;

                // 点与多边形顶点重合或在多边形的边上(这个判断有些情况不需要)
                if ((sx - px) * (px - tx) >= 0 && (sy - py) * (py - ty) >= 0 && (px - sx) * (ty - sy) == (py - sy) * (tx - sx))
                {
                    return containEdge;
                }

                // 点与相邻顶点连线的夹角
                var angle = Mathf.Atan2(sy - py, sx - px) - Math.Atan2(ty - py, tx - px);

                // 确保夹角不超出取值范围（-π 到 π）
                if (angle >= Mathf.PI)
                {
                    angle = angle - Mathf.PI * 2;
                }
                else if (angle <= -Mathf.PI)
                {
                    angle = angle + Mathf.PI * 2;
                }

                sum += angle;
            }

            // 计算回转数并判断点和多边形的几何关系
            return Mathf.RoundToInt((float)(sum / Math.PI)) == 0 ? false : true;
        }

        #endregion
    }
}