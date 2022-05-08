# Abstract

This article explains how to query SSRS with C#.

# Instructions

# Available channels

You can query SSRS by:

* SOAP;
* WebForms;
* HTTP;

## SOAP

See this article [https://docs.microsoft.com/en-us/sql/reporting-services/report-server-web-service/accessing-the-soap-api?view=sql-server-ver15](https://docs.microsoft.com/en-us/sql/reporting-services/report-server-web-service/accessing-the-soap-api?view=sql-server-ver15).

## WebForms

See this article [https://www.codeproject.com/Articles/675762/Call-SSRS-Reports-by-using-Csharp](https://www.codeproject.com/Articles/675762/Call-SSRS-Reports-by-using-Csharp).

## HTTP

What you do here is simulation user input. This approach may be tricky. What you have to do is to execute various `GET` and `POST` queries with proper parameters.


| Parameter | Description |
| ------ | ------ |
| rs:Command=Render | Indicates that report should be rendered. |
| rs:Format=PDF | The output format, for example `PDF`. There are multiple valid formats. |

Then just execute proper C# code with query:
```CS
const string ssrsLogin = "";
const string ssrsPass = "";

NetworkCredential networkCredential = new NetworkCredential(ssrsLogin, ssrsPass, ssrsDomain);

CredentialCache credentialCache = credentialCache = new CredentialCache()
{
	{
		new Uri(reportServerAddress), "Ntlm", networkCredential
	}
};

string baseUrl = "http://address/ReportServer/Pages/ReportViewer.aspx?";
string parameters = "/{athToReport/ReportName&rs:Command=Render&rs:Format=PDF&ReportParameter1=false&ReportParameter2=123456";
string fullUrl = baseUrl + parameters;

HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
request.Credentials = credentialCache;

using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
{
	if (response.StatusCode == HttpStatusCode.OK)
	{
		using (MemoryStream ms = new MemoryStream())
		{
			response.GetResponseStream().CopyTo(ms);
			System.IO.File.WriteAllBytes(filePath, ms.ToArray());
		}
	}

	response.Close();
}
```