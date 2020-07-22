using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class SaveGameManager : MonoBehaviour
{
    public GameData profileData;
    public GameData testData;

    public string saveGameFolder = "/SaveGameData/";

    public string saveFileName = "GameData.sav";
    // Start is called before the first frame update
    void Start()
    {
        //LoadGameDataWithJSON();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGameDataWithJSON();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGameDataWithJSON();
        }
    }

    public void SaveGameDataWithJSON()
    {
        // Convert our Scriptable Object Game Data - Profile Data - to a JSON string 
        string jsonString = JsonUtility.ToJson(profileData);
        // Find our save destination
        string dataPath = Application.persistentDataPath + saveGameFolder;
        
        // Check if there is a directory before saving
        if (!Directory.Exists(dataPath))
        {
            Debug.Log("Directory doesn't exist...creating now..");
            Directory.CreateDirectory(dataPath);
        }
        
        // Set our save data to this folder
        StreamWriter streamWriter = new StreamWriter(dataPath + saveFileName);
        // Write our data to that path
        streamWriter.Write(jsonString);
        // CLOSE THE STREAM WRITER
        streamWriter.Close();
        
        Debug.Log("Save successful");
    }

    public void LoadGameDataWithJSON()
    {
        // Get our full data path, which includes the folder the save is in, and the
        // save file type suffix
        string fullDataPath = Application.persistentDataPath + saveGameFolder + saveFileName;

        if (File.Exists(fullDataPath))
        {
            // Get all of the text in the save file
            string jsonString = File.ReadAllText(fullDataPath);
            
            Debug.Log("Found file directory: " + fullDataPath);
            // Set our type, (In this case, a Scriptable Object)[Which is of type "GameData"]) to the Scriptable Object in the jsonString
            //profileData = JsonUtility.FromJson<GameData>(jsonString);   // The TYPE is GAMEDATA because we created a Scriptable Object of type GameData
            // ^ This is the usual way of doing things, but we have to use FromJsonOverwrite for Scriptable Objects and MonoBehaviours
            JsonUtility.FromJsonOverwrite(jsonString, profileData);
        }
        else
        {
            Debug.Log("ERROR! Could not load save game data! Save file does not exist!");
        }
    }
}

// This is a helper class that will store all of the data the user can have including Buttz Collected,
// Backgrounds Collected, Achievements, etc.
// This approach will work but let's try using a SO instead..
/*
public class SaveData
{
    public string playerName;
}
*/