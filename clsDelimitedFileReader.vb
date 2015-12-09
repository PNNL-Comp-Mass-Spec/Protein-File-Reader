Option Strict On

' This class can be used to open a delimited file with protein information and return each protein present
'
' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Program started in October 2004
'
' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
' -------------------------------------------------------------------------------
' 
' Licensed under the Apache License, Version 2.0; you may not use this file except
' in compliance with the License.  You may obtain a copy of the License at 
' http://www.apache.org/licenses/LICENSE-2.0
'

Public Class DelimitedFileReader
    Inherits ProteinFileReaderBaseClass

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"

    ''' <summary>
    ''' Columns present in the delimited text file
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum eDelimitedFileFormatCode
        SequenceOnly = 0
        ProteinName_Sequence = 1
        ProteinName_Description_Sequence = 2
        UniqueID_Sequence = 3
        ProteinName_PeptideSequence_UniqueID = 4
        ProteinName_PeptideSequence_UniqueID_Mass_NET = 5
        ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore = 6
        UniqueID_Sequence_Mass_NET = 7
    End Enum
#End Region

#Region "Classwide Variables"
    Private mDelimiter As Char
    Private mDelimitedFileFormatCode As eDelimitedFileFormatCode
    Private mSkipFirstLine As Boolean
    Private mFirstLineSkipped As Boolean
#End Region

#Region "Interface Functions"

    ''' <summary>
    ''' Delimiter between columns
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Default is tab</remarks>
    Public Property Delimiter() As Char
        Get
            Return mDelimiter
        End Get
        Set(ByVal Value As Char)
            If Value <> String.Empty Then
                mDelimiter = Value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Delimited file format code
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DelimitedFileFormatCode() As eDelimitedFileFormatCode
        Get
            Return mDelimitedFileFormatCode
        End Get
        Set(ByVal Value As eDelimitedFileFormatCode)
            mDelimitedFileFormatCode = Value
        End Set
    End Property

    ''' <summary>
    ''' Protein Name or Protein Name and Description
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>If file format is eDelimitedFileFormatCode.SequenceOnly, returns the protein sequence</remarks>
    Public Overloads ReadOnly Property HeaderLine() As String
        Get
            Try
                Select Case mDelimitedFileFormatCode
                    Case eDelimitedFileFormatCode.SequenceOnly
                        Return mCurrentEntry.HeaderLine
                    Case eDelimitedFileFormatCode.ProteinName_Sequence
                        Return mCurrentEntry.Name
                    Case eDelimitedFileFormatCode.ProteinName_Description_Sequence
                        With mCurrentEntry
                            If Not String.IsNullOrWhiteSpace(.Description) Then
                                Return .Name & mDelimiter & .Description
                            Else
                                Return .Name
                            End If
                        End With
                    Case eDelimitedFileFormatCode.UniqueID_Sequence, eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET
                        Return mCurrentEntry.Name
                    Case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID, eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore
                        With mCurrentEntry
                            Return mCurrentEntry.Name & mDelimiter & .UniqueID.ToString
                        End With
                    Case Else
                        Debug.Assert(False, "Unknown file format code: " & mDelimitedFileFormatCode.ToString)
                        Return mCurrentEntry.HeaderLine
                End Select
            Catch ex As Exception
                Return mCurrentEntry.HeaderLine
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Controls whether to skip the first line
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SkipFirstLine() As Boolean
        Get
            Return mSkipFirstLine
        End Get
        Set(ByVal Value As Boolean)
            mSkipFirstLine = Value
        End Set
    End Property
