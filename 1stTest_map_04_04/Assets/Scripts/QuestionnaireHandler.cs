using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {
	
	private GoogleManager googleManager = null;
	private KeyValueList questionsList;
	private Dictionary<string,string> answersList;
	
	private Vector3 scrollViewVector = Vector2.zero;
	
	private float width = Screen.width * 0.9f,
				  height = Screen.height-10f;
	
	private float qHeight = 40f,
		 		  qWidth = Screen.width * 0.8f;
	
	private int currentPage = 0,
				lastPage = 1; 
	
	private ScenarioHandler scenarioHandlerRef;
	
	private int age = 0, gender = 0, frequency = 0, amount = 0, experience = 0, preferrence = 0;
	private string favouriteGame = "";	
	
	private int[] likertAnswers;
	private string[] wantBuildAnswers;
	private string[] commentAnswers;
	
	private int questionsCount = 0;
	
	void Start() 
	{
		scenarioHandlerRef = GameObject.Find("ScenarioHandlerBox").GetComponent<ScenarioHandler>();
		lastPage = scenarioHandlerRef.GetScenarioCount()+2;
		
		answersList = new Dictionary<string,string>();
		questionsList = new KeyValueList();
		googleManager = new GoogleManager();
		
		FindAllQuestions();
		
		likertAnswers = new int[lastPage];
		wantBuildAnswers = new string[lastPage];
		commentAnswers = new string[lastPage];
		
		for (int i = 0; i < lastPage; i++)
		{
			likertAnswers[i] = 0;
			wantBuildAnswers[i] = "";
			commentAnswers[i] = "";
		}
	}
	
	void OnGUI() 
	{
		if (questionsList.Count == 0)
			return;
		
		if (GameStateHandler.GetCurrentGameState() == GameStateHandler.GameState.QUESTIONNAIRE)
		{			
			float yPos = 10f;
				
			Rect positionRect = new Rect(5f, 5f, width, height),
				sizeRect = new Rect(0f, 0f, width, (qHeight * questionsList.Count)+10f);
			
			if (!GUI.skin.box.wordWrap)
				GUI.skin.box.wordWrap = true;
			
			GUI.BeginGroup(positionRect);
			scrollViewVector = GUI.BeginScrollView(positionRect, scrollViewVector, sizeRect); 
			
			foreach (KeyValuePair<string,string> pair in questionsList)
			{
				if (currentPage == 0)
					yPos = BuildDemographics(pair, yPos);
				else if (currentPage < lastPage-1)
					yPos = BuildDuring(pair, yPos);
				else
					yPos = BuildAfter(pair, yPos);
			}
			
			if (GetQuestionsAnswered())
			{
				if (currentPage < lastPage-1)
				{
					if (GUI.Button(new Rect(qWidth-200f, height-75f, 150f, 50f), "Continue"))
					{
						if (currentPage == 0)
						{
							SubmitAllAnswers(false);
						}
						else
						{
							SubmitAllAnswers(true);
							scenarioHandlerRef.GetNewRandomScenario();
							GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);
						}
						
						currentPage++;
					}
				}
				else
				{
					float exitBtnY = height-75f;
					if (exitBtnY > yPos)
						exitBtnY = yPos;
					if (GUI.Button(new Rect(qWidth-200f, exitBtnY, 150f, 50f), "Exit Game"))
					{
						SubmitAllAnswers(true);
						GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.END);
					}
				}
			}
			GUI.EndScrollView();
			GUI.EndGroup();
			
			questionsCount = 0;
		}
	}
	
	private bool GetQuestionsAnswered()
	{
		bool bAnswered = true;
		foreach (KeyValuePair<string,string> answer in answersList)
		{
			Debug.Log ("Answer: " + answer.Key + " - " + answer.Value);
			if (answer.Value == "" || answer.Key == "")
			{
				bAnswered = false;
				break;
			}
		}
		
		return bAnswered;
	}
	
	private float BuildAfter(KeyValuePair<string,string> pair, float yPos)
	{
		yPos = BuildDuring(pair, yPos);
		
		if (pair.Key == "preferrence")
		{
			string[] preferrenceOptions = {"Mouse", "Keyboard"};
			
			preferrence = AddMultipleChoice(preferrenceOptions, pair.Value, pair.Key, preferrence, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		return yPos;
	}
	
	private float BuildDuring(KeyValuePair<string,string> pair, float yPos)
	{
		if (pair.Key == "likert" + currentPage.ToString())
		{
			likertAnswers[currentPage] = AddLikertScale(pair.Value, pair.Key, likertAnswers[currentPage], yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "wantbuild" + currentPage.ToString())
		{
			wantBuildAnswers[currentPage] = AddEssay(wantBuildAnswers[currentPage], pair.Value, pair.Key, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "comments" + currentPage.ToString())
		{
			commentAnswers[currentPage] = AddEssay(commentAnswers[currentPage], pair.Value, pair.Key, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		return yPos;
	}
	
	private float BuildDemographics(KeyValuePair<string,string> pair, float yPos)
	{
		if (pair.Key == "age")
		{
			string[] ageIntervals = {"14 or less", "15-19", "20-24", "25-29", "30 or more"};
			
			age = AddMultipleChoice(ageIntervals, pair.Value, pair.Key, age, yPos);

			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "gender")
		{
			string[] genders = {"Male", "Female"};
			
			gender = AddMultipleChoice(genders, pair.Value, pair.Key, gender, yPos);

			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "freqplaying")
		{
			string[] freqIntervals = {"Once per year or less", "More than once per month", "More than once per week", "More than once per day"};
		
			frequency = AddMultipleChoice(freqIntervals, pair.Value, pair.Key, frequency, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "amountplaying")
		{
			string[] amountIntervals = {"Less than 1 hour", "More than 1 hour", "More than 2 hours", "More than 4 hours", "More than 6 hours", "More than 8 hours"};
			
			amount = AddMultipleChoice(amountIntervals, pair.Value, pair.Key, amount, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "favourite")
		{	
			favouriteGame = AddEssay(favouriteGame, pair.Value, pair.Key, yPos);

			yPos += qHeight * 2.1f;
		}
		
		if (pair.Key == "likert0")
		{
			experience = AddLikertScale(pair.Value, pair.Key, experience, yPos);
			
			yPos += qHeight * 2.1f;
		}
		
		return yPos;
	}
	
	private int AddLikertScale(string question, string key, int toolbarSelection, float yPos)
	{
		string[] likertChoices = {"-3", "-2", "-1", "0", "+1", "+2", "+3"};
		
		toolbarSelection = AddMultipleChoice(likertChoices, question, key, toolbarSelection, yPos);
		
		return toolbarSelection;
	}
	
	private string AddEssay(string text, string question, string key, float yPos) 
	{
		GUI.BeginGroup(new Rect(0f, yPos, width, qHeight*2f));
		
		GUI.Box(new Rect(5f, 5f, qWidth, qHeight-10f), question);
		
		text = GUI.TextArea(new Rect(5, qHeight-5f, qWidth, qHeight), text);
			
		GUI.EndGroup();	
		
		AddOrReplaceToDict(answersList, key, text);
		questionsCount++;
		
		return text;
	}
	
	private int AddMultipleChoice(string[] toolbarOptions, string question, string key, int toolbarSelection, float yPos)
	{
		GUI.BeginGroup(new Rect(0f, yPos, width, qHeight*2f));
		
		GUI.Box(new Rect(5f, 5f, qWidth, qHeight-10f), question);
		
		toolbarSelection = GUI.Toolbar(new Rect(5f, qHeight-5f, qWidth, qHeight), toolbarSelection, toolbarOptions);
			
		GUI.EndGroup();
		
		if (GUI.changed)
			AddOrReplaceToDict(answersList, key, toolbarOptions[toolbarSelection]);		
		
		questionsCount++;
		return toolbarSelection;
	}
	
	private void AddOrReplaceToDict(Dictionary<string,string> dict, string key, string value)
	{
		if (dict.ContainsKey(key))
			dict[key] = value;
		else
			dict.Add(key, value);
	}
	
	private void SubmitAllAnswers(bool bUpdateRow)
	{
		string now = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToString("HH:mm:ss");
		AddOrReplaceToDict(answersList, "timestamp" + currentPage.ToString(), now);
		
		string scenario = scenarioHandlerRef.GetCurrentScenario().ToString();
		AddOrReplaceToDict(answersList, "scenario" + currentPage.ToString(), scenario);
		
		googleManager.WriteDictToRow(answersList, bUpdateRow);
		answersList.Clear();
	}
	
	private void FindAllQuestions()
	{
		int row = 2, column = 1;
		string cellValue = googleManager.GetCellValue(row, column);
		while (cellValue != "")
		{
			questionsList.Add(googleManager.GetCellValue(row-1, column), cellValue);
			
			column++;			
			cellValue = googleManager.GetCellValue(row, column);
		}
	}

}
