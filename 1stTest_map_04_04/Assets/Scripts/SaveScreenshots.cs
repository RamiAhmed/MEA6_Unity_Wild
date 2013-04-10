using UnityEngine;
using System.IO;
using System.Collections;
 
public class SaveScreenshots : MonoBehaviour
{    
    private int screenshotCount = -1;
 
    // Check for screenshot key each frame
    void LateUpdate()
    {
		if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.Print))
		{
			TakeScreenshot(true);
		}
    }
	
	public void TakeScreenshot(bool bEnabledInEditor = false)
	{
		if (!Application.isEditor || bEnabledInEditor)
		{
			string path = @Application.dataPath + @"/Data/Shots/"; 
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);	
			}
			
	        // take screenshot 
	        string screenshotFilename;
	        do
	        {
	            screenshotCount++;
	            screenshotFilename = path + "screenshot" + screenshotCount + ".png";
	
	        } while (File.Exists(screenshotFilename));
	
	        Application.CaptureScreenshot(screenshotFilename);
			Debug.Log("Screenshot saved in " + screenshotFilename);		
		}
	}
}