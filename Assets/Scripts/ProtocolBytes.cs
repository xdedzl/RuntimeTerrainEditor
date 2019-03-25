// ==========================================
// 描述： 
// 作者： LYG
// 时间： 2018-11-13 08:38:23
// 版本： V 1.0
// ==========================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 提供了一种基于字节流的协议
/// 字节流协议是一种最基本的协议。它把所有参数放入byte[]结构中，客户端和服务端按照约定的数据类型和顺序解析各个参数。本字节流协议支持int、float和string三种数据类型。
/// 不用Array.concat的原因是当数据量太大时，不断的执行concat会非常慢
public class ProtocolBytes
{
    private byte[] bytes;     //传输的字节流
    private List<byte> byteList;
    private int index;

    public ProtocolBytes()
    {
        index = 0;
        byteList = new List<byte>();
    }

    public ProtocolBytes(byte[] _bytes)
    {
        index = 0;
        bytes = _bytes;
        byteList = new List<byte>(_bytes);
    }

    /// <summary>
    /// 编码器
    /// </summary>
    /// <returns></returns>
    public byte[] Encode()
    {
        return byteList.ToArray();
    }

    /// <summary>
    /// 协议内容 提取每一个字节并组成字符串 用于查看消息
    /// </summary>
    /// <returns></returns>
    public string GetDesc()
    {
        string str = "";
        if (bytes == null) return str;
        for (int i = 0; i < bytes.Length; i++)
        {
            int b = (int)bytes[i];
            str += b.ToString() + " ";
        }
        return str;
    }

    #region 添加和获取字符串

    /// <summary>
    /// 将字符转转为字节数组加入字节流
    /// </summary>
    /// <param name="str">要添加的字符串</param>
    public void AddString(string str)
    {
        Int32 len = str.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        byteList.AddRange(lenBytes);
        byteList.AddRange(strBytes);
    }

    /// <summary>
    /// 将字节数组转化为字符串
    /// </summary>
    /// <param name="index">索引起点</param>
    /// <param name="end">为下一个转换提供索引起点</param>
    /// <returns></returns>
    public string GetString()
    {
        if (bytes == null)
            return "";
        if (bytes.Length < index + sizeof(int))
            return "";
        int strLen = BitConverter.ToInt32(bytes, index);
        if (bytes.Length < index + sizeof(int) + strLen)
            return "";
        string str = Encoding.UTF8.GetString(bytes, index + sizeof(int), strLen);
        index = index + sizeof(int) + strLen;
        return str;
    }

    #endregion

    #region 添加获取整数

    /// <summary>
    /// 将Int32转化成字节数组加入字节流
    /// </summary>
    /// <param name="num">要转化的Int32</param>
    public void AddInt32(int num)
    {
        byteList.Add((byte)num);
        byteList.Add((byte)(num >> 8));
        byteList.Add((byte)(num >> 16));
        byteList.Add((byte)(num >> 24));
    }

    /// <summary>
    /// 将字节数组转化成Int32
    /// </summary>
    public int GetInt32()
    {
        if (bytes == null)
            return 0;
        if (bytes.Length < index + 4)
            return 0;

        return (int)(bytes[index++] | bytes[index++] << 8 | bytes[index++] << 16 | bytes[index++] << 24);
    }

    #endregion

    #region 添加获取浮点数

    /// <summary>
    /// 将float转化成字节数组加入字节流
    /// </summary>
    /// <param name="num">要转化的float</param>
    public unsafe void AddFloat(float num)
    {
        uint temp = *(uint*)&num;
        byteList.Add((byte)temp);
        byteList.Add((byte)(temp >> 8));
        byteList.Add((byte)(temp >> 16));
        byteList.Add((byte)(temp >> 24));
    }

    /// <summary>
    /// 将字节数组转化成float
    /// </summary>
    public unsafe float GetFloat()
    {
        if (bytes == null)
            return -1;
        if (bytes.Length < index + sizeof(float))
            return -1;
        uint temp = (uint)(bytes[index++] | bytes[index++] << 8 | bytes[index++] << 16 | bytes[index++] << 24);
        return *((float*)&temp);
    }

    #endregion

    #region 添加获取布尔值

    public void AddBoolen(bool value)
    {
        byteList.Add((byte)(value ? 1 : 0));
    }

    public bool GetBoolen()
    {
        return (bytes[index++] == 1);
    }

    #endregion

    #region 添加获取Vector3

    public void AddVector3(Vector3 v)
    {
        AddFloat(v.x);
        AddFloat(v.y);
        AddFloat(v.z);
    }

