Attribute VB_Name = "Module1"
Sub JsonConvert()
Dim ws As Worksheet
Set ws = ActiveSheet '現在のシートを取得

Dim lastRow As Long
Dim lastCol As Long
lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row '最終行の数をセット
lastCol = ws.Cells(1, ws.Columns.Count).End(xlToLeft).Column '最終列の数をセット

Dim txt As Object
Set txt = CreateObject("ADODB.Stream")
txt.Type = 2 'テキストモードに設定
txt.Charset = "UTF-8"
txt.Open

txt.Writetext vbCrLf & "{ ""TextEventData"" : [" & vbCrLf, adWriteLine
Dim i As Long
Dim j As Long
For i = 2 To lastRow '最終行までループ
    txt.Writetext vbCrLf & vbTab & "{" & vbCrLf, adWriteLine
For j = 1 To lastCol '現在参照中の行を最終列までループ
    Dim key As String
    key = ws.Cells(1, j).value '先頭行のColumn取得
    Dim value As String
    value = ws.Cells(i, j).value 'セルの値取得
    
    txt.Writetext vbTab & vbTab & " """ & key & """ : """ & value & """ ", adWriteLine
    If j < lastCol Then '最終列でなければ","を付けて改行
        txt.Writetext "," & vbCrLf, adWriteLine
    End If
Next j
    txt.Writetext vbCrLf & vbTab & "}", adWriteLine
    If i < lastRow Then '最終行でなければ","を付けて改行
        txt.Writetext "," & vbCrLf, adWriteLine
    End If
Next i

txt.Writetext vbCrLf & "]}", adWriteLine '書き込み終了

'先頭から3Byte分スキップ
txt.Position = 0
txt.Type = 1
txt.Position = 3

Dim tmp() As Byte
tmp = txt.Read
txt.Close

' 保存先選択
    Dim filePath As String
    filePath = Application.GetSaveAsFilename("TextEventData.json", "JSON Files (*.json), *.json")
    If filePath = "False" Then Exit Sub

    ' 保存
    Set txt = CreateObject("ADODB.Stream")
    txt.Type = 1
    txt.Open
    txt.Write tmp
    txt.SaveToFile filePath, 2
    txt.Close

    MsgBox "JSONファイルを保存しました：" & filePath

End Sub

