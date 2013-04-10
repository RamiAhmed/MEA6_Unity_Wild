using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using Google.GData.Client;
using Google.GData.Spreadsheets;

public class GoogleManager 
{
	public string googleDocsAPIProjectName = "Unity Tests Project 1";
	public string googleUserName = "RamiAhmedBock@gmail.com";
	public string googlePassword = "u8FDa7d4P0";
	public string googleSpreadsheetName = "Unity_Test_1";

	private SpreadsheetsService service;
	private SpreadsheetEntry sheet;
	private WorksheetEntry worksheet;	
	
	// Use this for initialization
	public GoogleManager()
	{
		InitGoogleConnection();
	}
	
	public GoogleManager(string username, string password)
	{
		googleUserName = username;
		googlePassword = password;
		
		InitGoogleConnection();
	}
	
	public GoogleManager(string username, string password, string googleAPIProject, string googleSpreadsheet)
	{
		googleUserName = username;
		googlePassword = password;
		googleDocsAPIProjectName = googleAPIProject;
		googleSpreadsheetName = googleSpreadsheet;
		
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
	}
	
	public void PrintAllRows()
	{
		try
		{
			AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
			
			ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
			ListFeed listFeed = service.Query(listQuery);
			
			foreach (ListEntry row in listFeed.Entries)
			{
				foreach (ListEntry.Custom element in row.Elements)
				{
					Debug.Log(element.LocalName + " : " + element.Value);	
				}
			}
		}
		catch (WebException e)
		{
			Debug.LogWarning("PrintAllRows WebException: " + e);
		}		
		catch (Exception e)
		{
			Debug.LogWarning("PrintAllRows Exception: " + e);	
		}
	}
	
	public void WriteListToRow(KeyValueList rowToAdd)
	{
		try
		{		
			AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
	
			ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
			ListFeed listFeed = service.Query(listQuery);		
			
			ListEntry row = new ListEntry();
			foreach (KeyValuePair<string, string> pair in rowToAdd)
			{
				row.Elements.Add(new ListEntry.Custom() { LocalName = pair.Key, Value = pair.Value } );			
			}	
			
			service.Insert(listFeed, row);
		}
		catch (WebException e)
		{
			Debug.LogWarning("WriteListToCell WebException: " + e);
		}
		catch (Exception e)
		{
			Debug.LogWarning("WriteListToCell Exception: " + e);
		}
	}
}

