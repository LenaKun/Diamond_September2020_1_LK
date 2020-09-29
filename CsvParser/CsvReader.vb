Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Data
Imports System.IO
Imports Microsoft.VisualBasic.FileIO


Public Class CsvReader

    Public Shared Function ReadFile(ByVal fileName As String, Optional HasColumnNames As Boolean = False) As DataTable ' List(Of String())

        ' Initialize the return values

        Dim dt As New DataTable
        Dim i As Integer
        Using parser As New TextFieldParser(fileName)

            ' Setup the comma-delimited file parser.
            parser.TextFieldType = FieldType.Delimited
            parser.Delimiters = New String() {","}
            parser.HasFieldsEnclosedInQuotes = True
           
            While Not parser.EndOfData
                Try
                    ' Read the comma-delimited text as fields into a string array.
                    Dim input As String() = parser.ReadFields()
                    If dt.Columns.Count = 0 Then
                        If HasColumnNames Then
                            For Each fld As String In input
                                dt.Columns.Add(fld)
                            Next
                        Else
                            For i = 0 To input.Length
                                dt.Columns.Add("Field" & i)
                            Next

                        End If
                    Else
                        dt.Rows.Add(input)
                    End If
                    
                Catch ex As MalformedLineException
                    ' Ignore mal-formed lines.
                Catch ex As Exception

                End Try
            End While

        End Using

        Return dt

    End Function

End Class

