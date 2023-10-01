using UnityEngine;
using UnityEditor;
using System.IO;

public class FileSelector : MonoBehaviour
{
    public string filepathFace;
    public string filepathEyes;
    public string filepathBody;
    public string filepathWav;
}

[CustomEditor(typeof(FileSelector))]
public class FileSelectorExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FileSelector fileSelector = (FileSelector)target;

        //EditorGUILayout.Space();

        //// ��ʾ�����ļ�ѡ��ť��������Inspector�����ѡ������txt�ļ�
        //fileSelector.filepathFace = DrawFileField("Face File", fileSelector.filepathFace, "txt");
        fileSelector.filepathEyes = DrawFileField("Eyes File", fileSelector.filepathEyes, "txt");
        fileSelector.filepathBody = DrawFileField("Body File", fileSelector.filepathBody, "txt");

        EditorGUILayout.Space();

        
    }

    private string DrawFileField(string label, string filePath, string extension)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);

        if (GUILayout.Button("Select"))
        {
            string path = EditorUtility.OpenFilePanel(label, "", extension);
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
            }
        }

        EditorGUILayout.EndHorizontal();
        return filePath;
    }
}
