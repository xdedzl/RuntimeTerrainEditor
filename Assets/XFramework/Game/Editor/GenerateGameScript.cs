using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using XFramework;
using XFramework.UI;
using System.IO;
using System.Text.RegularExpressions;
using XFramework.Pool;
using XFramework.Event;
using System.Resources;

/// <summary>
/// Game脚本的刷新
/// </summary>
public class GenerateGame
{
    // 需要忽略的模块
    public static Type[] ingoreTypes = new Type[] {
        typeof(UIMgrDicType),
        typeof(UIMgrStackType),
    };

    // 自定义模块属性名
    public static Dictionary<Type, string> filedNames = new Dictionary<Type, string>()
    {
        { typeof(DataSubjectManager), "ObserverModule" },
        { typeof(ResourceManager), "ResModule" },
        { typeof(ObjectPoolManager), "ObjectPool" },
    };

    // 被添加的模块类型
    public static List<Type> modules = new List<Type>();

    [MenuItem("XFramework/RefreshGameScripts")]
    public static void GenerateGameScript()
    {
        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/XFramework/Game");
        info = info.Exists ? info : new DirectoryInfo(Application.dataPath + "/Game");
        if (!info.Exists)
        {
            Debug.LogWarning("文件夹名被修改");
            return;
        }

        string scriptPath = null;
        foreach (var item in info.GetFiles("*.cs", SearchOption.AllDirectories))
        {
            if (item.Name == "Game.cs")
            {
                scriptPath = item.FullName;
            }
        }

        if (string.IsNullOrEmpty(scriptPath))
            Debug.LogWarning("没有Game脚本或不在指定位置");
        string script = File.ReadAllText(scriptPath);

        // 替换框架模块的属性定义
        script = Replace(script, "// Start0", "// End0", "\n" + GenerateAttribute(Assembly.Load("XFrameworkRuntime")) + "\t");

        // 替换非框架模块的属性定义
        script = Replace(script, "// Start1", "// End1", "\n" + GenerateAttribute(Assembly.Load("Assembly-CSharp")) + "\t");

        // 替换模块的初始化
        string addModules = "\n";
        foreach (var item in modules)
        {
            addModules += AddModule(item) + "\n";
        }
        script = Replace(script, "// Start2", "// End2", addModules + "\t\t");

        // 替换模块引用的刷新
        string getModules = "\n";
        foreach (var item in modules)
        {
            getModules += GetModule(item) + "\n";
        }
        script = Replace(script, "// Start3", "// End3", getModules + "\t\t");


        // 替换8空格\t为四空格
        script = script.Replace("\t", "    ");

        File.WriteAllText(scriptPath, script);

        Debug.Log("更新成功");
    }

    // 替换字符串
    private static string Replace(string target, string startFlag, string endFlag, string newValue)
    {
        string searchPattern = startFlag;
        Regex regex = new Regex($@"[\s\S]*{searchPattern}");
        int startIndex = regex.Match(target).Value.Length;

        searchPattern = endFlag;
        regex = new Regex($@"[\s\S]*{searchPattern}");
        int endIndex = regex.Match(target).Value.Length - searchPattern.Length;

        target = target.Remove(startIndex, endIndex - startIndex);

        return target.Insert(startIndex, newValue);
    }

    // 生成属性
    private static string GenerateAttribute(Assembly assm)
    {
        string attributes = "";

        foreach (var type in assm.GetTypes())
        {
            if (type.IsClass)
            {
                foreach (var item in type.GetInterfaces())
                {
                    if (item == typeof(IGameModule) && !IsIngoreType(type))
                    {
                        attributes += Attribute(type) + "\n";
                        modules.Add(type);
                    }
                }
            }
        }

        return attributes;
    }

    // 返回一个静态public的属性
    private static string Attribute(Type type)
    {
        return $"\tpublic static {type.Name} {GetFieldName(type)} {{ get; private set; }}";
    }

    // 添加模块的字符串
    private static string AddModule(Type type)
    {
        return $"\t\t{GetFieldName(type)} = GameEntry.AddModule<{type.Name}>();";
    }

    // 获取模块的字符串
    private static string GetModule(Type type)
    {
        return $"\t\t{GetFieldName(type)} = GameEntry.GetModule<{type.Name}>();";
    }

    // 是否为忽略模块
    private static bool IsIngoreType(Type type)
    {
        foreach (var item in ingoreTypes)
        {
            if(type == item)
            {
                return true;
            }
        }
        return false;
    }

    // 给模块取名
    private static string GetFieldName(Type type)
    {
        if (filedNames.ContainsKey(type))
        {
            return filedNames[type];
        }

        string typeName = type.Name;
        string name = "";
        for (int i = typeName.Length - 1; i >= 0; i--)
        {
            if (typeName[i] < 90 && typeName[i] > 65)
            {
                name = typeName.Remove(i);
                break;
            }
        }
        return name += "Module";
    }
}