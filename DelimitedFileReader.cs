// This class can be used to open a delimited file with protein information and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in October 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace ProteinFileReader
{
    /// <summary>
    /// Class for reading tab-delimited protein files
    /// </summary>
    public sealed class DelimitedProteinFileReader : ProteinFileReaderBaseClass
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DelimitedProteinFileReader()
        {
            InitializeLocalVariables(ProteinFileFormatCode.ProteinName_Description_Sequence);
        }

        /// <summary>
        /// Constructor that accepts a text file path
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="fileFormat"></param>
        public DelimitedProteinFileReader(string inputFilePath, ProteinFileFormatCode fileFormat = ProteinFileFormatCode.ProteinName_Description_Sequence)
        {
            InitializeLocalVariables(fileFormat);
            OpenFile(inputFilePath);
        }

        #region "Constants and Enums"

        /// <summary>
        /// Columns present in the delimited text file
        /// </summary>
        public enum ProteinFileFormatCode
        {
            /// <summary>
            /// File contains protein sequences, one per line
            /// </summary>
            SequenceOnly = 0,

            /// <summary>
            /// Each line of the file contains a protein name, tab, sequence
            /// </summary>
            ProteinName_Sequence = 1,

            /// <summary>
            /// Each line of the file contains a protein name, description, and sequence
            /// </summary>
            ProteinName_Description_Sequence = 2,

            /// <summary>
            /// Each line of the file contains a unique id and protein sequence
            /// </summary>
            UniqueID_Sequence = 3,

            /// <summary>
            /// Each line of the file contains a protein name, peptide sequence, and unique id
            /// </summary>
            ProteinName_PeptideSequence_UniqueID = 4,

            /// <summary>
            /// Each line of the file contains a protein name, peptide sequence, unique id, mass, and NET
            /// </summary>
            ProteinName_PeptideSequence_UniqueID_Mass_NET = 5,

            /// <summary>
            /// Each line of the file contains a protein name, peptide sequence, unique id, mass, NET, NET Standard deviation, and discriminant score
            /// </summary>
            ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore = 6,

            /// <summary>
            /// Each line of the file contains a unique id, sequence, mass, and NET
            /// </summary>
            UniqueID_Sequence_Mass_NET = 7,

            /// <summary>
            /// Each line of the file contains a protein name, description, sequence hash, and sequence
            /// </summary>
            ProteinName_Description_Hash_Sequence = 8
        }

        #endregion

        #region "Class wide Variables"

        private CsvHelper.CsvReader mCsvReader;

        private char mDelimiter;

        private bool mFirstLineSkipped;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Delimiter between columns
        /// </summary>
        /// <remarks>
        /// Default is a tab character
        /// </remarks>
        public char Delimiter
        {
            get => mDelimiter;
            set
            {
                if (value.ToString() != string.Empty)
                {
                    mDelimiter = value;
                }
            }
        }

        /// <summary>
        /// Delimited file format code
        /// </summary>
        public ProteinFileFormatCode DelimitedFileFormatCode { get; set; }

        /// <summary>
        /// Protein Name or Protein Name and Description
        /// </summary>
        /// <remarks>
        /// If file format is DelimitedFileFormatCode.SequenceOnly, returns the protein sequence
        /// </remarks>
        public override string HeaderLine
        {
            get
            {
                try
                {
                    switch (DelimitedFileFormatCode)
                    {
                        case ProteinFileFormatCode.SequenceOnly:
                            return mCurrentEntry.HeaderLine;

                        case ProteinFileFormatCode.ProteinName_Sequence:
                            return mCurrentEntry.Name;

                        case ProteinFileFormatCode.ProteinName_Description_Sequence:
                        case ProteinFileFormatCode.ProteinName_Description_Hash_Sequence:
                            if (!string.IsNullOrWhiteSpace(mCurrentEntry.Description))
                            {
                                return mCurrentEntry.Name + mDelimiter + mCurrentEntry.Description;
                            }

                            return mCurrentEntry.Name;

                        case ProteinFileFormatCode.UniqueID_Sequence:
                        case ProteinFileFormatCode.UniqueID_Sequence_Mass_NET:
                            return mCurrentEntry.Name;

                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                            return mCurrentEntry.Name + mDelimiter + mCurrentEntry.UniqueID;

                        default:
                            throw new Exception("Unknown file format code: " + DelimitedFileFormatCode);
                    }
                }
                catch (Exception)
                {
                    return mCurrentEntry.HeaderLine;
                }
            }
        }

        /// <summary>
        /// Controls whether to skip the first line (since it is a header line)
        /// </summary>
        public bool SkipFirstLine { get; set; }

        #endregion

        /// <summary>
        /// Close the data file
        /// </summary>
        public override bool CloseFile()
        {
            mCsvReader?.Dispose();

            // Call CloseFile in the base class
            return base.CloseFile();
        }

        private void InitializeLocalVariables(ProteinFileFormatCode fileFormat)
        {
            mDelimiter = '\t';
            DelimitedFileFormatCode = fileFormat;
        }

        /// <summary>
        /// Determine if the provided string can be parsed as a number
        /// </summary>
        /// <param name="text"></param>
        private bool IsNumber(string text)
        {
            return double.TryParse(text, out _);
        }

        /// <summary>
        /// Open the delimited text file
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns>True if success, false if a problem</returns>
        public override bool OpenFile(string inputFilePath)
        {
            // Reset the first line tracking variable
            mFirstLineSkipped = false;

            // Call OpenFile in the base class
            var fileOpened = base.OpenFile(inputFilePath);

            if (!fileOpened)
                return false;

            if (Path.GetExtension(inputFilePath).EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && mDelimiter != ',')
            {
                Console.WriteLine();
                Console.WriteLine("Note: auto-changing the delimiter to a comma since the input file's extension is .csv");
                Console.WriteLine();
                mDelimiter = ',';
            }

            var configuration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = mDelimiter.ToString()
            };

            mCsvReader = new CsvHelper.CsvReader(mProteinFileInputStream, configuration);

            return true;
        }

        private bool ParseNameDescriptionSequenceLine(
            string dataLine,
            IList<string> rowData,
            int colIndexName,
            int colIndexDescription,
            int colIndexSequence)
        {
            // Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
            if (IsNumber(rowData[colIndexSequence]))
                return false;

            mCurrentEntry.HeaderLine = dataLine;
            mCurrentEntry.Name = colIndexName >= 0 ? rowData[colIndexName].Trim() : string.Empty;
            mCurrentEntry.Description = colIndexDescription >= 0 ? rowData[colIndexDescription].Trim() : string.Empty;

            if (!DiscardProteinResidues)
                mCurrentEntry.Sequence = rowData[colIndexSequence];

            return true;
        }

        /// <summary>
        /// Reads the next entry in a delimited protein (or delimited peptide) file
        /// </summary>
        /// <remarks>
        /// This function will update variable mFileLineSkipCount if any lines are skipped due to having an invalid format
        /// </remarks>
        /// <returns>True if an entry is found, otherwise false</returns>
        public override bool ReadNextProteinEntry()
        {
            var entryFound = false;

            mFileLineSkipCount = 0;

            if (mProteinFileInputStream == null || mCsvReader == null)
            {
                mCurrentEntry.Clear();
                return false;
            }

            try
            {
                if (SkipFirstLine && !mFirstLineSkipped)
                {
                    mFirstLineSkipped = true;

                    mCsvReader.Read();
                    mCsvReader.ReadHeader();

                    var headerNames = new List<string>();

                    if (mCsvReader.Parser.Context.Reader.HeaderRecord != null)
                    {
                        headerNames.AddRange(mCsvReader.Parser.Context.Reader.HeaderRecord.Select(headerName => headerName ?? string.Empty));
                    }

                    var headerLine = string.Join(mDelimiter.ToString(), headerNames);

                    if (!string.IsNullOrWhiteSpace(headerLine))
                    {
                        mFileBytesRead += headerLine.Length + 2;
                        mFileLinesRead++;
                    }
                }

                // Read the file until the next valid line is found
                while (!entryFound && mCsvReader.Read())
                {
                    mFileLinesRead++;

                    var rowData = new List<string>();

                    for (var columnIndex = 0; columnIndex < mCsvReader.Parser.Count; columnIndex++)
                    {
                        if (!mCsvReader.TryGetField<string>(columnIndex, out var fieldValue))
                        {
                            break;
                        }

                        rowData.Add(fieldValue);
                    }

                    if (rowData.Count == 0)
                    {
                        mFileBytesRead += 2;
                        continue;
                    }

                    var dataLine = string.Join(mDelimiter.ToString(), rowData);
                    mFileBytesRead += dataLine.Length + 2;

                    var validRow = rowData.Any(item => !string.IsNullOrWhiteSpace(item));

                    if (!validRow)
                    {
                        continue;
                    }

                    mCurrentEntry.Clear();

                    switch (DelimitedFileFormatCode)
                    {
                        case ProteinFileFormatCode.SequenceOnly:
                            if (rowData.Count >= 1)
                            {
                                entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, -1, -1, 0);
                            }
                            break;

                        case ProteinFileFormatCode.ProteinName_Sequence:
                            if (rowData.Count >= 2)
                            {
                                entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, 0, -1, 1);
                            }
                            break;

                        case ProteinFileFormatCode.ProteinName_Description_Sequence:
                            if (rowData.Count >= 3)
                            {
                                entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, 0, 1, 2);
                            }
                            break;

                        case ProteinFileFormatCode.ProteinName_Description_Hash_Sequence:
                            if (rowData.Count >= 4)
                            {
                                entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, 0, 1, 3);
                            }
                            break;

                        case ProteinFileFormatCode.UniqueID_Sequence:
                        case ProteinFileFormatCode.UniqueID_Sequence_Mass_NET:
                            if (rowData.Count >= 2)
                            {
                                // Only process the line if the first column is numeric (useful for skipping header lines)
                                // Also require that the sequence column is not a number
                                if (IsNumber(rowData[0]) && !IsNumber(rowData[1]))
                                {
                                    entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, -1, -1, 1);

                                    if (!entryFound)
                                        break;

                                    mCurrentEntry.UniqueID = int.TryParse(rowData[0], out var id) ? id : 0;

                                    if (rowData.Count >= 4)
                                    {
                                        if (double.TryParse(rowData[2], out var mass))
                                            mCurrentEntry.Mass = mass;

                                        if (float.TryParse(rowData[3], out var normalizedElutionTime))
                                            mCurrentEntry.NET = normalizedElutionTime;
                                    }
                                }
                            }
                            break;

                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                        case ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                            if (rowData.Count >= 3)
                            {
                                // Only process the line if the third column is numeric (useful for skipping header lines)
                                // Also require that the sequence column is not a number
                                if (IsNumber(rowData[2]) && !IsNumber(rowData[1]))
                                {
                                    entryFound = ParseNameDescriptionSequenceLine(dataLine, rowData, 0, -1, 1);

                                    if (!entryFound)
                                        break;

                                    mCurrentEntry.UniqueID = int.TryParse(rowData[2], out var id) ? id : 0;

                                    if (rowData.Count >= 5)
                                    {
                                        if (double.TryParse(rowData[3], out var mass))
                                            mCurrentEntry.Mass = mass;

                                        if (float.TryParse(rowData[4], out var normalizedElutionTime))
                                            mCurrentEntry.NET = normalizedElutionTime;

                                        if (rowData.Count >= 7)
                                        {
                                            if (float.TryParse(rowData[5], out var netStDev))
                                                mCurrentEntry.NETStDev = netStDev;

                                            if (float.TryParse(rowData[6], out var discriminantScore))
                                                mCurrentEntry.DiscriminantScore = discriminantScore;
                                        }
                                    }
                                }
                            }
                            break;

                        default:
                            throw new Exception("Unknown file format code: " + DelimitedFileFormatCode);
                    }

                    if (!entryFound)
                    {
                        mFileLineSkipCount++;
                    }
                }

                if (!entryFound)
                {
                    AdjustBytesReadForEOF();
                }
            }
            catch (Exception ex)
            {
                // Error reading the input file
                entryFound = false;

                Console.WriteLine("Exception in DelimitedProteinFileReader.ReadNextProteinEntry: " + ex.Message);
            }

            if (!entryFound)
            {
                mCurrentEntry.Clear();
            }

            return entryFound;
        }
    }
}