#End Region

    Private Sub InitializeLocalVariables()

        mDelimiter = ControlChars.Tab
        mDelimitedFileFormatCode = eDelimitedFileFormatCode.ProteinName_Description_Sequence

    End Sub

    Protected Function IsNumber(ByVal strText As String) As Boolean
        Dim dblValue As Double
        Return Double.TryParse(strText, dblValue)
    End Function

    ''' <summary>
    ''' Reads the next entry in delimited protein (or delimited peptide) file
    ''' </summary>
    ''' <returns>True if an entry is found, otherwise false</returns>
    ''' <remarks>This function will update variable mFileLineSkipCount if any lines are skipped due to having an invalid format</remarks>
    Public Overrides Function ReadNextProteinEntry() As Boolean

        Const MAX_SPLIT_LINE_COUNT As Integer = 8
        Dim strLineIn As String
        Dim strSplitLine() As String
        Dim strSepChars() As Char = New Char() {mDelimiter}

        Dim blnEntryFound As Boolean

        blnEntryFound = False
        mFileLineSkipCount = 0

        If Not mProteinFileInputStream Is Nothing Then

            Try
                If mSkipFirstLine And Not mFirstLineSkipped Then
                    mFirstLineSkipped = True

                    If Not mProteinFileInputStream.EndOfStream Then
                        strLineIn = mProteinFileInputStream.ReadLine

                        If Not String.IsNullOrWhiteSpace(strLineIn) Then
                            mFileBytesRead += strLineIn.Length + 2
                            mFileLinesRead += 1
                        End If
                    End If
                End If

                ' Read the file until the next valid line is found
                Do While Not blnEntryFound AndAlso Not mProteinFileInputStream.EndOfStream
                    strLineIn = mProteinFileInputStream.ReadLine

                    If Not String.IsNullOrWhiteSpace(strLineIn) Then
                        mFileBytesRead += strLineIn.Length + 2
                        mFileLinesRead += 1

                        strLineIn = strLineIn.Trim()
                        strSplitLine = strLineIn.Split(strSepChars, MAX_SPLIT_LINE_COUNT)

                        EraseProteinEntry(mCurrentEntry)

                        Select Case mDelimitedFileFormatCode
                            Case eDelimitedFileFormatCode.SequenceOnly
                                If strSplitLine.Length >= 1 Then
                                    ' Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                    If Not IsNumber(strSplitLine(0)) Then
                                        With mCurrentEntry
                                            .HeaderLine = strLineIn
                                            .Name = String.Empty
                                            .Description = String.Empty
                                            .Sequence = strSplitLine(0)
                                        End With
                                        blnEntryFound = True
                                    End If
                                End If

                            Case eDelimitedFileFormatCode.ProteinName_Sequence
                                If strSplitLine.Length >= 2 Then
                                    ' Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                    If Not IsNumber(strSplitLine(1)) Then
                                        With mCurrentEntry
                                            .HeaderLine = strLineIn
                                            .Name = strSplitLine(0)
                                            .Description = String.Empty
                                            .Sequence = strSplitLine(1)
                                        End With
                                        blnEntryFound = True
                                    End If
                                End If
                            Case eDelimitedFileFormatCode.ProteinName_Description_Sequence
                                If strSplitLine.Length >= 3 Then
                                    ' Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                    If Not IsNumber(strSplitLine(2)) Then
                                        With mCurrentEntry
                                            .HeaderLine = strLineIn
                                            .Name = strSplitLine(0)
                                            .Description = strSplitLine(1)
                                            .Sequence = strSplitLine(2)
                                        End With
                                        blnEntryFound = True
                                    End If
                                End If
                            Case eDelimitedFileFormatCode.UniqueID_Sequence, eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET
                                If strSplitLine.Length >= 2 Then
                                    ' Only process the line if the first column is numeric (useful for skipping header lines)
                                    ' Also require that the sequence column is not a number
                                    If IsNumber(strSplitLine(0)) And Not IsNumber(strSplitLine(1)) Then
                                        With mCurrentEntry
                                            .HeaderLine = strLineIn
                                            .Name = String.Empty
                                            .Description = String.Empty
                                            .Sequence = strSplitLine(1)
                                            Try
                                                .UniqueID = Integer.Parse(strSplitLine(0))
                                            Catch ex As Exception
                                                .UniqueID = 0
                                            End Try

                                            If strSplitLine.Length >= 4 Then
                                                Try
                                                    .Mass = Double.Parse(strSplitLine(2))
                                                Catch ex As Exception
                                                End Try

                                                Try
                                                    .NET = Single.Parse(strSplitLine(3))
                                                Catch ex As Exception
                                                End Try
                                            End If
                                        End With
                                        blnEntryFound = True
                                    End If
                                End If
                            Case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID, eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore
                                If strSplitLine.Length >= 3 Then
                                    ' Only process the line if the third column is numeric (useful for skipping header lines)
                                    ' Also require that the sequence column is not a number
                                    If IsNumber(strSplitLine(2)) And Not IsNumber(strSplitLine(1)) Then
                                        With mCurrentEntry
                                            .HeaderLine = strLineIn
                                            .Name = strSplitLine(0)
                                            .Description = String.Empty
                                            .Sequence = strSplitLine(1)
                                            Try
                                                .UniqueID = Integer.Parse(strSplitLine(2))
                                            Catch ex As Exception
                                                .UniqueID = 0
                                            End Try

                                            If strSplitLine.Length >= 5 Then
                                                Try
                                                    .Mass = Double.Parse(strSplitLine(3))
                                                    .NET = Single.Parse(strSplitLine(4))
                                                Catch ex As Exception
                                                End Try

                                                If strSplitLine.Length >= 7 Then
                                                    Try
                                                        .NETStDev = Single.Parse(strSplitLine(5))
                                                        .DiscriminantScore = Single.Parse(strSplitLine(6))
                                                    Catch ex As Exception
                                                    End Try
                                                End If
                                            End If
                                        End With
                                        blnEntryFound = True
                                    End If
                                End If
                            Case Else
                                Debug.Assert(False, "Unknown file format code: " & mDelimitedFileFormatCode.ToString)
                                blnEntryFound = False
                        End Select

                        If Not blnEntryFound Then
                            mFileLineSkipCount += 1
                        End If

                    End If
                Loop

            Catch ex As Exception
                ' Error reading the input file
                ' Ignore any errors
                blnEntryFound = False
            End Try
        End If

        If Not blnEntryFound Then
            EraseProteinEntry(mCurrentEntry)
        End If

        Return blnEntryFound

    End Function

    ''' <summary>
    ''' Open the delimited text file
    ''' </summary>
    ''' <param name="strInputFilePath"></param>
    ''' <returns>True if success, false if a problem</returns>
    ''' <remarks></remarks>
    Public Overrides Function OpenFile(ByVal strInputFilePath As String) As Boolean

        ' Reset the first line tracking variable
        mFirstLineSkipped = False

        ' Call OpenFile in the base class
        Return MyBase.OpenFile(strInputFilePath)
    End Function
End Class
