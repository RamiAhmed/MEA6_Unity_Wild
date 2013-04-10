using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {
	
	GoogleManager googleManager = null;
	
	List<string> questionsList = new List<string>();
	List<string> answersList = new List<string>();

	void Start() 
	{
		googleManager = new GoogleManager();
		
		//Debug.Log(googleManager.GetCellValue(2, 1));
		ListQuestions();
	}
	
	void OnGUI() 
	{
		if (GameStateHandler.GetCurrentGameState() == GameStateHandler.GameState.QUESTIONNAIRE)
		{
			float width = Screen.width * 0.75f,
				height = Screen.height * 0.75f;
			float x = 5f,
				y = 5f;
			
			float questionHeight = 50f, yPos = 0f;
			
			GUI.BeginGroup(new Rect(x, y, width, height));
			GUI.Box(new Rect(0f, 0f, width, height), "");
			
			for (int i = 0; i < questionsList.Count; i++)
			{
				string question = questionsList[i];
				answersList.Add("");
				
				GUI.Label(new Rect(0f, yPos, width, questionHeight), question);
				yPos += questionHeight;
				
				if (i == 1)
				{
					if (GUI.Button(new Rect(width * 0.33f, yPos, width * 0.25f, questionHeight/2f), "Male"))
					{
						answersList[i] = "Male";
					}
					
					if (GUI.Button(new Rect(width * 0.66f, yPos, width * 0.25f, questionHeight/2f), "Female"))
					{
						answersList[i] = "Female";
					}					
				}
				else if (i == 2)
				{
					if (GUI.Button(new Rect(5f, yPos, (width/2f), 20f), "More than once per day"))
					{
						answersList[i] = "More than once per day";
					}
					
					yPos += 20f;
					if (GUI.Button(new Rect(5f, yPos, (width/2f), 20f), "More than once per week"))
					{
						answersList[i] = "More than once per week";
					}
					
					yPos += 20f;
					if (GUI.Button(new Rect(5f, yPos, (width/2f), 20f), "More than once per month"))
					{
						answersList[i] = "More than once per month";
					}
					
					yPos += 20f;
					if (GUI.Button(new Rect(5f, yPos, (width/2f), 20f), "More than once per year"))
					{
						answersList[i] = "More than once per year";
					}
					
					yPos += 20f;
					if (GUI.Button(new Rect(5f, yPos, (width/2f), 20f), "Once per year or less"))
					{
						answersList[i] = "Once per year or less";	
					}
					
					yPos += 20f;
				}
				else
				{
					answersList[i] = GUI.TextArea(new Rect(0f, yPos, width, questionHeight), answersList[i]);
					yPos += questionHeight;
				}
			}
			
			if (GUI.Button(new Rect((width/2f), height-questionHeight, (width/2f), questionHeight), "Submit"))
			{
				//foreach (string question in questionsList)
				for (int j = 0; j < questionsList.Count; j++)
				{
					googleManager.WriteListToRow(questionsList[j], answersList[j]);
				}
			}			
			
			GUI.EndGroup();
		}
	}
	
	private void ListQuestions()
	{
		int numOfColumns = 7;
		for (int i = 2; i < numOfColumns+1; i++)
		{
			string question = googleManager.GetCellValue(1, i);
			questionsList.Add(question);
		}
	}
}
