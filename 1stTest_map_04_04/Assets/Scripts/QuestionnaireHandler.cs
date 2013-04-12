using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {
	
	private GoogleManager googleManager = null;
	private KeyValueList questionsList;
	private Dictionary<string,string> answersList;
	
	private Vector3 scrollViewVector = Vector2.zero;
	
	private int currentPage = 0,
				lastPage = 1; 
	
	private ScenarioHandler scenarioHandlerRef;
	private PlayerController playerRef;
	
	private int age = 0, gender = 0, frequency = 0, amount = 0, experience = 0, preferrence = 0;
	private string favouriteGame = "";	
	
	private int[] likertAnswers;
	private string[] wantBuildAnswers;
	private string[] commentAnswers;
	
	void Start() 
	{
		scenarioHandlerRef = GameObject.Find("ScenarioHandlerBox").GetComponent<ScenarioHandler>();
		lastPage = scenarioHandlerRef.GetScenarioCount()+1;
		
		playerRef = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		
		answersList = new Dictionary<string,string>();
		questionsList = new KeyValueList();
		googleManager = new GoogleManager();
		
		FindAllQuestions();
		
		lastPage++;
		likertAnswers = new int[lastPage];
		wantBuildAnswers = new string[lastPage];
		commentAnswers = new string[lastPage];
		
		for (int i = 0; i < lastPage; i++)
		{
			likertAnswers[i] = 0;
			wantBuildAnswers[i] = "";
			commentAnswers[i] = "";
		}
		lastPage--;
	}
	
	void OnGUI() 
	{
		if (questionsList.Count == 0)
			return;
		
		if (GameStateHandler.GetCurrentGameState() == GameStateHandler.GameState.QUESTIONNAIRE)
		{		
			float width = Screen.width * 0.95f,
				  height = Screen.height * 0.95f;			
			
			if (!GUI.skin.box.wordWrap)
				GUI.skin.box.wordWrap = true;
			
			GUILayout.BeginArea(new Rect(5f, 5f, width, height));
			GUILayout.BeginVertical();
			scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
			
			foreach (KeyValuePair<string,string> pair in questionsList)
			{
				if (currentPage == 0)
					BuildDemographics(pair);
				else if (currentPage < lastPage)
					BuildDuring(pair);
				else
					BuildAfter(pair);
			}
			
			if (GetQuestionsAnswered())
			{
				if (currentPage < lastPage)
				{
					if (GUILayout.Button("Continue"))
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
					if (GUILayout.Button("Exit Game"))
					{
						SubmitAllAnswers(true);
						GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.END);
					}
				}
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
	
	private bool GetQuestionsAnswered()
	{
		bool bAnswered = true;
		foreach (KeyValuePair<string,string> answer in answersList)
		{
			//Debug.Log("Answer: " + answer.Key + " - " + answer.Value);
			if ((answer.Value == "" || answer.Key == "") && !answer.Key.Contains("comment"))
			{
				bAnswered = false;
				break;
			}
		}
		
		return bAnswered;
	}
	
	private void BuildAfter(KeyValuePair<string,string> pair)
	{
		BuildDuring(pair);
		
		if (pair.Key == "preferrence")
		{
			string[] preferrenceOptions = {"Mouse", "Keyboard"};
			
			preferrence = AddMultipleChoice(preferrenceOptions, pair.Value, pair.Key, preferrence);
		}
	}
	
	private void BuildDuring(KeyValuePair<string,string> pair)
	{
		if (pair.Key == "likert" + currentPage.ToString())
		{
			likertAnswers[currentPage] = AddLikertScale(pair.Value, pair.Key, likertAnswers[currentPage]);
		}
		
		if (pair.Key == "wantbuild" + currentPage.ToString())
		{
			wantBuildAnswers[currentPage] = AddEssay(wantBuildAnswers[currentPage], pair.Value, pair.Key);
		}
		
		if (pair.Key == "comments" + currentPage.ToString())
		{
			commentAnswers[currentPage] = AddEssay(commentAnswers[currentPage], pair.Value, pair.Key);
		}
	}
	
	private void BuildDemographics(KeyValuePair<string,string> pair)
	{
		if (pair.Key == "age")
		{
			string[] ageIntervals = {"14 or less", "15-19", "20-24", "25-29", "30 or more"};
			
			age = AddMultipleChoice(ageIntervals, pair.Value, pair.Key, age);
		}
		
		if (pair.Key == "gender")
		{
			string[] genders = {"Male", "Female"};
			
			gender = AddMultipleChoice(genders, pair.Value, pair.Key, gender);
		}
		
		if (pair.Key == "freqplaying")
		{
			string[] freqIntervals = {"Once per year or less", "More than once per month", "More than once per week", "More than once per day"};
		
			frequency = AddMultipleChoice(freqIntervals, pair.Value, pair.Key, frequency);
		}
		
		if (pair.Key == "amountplaying")
		{
			string[] amountIntervals = {"Less than 1 hour", "More than 1 hour", "More than 2 hours", "More than 4 hours", "More than 6 hours", "More than 8 hours"};
			
			amount = AddMultipleChoice(amountIntervals, pair.Value, pair.Key, amount);
		}
		
		if (pair.Key == "favourite")
		{	
			favouriteGame = AddEssay(favouriteGame, pair.Value, pair.Key);
		}
		
		if (pair.Key == "likert0")
		{
			experience = AddLikertScale(pair.Value, pair.Key, experience);
		}
	}
	
	private int AddLikertScale(string question, string key, int toolbarSelection)
	{
		string[] likertChoices = {"-3", "-2", "-1", "0", "+1", "+2", "+3"};
		
		toolbarSelection = AddMultipleChoice(likertChoices, question, key, toolbarSelection);
		
		return toolbarSelection;
	}
	
	private string AddEssay(string text, string question, string key) 
	{
		GUILayout.Box(question);
		
		text = GUILayout.TextArea(text);
		
		AddOrReplaceToDict(answersList, key, text);
		
		return text;
	}
	
	private int AddMultipleChoice(string[] toolbarOptions, string question, string key, int toolbarSelection)
	{
		GUILayout.Box(question);
		
		toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarOptions);
		
		AddOrReplaceToDict(answersList, key, toolbarOptions[toolbarSelection]);		
		
		return toolbarSelection;
	}
	
	private void SubmitAllAnswers(bool bUpdateRow)
	{
		string now = System.DateTime.Now.ToString("dd.MM.yyyy") + " " + System.DateTime.Now.ToString("HH:mm:ss");
		AddOrReplaceToDict(answersList, "timestamp" + currentPage.ToString(), now);
		
		string scenario = scenarioHandlerRef.GetCurrentScenario().ToString();
		AddOrReplaceToDict(answersList, "scenario" + currentPage.ToString(), scenario);
		
		string elapsedTime = playerRef.GetElapsedTime().ToString();
		AddOrReplaceToDict(answersList, "timespent" + currentPage.ToString(), elapsedTime);
		
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
	
	private void AddOrReplaceToDict(Dictionary<string,string> dict, string key, string value)
	{
		if (dict.ContainsKey(key))
			dict[key] = value;
		else
			dict.Add(key, value);
	}	
}