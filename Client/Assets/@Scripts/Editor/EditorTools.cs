using UnityEditor;
using UnityEngine;

public class EditorTools : EditorWindow
{
#if UNITY_EDITOR
    [MenuItem("Tools/Remove Save Data")]
    public static void RemoveSaveData()
    {
        // PlayerPrefs 기반의 세이브 시스템을 사용하므로 PlayerPrefs에서 키를 삭제합니다.
        if (PlayerPrefs.HasKey("SaveData"))
        {
            PlayerPrefs.DeleteKey("SaveData");
            PlayerPrefs.Save();
            LogMessage.Log("SaveData (PlayerPrefs) Deleted Successfully.");
        }
        else
        {
            LogMessage.Log("No SaveData found in PlayerPrefs.");
        }
    }
    
    [MenuItem("Tools/Open Save File Location")]
    public static void OpenSaveLocation()
    {
        // PlayerPrefs 위치 관련 정보 출력 (OS별로 상이함)
        LogMessage.Log($"Persistent Data Path: {Application.persistentDataPath}");
        // Mac에서는 ~/Library/Preferences/unity.CompanyName.ProductName.plist 에 저장됨
    }
#endif
}
