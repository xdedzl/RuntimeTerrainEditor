using UnityEditor;
using UnityEngine;
using XFramework;
using System;
using System.IO;
using System.Reflection;
using XFramework.Editor;

[CustomEditor(typeof(Game))]
public class GameInspector : Editor
{
    private string[] typeNames = null;
    private int entranceProcedureIndex = 0;

    private Game game;
    private Type currentType;
    private ProcedureBase startPrcedureTemplate;
    private ProtocolBytes p = new ProtocolBytes();
    private string savePath;

    /// <summary>
    /// 根据上一次操作初始化流程参数
    /// </summary>
    private void Awake()
    {
        entranceProcedureIndex = EditorPrefs.GetInt("index", 0);

        typeNames = typeof(ProcedureBase).GetSonNames();
        if (typeNames.Length == 0)
            return;

        if (entranceProcedureIndex > typeNames.Length - 1)
            entranceProcedureIndex = 0;

        game = target as Game;
        game.TypeName = typeNames[entranceProcedureIndex];

        startPrcedureTemplate = Utility.Reflection.CreateInstance<ProcedureBase>(GetType(typeNames[entranceProcedureIndex]));
        game.DeSerialize(startPrcedureTemplate);

        savePath = Application.persistentDataPath + "/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "Procedure";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        savePath += "/";
    }

    public override void OnInspectorGUI()
    {
        typeNames = typeof(ProcedureBase).GetSonNames();
        if (typeNames.Length == 0)
            return;

        if (entranceProcedureIndex > typeNames.Length - 1)
            entranceProcedureIndex = 0;

        GUI.backgroundColor = new Color32(0, 170, 255, 30);
        GUILayout.BeginVertical("Box");
        GUI.backgroundColor = Color.white;

        int lastIndex = entranceProcedureIndex;
        entranceProcedureIndex = EditorGUILayout.Popup("Entrance Procedure", entranceProcedureIndex, typeNames);

        if (lastIndex != entranceProcedureIndex)
        {
            game.TypeName = typeNames[entranceProcedureIndex];
            currentType = GetType(typeNames[entranceProcedureIndex]);

            startPrcedureTemplate = Utility.Reflection.CreateInstance<ProcedureBase>(GetType(typeNames[entranceProcedureIndex]));
            if (File.Exists(savePath + currentType.Name))
            {
                ProtocolBytes p = new ProtocolBytes(File.ReadAllBytes(savePath + currentType.Name));
                p.GetString();
                p.DeSerialize(startPrcedureTemplate);
            }
        }

        currentType = currentType ?? GetType(typeNames[entranceProcedureIndex]);
        startPrcedureTemplate = startPrcedureTemplate ?? Utility.Reflection.CreateInstance<ProcedureBase>(GetType(typeNames[entranceProcedureIndex]));

        // 可视化当前流程的变量
        if (!Application.isPlaying)
        {
            XEditorUtility.SerializableObj(startPrcedureTemplate);
            Serialize();
        }
        else
        {
            XEditorUtility.SerializableObj(Game.ProcedureModule.GetCurrentProcedure());
        }

        GUILayout.EndVertical();
    }

    private void OnDestroy()
    {
        EditorPrefs.SetInt("index", entranceProcedureIndex);
    }

    // Type.GetType(string)在编辑器下貌似有些问题
    private Type GetType(string name)
    {
        Assembly assembly = Assembly.Load("Assembly-CSharp");
        Type[] allType = assembly.GetTypes();
        foreach (Type type in allType)
        {
            if (type.Name == name)
                return type;
        }
        return null;
    }

    private void Serialize()
    {
        p.Clear();
        p.AddString(currentType.Name);

        p.Serialize(startPrcedureTemplate);

        File.WriteAllBytes(savePath + currentType.Name, p.Encode());
    }
}