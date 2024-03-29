using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {
	
	public bool bQuestionnaireEnabled = true;
	
	private GoogleManager googleManager = null;
	private Dictionary<string,string> questionsList;
	private Dictionary<string,string> helperTextList;
	private Dictionary<string,string> answersList;
	
	private int currentPage = 0,
				lastPage = 1; 
	
	private ScenarioHandler scenarioHandlerRef;
	private PlayerController playerRef;
	
	private int age = 0, gender = 0, frequency = 0, amount = 0, experience = 0, preferrence = 0;
	private string favouriteGame = "";	
	
	private int[] likertAnswers;
	private string[] wantBuildAnswers;
	private string[] commentAnswers;
	
	private GUIStyle windowStyle = new GUIStyle();
	private Rect questionnaireRect;
	private float qWidth = 600, qHeight = 400;
	
	void Start() 
	{
		scenarioHandlerRef = this.GetComponent<ScenarioHandler>();
		
		if (bQuestionnaireEnabled)
		{
			googleManager = this.GetComponent<GoogleManager>();
			playerRef = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			
			answersList = new Dictionary<string,string>();
			questionsList = new Dictionary<string,string>();
			helperTextList = new Dictionary<string, string>();
			
			FindAllQuestions();
	
			lastPage = scenarioHandlerRef.GetScenarioCount()+1;		
			
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
			
			float screenWidth = Screen.width * 0.95f,
				screenHeight = Screen.height * 0.90f;
			qWidth = (screenWidth > qWidth) ? screenWidth : qWidth;
			qHeight = (screenHeight > qHeight) ? screenHeight : qHeight;		
			
			questionnaireRect = new Rect((Screen.width/2f) - (qWidth/2f), (Screen.height/2f) - (qHeight/2f), qWidth, qHeight);
			
			windowStyle.normal.background = MakeColorTexture((int)qWidth+1, (int)qHeight+1, Color.black);
		}
	}
	
	void OnGUI() 
	{
		if (bQuestionnaireEnabled)
		{
			if (questionsList == null)
				return;
					
			if (questionsList.Count == 0)
				return;
			
			if (GameStateHandler.GetCurrentGameState() == GameStateHandler.GameState.QUESTIONNAIRE)
			{		
				questionnaireRect = GUILayout.Window(0, questionnaireRect, DrawQuestionnaire, "", windowStyle);
				
				GUI.BringWindowToFront(0);
			}
		}
		else
		{
			if (GameStateHandler.GetCurrentGameState() == GameStateHandler.GameState.QUESTIONNAIRE)
			{
				scenarioHandlerRef.GetNewRandomScenario();
				GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);
			}
		}
	}
	
	void DrawQuestionnaire(int windowID)
	{
		if (!GUI.skin.box.wordWrap)
			GUI.skin.box.wordWrap = true;
		
		GUILayout.BeginVertical(windowStyle);
		
		GUILayout.Box("Questionnaire");		
		
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
			GUILayout.FlexibleSpace();
			
			if (currentPage < lastPage)
			{
				if (GUILayout.Button("Continue", GUILayout.Height(40)))
				{
					SubmitAllAnswers();
					
					if (currentPage > 0)
					{
						scenarioHandlerRef.GetNewRandomScenario();
						GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);
					}
					
					currentPage++;
				}
			}
			else
			{
				if (GUILayout.Button("Exit Game", GUILayout.Height(40)))
				{
					SubmitAllAnswers();
					UploadAllAnswers();
					
					GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.END);
				}
			}
		}
		
		GUILayout.EndVertical();	
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
			string[] preferrenceOptions = {"Mouse", "Keyboard"}; // update with scenarios
			
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
		if (helperTextList.ContainsKey(key))
		{
			question += "\n" + helperTextList[key];
		}		
		
		GUILayout.Box(question);			
		
		text = GUILayout.TextArea(text, GUILayout.Height(40));
		
		AddOrReplaceToDict(answersList, key, text);
		
		GUILayout.Space(15);
		
		return text;
	}
	
	private int AddMultipleChoice(string[] toolbarOptions, string question, string key, int toolbarSelection)
	{
		if (helperTextList.ContainsKey(key))
		{	
			question += "\n" + helperTextList[key];
		}	
		
		GUILayout.Box(question);
		
		toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarOptions);
		
		AddOrReplaceToDict(answersList, key, toolbarOptions[toolbarSelection]);	
		
		GUILayout.Space(15);
		
		return toolbarSelection;
	}
	
	private void SubmitAllAnswers()
	{
		string now = System.DateTime.Now.ToString("dd.MM.yyyy") + " " + System.DateTime.Now.ToString("HH:mm:ss");
		AddOrReplaceToDict(answersList, "timestamp" + currentPage.ToString(), now);
		
		string scenario = scenarioHandlerRef.GetCurrentScenario().ToString();
		AddOrReplaceToDict(answersList, "scenario" + currentPage.ToString(), scenario);
		
		string elapsedTime = playerRef.GetElapsedTime().ToString();
		AddOrReplaceToDict(answersList, "timespent" + currentPage.ToString(), elapsedTime);
		
		string completion = playerRef.GetAssignmentCompleted().ToString();
		AddOrReplaceToDict(answersList, "completed" + currentPage.ToString(), completion);
	}
	
	private void UploadAllAnswers()
	{
		googleManager.WriteDictToRow(answersList, false);
		answersList.Clear();
	}
	
	private void FindAllQuestions()
	{		
		List<string> keys = new List<string>();
		List<string> values = new List<string>();
		List<string> helpers = new List<string>();
		
		TextAsset questionsFile = Resources.Load("Data/questionsSheet", typeof(TextAsset)) as TextAsset;
		if (questionsFile != null)
		{
			int currentRow = 1;
			
			string[] questions = questionsFile.text.Split(","[0]);
			foreach (string q in questions)
			{
				if (q.Contains("\n"))
				{
					int breakIndex = q.IndexOf("\n");
					string q1 = q.Substring(0, breakIndex-1);	
					string q2 = q.Substring(breakIndex+1, q.Length - (breakIndex+2));
					
					if (currentRow == 1)
					{
						keys.Add(q1);
						values.Add(q2);
					}
					else
					{
						values.Add(q1);
						helpers.Add(q2);
					}
					
					currentRow++;
				}
				else
				{
					switch (currentRow)
					{
						case 1: keys.Add(q); break;
						case 2: values.Add(q); break;
						case 3: helpers.Add(q); break;
					}
				}
			}
			
			for (int i = 0; i < keys.Count; i++)
			{
				string key = keys[i];
				string value = values[i];
				string helper = helpers[i];
				
				if (value[0].Equals('\"'))
				{
					value = value.Replace("\"\"", "\"");
					value = value.Substring(1, value.Length-2);
				}
				
				AddOrReplaceToDict(questionsList, key, value);
				if (helper.Length > 0)
					AddOrReplaceToDict(helperTextList, key, helper);
			}
		}
		else
			Debug.LogWarning("Could not find questions file");
	}
	
	private void AddOrReplaceToDict(Dictionary<string,string> dict, string key, string value)
	{
		if (dict.ContainsKey(key))
			dict[key] = value;
		else
			dict.Add(key, value);
	}	
	
	private Texture2D MakeColorTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];

        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}