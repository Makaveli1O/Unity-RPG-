using System.IO;
using UnityEngine;

public class LoadMenu : MonoBehaviour
{
    public GameObject loadButtonPrefab;
    public GameObject scrollViewContent;
    // Start is called before the first frame update
    void Start()
    {
        // Make a reference to a directory.
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "\\Saves");

        // Get a reference to each directory in that directory.
        DirectoryInfo[] diArr = di.GetDirectories();

        float offset = 46f;
        int i = 1;
        // Display the names of the directories.
        foreach (DirectoryInfo dri in diArr){
            Vector3 newPos = new Vector3(0f, 180f - offset * i);
            GameObject btn = Instantiate(loadButtonPrefab, newPos, Quaternion.identity, scrollViewContent.transform);
            RectTransform rectTransform = btn.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = newPos;
            LoadButton btnScript = btn.GetComponent<LoadButton>();
            btnScript.text.SetText(dri.Name);
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
