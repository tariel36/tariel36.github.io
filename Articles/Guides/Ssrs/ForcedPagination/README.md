# Abstract

This article describes how to implement forced pagination in SSRS report.

# Instructions

## 01. Add VBA function

1. Open report designer;
2. Right click on the free space in report designer;
3. Click `Report properties`;
4. Go to `Code` tab;

Add function that will report current page number:

```VB
Function PageNumber() As Integer
    Return CInt(Me.Report.Globals!PageNumber)
End Function
```

Add function that will filter query rows based on the page. The input are row ids with the page they're on and the page you want. As output your receive row ids.

```VB
Function GetPagedRows(ByVal values As Object(), ByVal pageNbr As Integer) As Object()
    If values IsNot Nothing Then
        Dim checkedRows As String = Strings.Join(values, ",")
        Dim separateRows As String() = checkedRows.Split(",")
        Dim resultStr As String = ""

        For i As Integer = 0 To separateRows.Length - 1

            If separateRows(i) IsNot Nothing Then
                Dim taskSplit As String() = separateRows(i).Split("-")

                If taskSplit.length = 2 AndAlso taskSplit(0) = pageNbr.ToString() AndAlso taskSplit(1) IsNot Nothing Then

                    If resultStr.Length > 0 Then
                        resultStr = resultStr & "," & taskSplit(1)
                    Else
                        resultStr = taskSplit(1)
                    End If
                End If
            End If
        Next

        If resultStr.Length > 0 Then
            Dim result As Object() = resultStr.Split(",")
            Return result
        End If
    End If

    Return New Object(-1) {}
End Function
```

Example input:

```
values: new object[] { "1-250", "1-252", "1-253", "2-254", "2-255" }
pageNbr: 1
```

Example output:

```
new object[] { "250", "252", "253" }
```

When you're done, click `OK`.

## 02. Add report parameters

Add two hidden parameters:

* List of the `page-rowid` pairs that have to be in the same order that the data you display;
* `Optional` Page size;


| Name | Prompt | Data type | Allow blank values ("") | Allow null value | Allow multiple values | Select parameter visibility: | Available Values | Value | Default Values | Value | Refresh data when parameter changes | Report Part Notifications |
| ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ | ------ |
| PagedRows | PagedRows | Text | [ ] | [ ] | [x] | Hidden | [x] Get values from | Dataset: `PagedRows`; Value field: `pagedIdRow`; Label field: `pagedIdRow` | [x] Get values from a query | Dataset: `PagedTasks`; Value field: `pagedIdRow` | [x] Always refresh | [x] Notify me when this report part is updated on the server |
| PageSize | PageSize | Integer | [ ] | [ ] | [ ] | `Hidden` | [x] Specify values | Label: 20; Value: 20; | `Specify values` | 20 | [x] Automatically determine when to refresh | [x] Notify me when this report part is updated on the server |

## 03. Add `Tablix` grouping
Add default gruoping to the `Tablix` that you want to paginate. Ensure that selected `Tablix` is named `Details`.

Add new grouping called `pagebreak`. Right click on `Details` -> `Add group` -> `Parent group`. Check `Group by:` and click on `fx` button. Paste following expression:
```VB
=CEILING(RowNumber(Nothing)/Parameters!PageSize.Value)
```
Leave `Add group header` and `Add group footer` unchecked. Confirm with `OK`.

Right click on the `pagebreak` grouping and `Group Properties`. Provide proper name in `General`, then in `Page breaks` tab check `Between each instance of a group`, then in `Sorting` tab remove all sorts. Confirm with `OK`.

Remove grouping columns. You will be asked whether remove groups too. You don't want to remove groups, so no.

> Be aware that it's only logical pagination, so ensure that the data you paginate will fit on single page, otherwise SSRS may introduce it's own pagination or page breaking.

## 04. Example TSQL procedures

```SQL
create PROCEDURE [RowsSource]
    , @pageNbr int = null
    , @pageSize int = null
AS

SELECT
    -- Provide your own field instead of `name`.
    CEILING(CAST(ROW_NUMBER() OVER (ORDER BY name) as float) / COALESCE(@pageSize, 1)) [page],
    -- Provide your own field instead of `name` and `id`.
    -- This part provides the required `row id-id` format.
    CONCAT(CEILING(CAST(ROW_NUMBER() OVER (ORDER BY name) as float) / COALESCE(@pageSize, 1)), '-', id) [pagedIdRow],
    id,
    name,
FROM @tasks
ORDER BY [name]

```

## 05. Additional reading
* [https://www.sqlchick.com/entries/2010/9/11/displaying-fixed-number-of-rows-per-ssrs-report-page.html](https://www.sqlchick.com/entries/2010/9/11/displaying-fixed-number-of-rows-per-ssrs-report-page.html)
