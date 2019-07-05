using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExcelDataReader;
using System.IO;
using System.Data;
using System;
using System.Reflection;
using System.Text;

/*
 * 转换后json的文件名和xlsx的文件名相同
 * 表格名要和类名对应
 * 表格第一行为策划命名
 * 表格第二行为类成员变量名
 * 表格第三行为类成员变量类型
 * 成功后会删除xlsx
 */


/// <summary>
/// Excel转Json工具
/// </summary>
public class Ecel2Json
{
    /// <summary>
    /// 最后生成的Json文件是一行字符串，可以在VsCode中安装Json Tool后按Ctrl + Alt + M进行格式化
    /// </summary>
    [MenuItem("Assets/Excel2Json")]
    public static void Excel2Json()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        foreach (var item in objs)
        {
            string path = AssetDatabase.GetAssetPath(item);

            ReadExcel(path);
        }

        Debug.Log("Excel2Json结束");
    }

    /// <summary>
    /// 读取一个Excel文件
    /// </summary>
    /// <param name="xlsxPath"></param>
    private static void ReadExcel(string xlsxPath)
    {
        if (!xlsxPath.EndsWith(".xlsx"))
        {
            Debug.LogError(xlsxPath + "不是Excel文件");
            return;
        }
        string dataPath = Application.dataPath.Replace("Assets", "");

        xlsxPath = dataPath + xlsxPath;
        string jsonPath = xlsxPath.Replace(".xlsx", ".json");

        using (var stream = File.Open(xlsxPath, FileMode.Open, FileAccess.Read))
        {
            using(IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet();

                DataTableCollection tc = result.Tables;
                for (int i = 0; i < tc.Count; i++)
                {
                    ReadSheet(result.Tables[i], jsonPath);
                }
            }
        }
        File.Delete(xlsxPath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 读取Excel中的一张表
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="jsonPath"></param>
    private static void ReadSheet(DataTable dataTable,string jsonPath)
    {
        int row = dataTable.Rows.Count;
        int column = dataTable.Columns.Count;

        DataRowCollection collect = dataTable.Rows;
        string[] jsonFileds = new string[column];

        for (int i = 0; i < column; i++)
        {
            jsonFileds[i] = collect[1][i].ToString();
        }

        List<object> objs = new List<object>();

        Type type = Type.GetType(dataTable.TableName);
        for (int i = 3; i < row; i++)
        {
            object obj = Activator.CreateInstance(type);

            for (int j = 0; j < column; j++)
            {
                FieldInfo field = type.GetField(jsonFileds[j]);
                if (field != null)
                {
                    switch (field.FieldType.Name)
                    {
                        case "Int32":
                            field.SetValue(obj, int.Parse(collect[i][j].ToString()));
                            break;
                        case "Single":
                            field.SetValue(obj, float.Parse(collect[i][j].ToString()));
                            break;
                        case "Double":
                            field.SetValue(obj, double.Parse(collect[i][j].ToString()));
                            break;
                        case "Boolean":
                            field.SetValue(obj, bool.Parse(collect[i][j].ToString()));
                            break;
                        case "String":
                            field.SetValue(obj, collect[i][j].ToString());
                            break;
                        case "String[]":
                            field.SetValue(obj, collect[i][j].ToString().Split('\n'));
                            break;
                    }
                }
                else
                {
                    Debug.LogError("字符串和变量名不匹配");
                }
            }
            objs.Add(obj);
        }
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(objs);

        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }
        File.WriteAllBytes(jsonPath, Encoding.UTF8.GetBytes(json));
        AssetDatabase.Refresh();
    }
}