    public Vector3 GetVector3()
    {
        float x = GetFloat();
        float y = GetFloat();
        float z = GetFloat();
        return new Vector3(x, y, z);
    }

    #endregion

    #region 添加获取数组

    public void AddFloatArray1(float[] array)
    {
        AddInt32(array.GetLength(0));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            AddFloat(array[i]);
        }
    }

    public void AddFloatArray2(float[,] array)
    {
        AddInt32(array.GetLength(0));
        AddInt32(array.GetLength(1));

        for (int i = 0,length_0 = array.GetLength(0); i < length_0; i++)
        {
            for (int j = 0,length_1 = array.GetLength(1); j < length_1; j++)
            {
                AddFloat(array[i, j]);
            }
        }
    }

    public void AddFloatArray3(float[,,] array)
    {
        AddInt32(array.GetLength(0));
        AddInt32(array.GetLength(1));
        AddInt32(array.GetLength(2));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            for (int j = 0, length_1 = array.GetLength(1); j < length_1; j++)
            {
                for (int k = 0, length_2 = array.GetLength(2); k < length_2; k++)
                {
                    AddFloat(array[i, j, k]);
                }
            }
        }
    }


    public float[] GetFloatArray1()
    {
        int length_0 = GetInt32();

        float[] array = new float[length_0];
        for (int i = 0; i < length_0; i++)
        {
            array[i] = GetFloat();
        }

        return array;
    }

    public float[,] GetFloatArray2()
    {
        int length_0 = GetInt32();
        int length_1 = GetInt32();

        float[,] array = new float[length_0, length_1];
        for (int i = 0; i < length_0; i++)
        {
            for (int j = 0; j < length_1; j++)
            {
                array[i,j] = GetFloat();
            }
        }

        return array;
    }

    public float[,,] GetFloatArray3()
    {
        int length_0 = GetInt32();
        int length_1 = GetInt32();
        int length_2 = GetInt32();

        float[,,] array = new float[length_0, length_1, length_2];
        for (int i = 0; i < length_0; i++)
        {
            for (int j = 0; j < length_1; j++)
            {
                for (int k = 0; k < length_2; k++)
                {
                    array[i, j, k] = GetFloat();
                }
            }
        }

        return array;
    }


    public void AddIntArray1(int[] array)
    {
        AddInt32(array.GetLength(0));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            AddInt32(array[i]);
        }
    }

    public void AddIntArray2(int[,] array)
    {
        AddInt32(array.GetLength(0));
        AddInt32(array.GetLength(1));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            for (int j = 0, length_1 = array.GetLength(1); j < length_1; j++)
            {
                AddInt32(array[i, j]);
            }
        }
    }

    public void AddIntArray3(int[,,] array)
    {
        AddInt32(array.GetLength(0));
        AddInt32(array.GetLength(1));
        AddInt32(array.GetLength(2));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            for (int j = 0, length_1 = array.GetLength(1); j < length_1; j++)
            {
                for (int k = 0, length_2 = array.GetLength(2); k < length_2; k++)
                {
                    AddInt32(array[i, j, k]);
                }
            }
        }
    }


    public int[] GetIntArray1()
    {
        int length_0 = GetInt32();

        int[] array = new int[length_0];
        for (int i = 0; i < length_0; i++)
        {
            array[i] = GetInt32();
        }

        return array;
    }

    public int[,] GetIntArray2()
    {
        int length_0 = GetInt32();
        int length_1 = GetInt32();

        int[,] array = new int[length_0, length_1];
        for (int i = 0; i < length_0; i++)
        {
            for (int j = 0; j < length_1; j++)
            {
                array[i, j] = GetInt32();
            }
        }

        return array;
    }

    public int[,,] GetIntArray3()
    {
        int length_0 = GetInt32();
        int length_1 = GetInt32();
        int length_2 = GetInt32();

        int[,,] array = new int[length_0, length_1, length_2];
        for (int i = 0; i < length_0; i++)
        {
            for (int j = 0; j < length_1; j++)
            {
                for (int k = 0; k < length_2; k++)
                {
                    array[i, j, k] = GetInt32();
                }
            }
        }

        return array;
    }

    public void AddVectorArray1(float[] array)
    {
        AddInt32(array.GetLength(0));

        for (int i = 0, length_0 = array.GetLength(0); i < length_0; i++)
        {
            AddFloat(array[i]);
        }
    }

    #endregion


    /// <summary>
    /// 添加帧同步
    /// </summary>
    public void AddFrameSynInfo(int id,Transform tran)
    {
        AddInt32(id);
        AddVector3(tran.position);
        AddVector3(tran.localEulerAngles);
    }
}