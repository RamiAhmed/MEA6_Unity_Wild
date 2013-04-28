using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using Google.GData.Client;
using Google.GData.Spreadsheets;

public class GoogleManager : MonoBehaviour
{
	public string googleUserName = "username";
	public string googlePassword = "password";	
	
	// should probably be public at some point
	private string googleDocsAPIProjectName = "Unity Tests Project 1";
	private string googleSpreadsheetName = "Interaction Tests Responses";

	private SpreadsheetsService service;
	private SpreadsheetEntry sheet;
	private WorksheetEntry worksheet;	
	
	void Awake()
	{
		InitGoogleConnection();	
	}
	
	private void InitGoogleConnection()
	{
		DumbSecurityCertificatePolicy.Instate();
		AuthenticateClientLogin(googleUserName, googlePassword);
		FindSpreadsheet();		
	}
	
    private void AuthenticateClientLogin(string pUsername, string pPassword)
    {
        // Create the service and set user credentials.
        service = new SpreadsheetsService(googleDocsAPIProjectName);
        service.setUserCredentials(pUsername, pPassword);
    }	
	
	private void FindSpreadsheet()
	{
		SpreadsheetQuery query = new SpreadsheetQuery();
		SpreadsheetFeed feed = service.Query(query);

		if (feed.Entries.Count <= 0)
			Debug.LogWarning("No spreadsheets found");
		
		foreach (SpreadsheetEntry spreadsheet in feed.Entries)
		{
			if (spreadsheet.Title.Text == googleSpreadsheetName)
			{
				sheet = spreadsheet;
				break;
			}
		}

		WorksheetFeed wsFeed = sheet.Worksheets;
		worksheet = (WorksheetEntry)wsFeed.Entries[0];
		
		if (wsFeed.Entries.Count <= 0)
			Debug.LogWarning("No sheets in spreadsheet found");
	}
	
	public string GetCellValue(int row, int column)
	{
		string cellValue = "";
		try
		{
			CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
			cellQuery.MinimumRow = (uint)row;
			cellQuery.MaximumRow = (uint)row;
			cellQuery.MinimumColumn = (uint)column;
			cellQuery.MaximumColumn = (uint)column;
			CellFeed cellFeed = service.Query(cellQuery);
			
			foreach (CellEntry cell in cellFeed.Entries)
			{
				cellValue = cell.Value.ToString();	
			}
			
		}
		catch (WebException e)
		{
			Debug.LogWarning("GetCellValue WebException: " + e);	
		}
		catch (Exception e)
		{
			Debug.LogWarning("GetCellValue Exception: " + e);	
		}
		
		return cellValue;
	}
	
	public void WriteDictToRow(Dictionary<string,string> dict, bool bUpdateRow)
	{
		try
		{		
			AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
	
			ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
			ListFeed listFeed = service.Query(listQuery);		
			
			if (!bUpdateRow)
			{
				ListEntry row = new ListEntry();
				
				foreach (KeyValuePair<string, string> pair in dict)
				{
					row.Elements.Add(new ListEntry.Custom() { LocalName = pair.Key, Value = pair.Value } );			
				}	
			
				service.Insert(listFeed, row);
			}
			else
			{
				ListEntry row = (ListEntry)listFeed.Entries[listFeed.Entries.Count-1];
				
				foreach (ListEntry.Custom element in row.Elements)
				{
					foreach (KeyValuePair<string, string> pair in dict)
					{
						if (element.LocalName == pair.Key)
						{
							element.Value = pair.Value;	
						}
					}
				}
				
				row.Update();
			}
		}
		catch (WebException e)
		{
			Debug.LogWarning("WriteDictToRow WebException: " + e);
		}
		catch (Exception e)
		{
			Debug.LogWarning("WriteDictToRow Exception: " + e);
		}			
	}
}

