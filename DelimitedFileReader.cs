// This class can be used to open a delimited file with protein information and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in October 2004
//
// E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
// Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using System.Diagnostics;

namespace ProteinFileReader
{
    /// <summary>
    /// Class for reading tab-delimited protein files
    /// </summary>
    public class DelimitedFileReader : ProteinFileReaderBaseClass
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public DelimitedFileReader()
        {
            InitializeLocalVariables();
        }

        #region "Constants and Enums"

        /// <summary>
        /// Columns present in the delimited text file
        /// </summary>
        /// <remarks></remarks>
        public enum eDelimitedFileFormatCode
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
            UniqueID_Sequence_Mass_NET = 7
        }

        #endregion

        #region "Classwide Variables"

        private char mDelimiter;
        private eDelimitedFileFormatCode mDelimitedFileFormatCode;
        private bool mSkipFirstLine;
        private bool mFirstLineSkipped;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Delimiter between columns
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Default is tab</remarks>
        public char Delimiter
        {
            get { return mDelimiter; }
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
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public eDelimitedFileFormatCode DelimitedFileFormatCode
        {
            get { return mDelimitedFileFormatCode; }
            set { mDelimitedFileFormatCode = value; }
        }

        /// <summary>
        /// Protein Name or Protein Name and Description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>If file format is eDelimitedFileFormatCode.SequenceOnly, returns the protein sequence</remarks>
        public override string HeaderLine
        {
            get
            {
                try
                {
                    switch (mDelimitedFileFormatCode)
                    {
                        case eDelimitedFileFormatCode.SequenceOnly:
                            return mCurrentEntry.HeaderLine;
                        case eDelimitedFileFormatCode.ProteinName_Sequence:
                            return mCurrentEntry.Name;
                        case eDelimitedFileFormatCode.ProteinName_Description_Sequence:
                            if (!string.IsNullOrWhiteSpace(mCurrentEntry.Description))
                            {
                                return mCurrentEntry.Name + mDelimiter + mCurrentEntry.Description;
                            }
                            else
                            {
                                return mCurrentEntry.Name;
                            }
                        case eDelimitedFileFormatCode.UniqueID_Sequence:
                        case eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET:
                            return mCurrentEntry.Name;
                        case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                        case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                        case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                            return mCurrentEntry.Name + mDelimiter + mCurrentEntry.UniqueID;
                        default:
                            Debug.Assert(false, "Unknown file format code: " + mDelimitedFileFormatCode);
                            return mCurrentEntry.HeaderLine;
                    }
                }
                catch (Exception)
                {
                    return mCurrentEntry.HeaderLine;
                }
            }
        }

        /// <summary>
        /// Controls whether to skip the first line
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SkipFirstLine
        {
            get { return mSkipFirstLine; }
            set { mSkipFirstLine = value; }
        }

        #endregion

        private void InitializeLocalVariables()
        {
            mDelimiter = '\t';
            mDelimitedFileFormatCode = eDelimitedFileFormatCode.ProteinName_Description_Sequence;
        }

        /// <summary>
        /// Determine if the provided string can be parsed as a number
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        protected bool IsNumber(string strText)
        {
            double dblValue;
            return double.TryParse(strText, out dblValue);
        }

        /// <summary>
        /// Reads the next entry in delimited protein (or delimited peptide) file
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        /// <remarks>This function will update variable mFileLineSkipCount if any lines are skipped due to having an invalid format</remarks>
        public override bool ReadNextProteinEntry()
        {
            const int MAX_SPLIT_LINE_COUNT = 8;
            string strLineIn = null;
            string[] strSplitLine = null;
            char[] strSepChars = {mDelimiter};

            var blnEntryFound = false;

            blnEntryFound = false;
            mFileLineSkipCount = 0;

            if (mProteinFileInputStream != null)
            {
                try
                {
                    if (mSkipFirstLine & !mFirstLineSkipped)
                    {
                        mFirstLineSkipped = true;

                        if (!mProteinFileInputStream.EndOfStream)
                        {
                            strLineIn = mProteinFileInputStream.ReadLine();

                            if (!string.IsNullOrWhiteSpace(strLineIn))
                            {
                                mFileBytesRead += strLineIn.Length + 2;
                                mFileLinesRead += 1;
                            }
                        }
                    }

                    // Read the file until the next valid line is found
                    while (!blnEntryFound && !mProteinFileInputStream.EndOfStream)
                    {
                        strLineIn = mProteinFileInputStream.ReadLine();

                        if (!string.IsNullOrWhiteSpace(strLineIn))
                        {
                            mFileBytesRead += strLineIn.Length + 2;
                            mFileLinesRead += 1;

                            strLineIn = strLineIn.Trim();
                            strSplitLine = strLineIn.Split(strSepChars, MAX_SPLIT_LINE_COUNT);

                            EraseProteinEntry(ref mCurrentEntry);

                            switch (mDelimitedFileFormatCode)
                            {
                                case eDelimitedFileFormatCode.SequenceOnly:
                                    if (strSplitLine.Length >= 1)
                                    {
                                        // Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                        if (!IsNumber(strSplitLine[0]))
                                        {
                                            mCurrentEntry.HeaderLine = strLineIn;
                                            mCurrentEntry.Name = string.Empty;
                                            mCurrentEntry.Description = string.Empty;
                                            mCurrentEntry.Sequence = strSplitLine[0];
                                            blnEntryFound = true;
                                        }
                                    }

                                    break;
                                case eDelimitedFileFormatCode.ProteinName_Sequence:
                                    if (strSplitLine.Length >= 2)
                                    {
                                        // Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                        if (!IsNumber(strSplitLine[1]))
                                        {
                                            mCurrentEntry.HeaderLine = strLineIn;
                                            mCurrentEntry.Name = strSplitLine[0];
                                            mCurrentEntry.Description = string.Empty;
                                            mCurrentEntry.Sequence = strSplitLine[1];
                                            blnEntryFound = true;
                                        }
                                    }
                                    break;
                                case eDelimitedFileFormatCode.ProteinName_Description_Sequence:
                                    if (strSplitLine.Length >= 3)
                                    {
                                        // Only process the line if the sequence column is not a number (useful for handling incorrect file formats)
                                        if (!IsNumber(strSplitLine[2]))
                                        {
                                            mCurrentEntry.HeaderLine = strLineIn;
                                            mCurrentEntry.Name = strSplitLine[0];
                                            mCurrentEntry.Description = strSplitLine[1];
                                            mCurrentEntry.Sequence = strSplitLine[2];
                                            blnEntryFound = true;
                                        }
                                    }
                                    break;
                                case eDelimitedFileFormatCode.UniqueID_Sequence:
                                case eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET:
                                    if (strSplitLine.Length >= 2)
                                    {
                                        // Only process the line if the first column is numeric (useful for skipping header lines)
                                        // Also require that the sequence column is not a number
                                        if (IsNumber(strSplitLine[0]) & !IsNumber(strSplitLine[1]))
                                        {
                                            mCurrentEntry.HeaderLine = strLineIn;
                                            mCurrentEntry.Name = string.Empty;
                                            mCurrentEntry.Description = string.Empty;
                                            mCurrentEntry.Sequence = strSplitLine[1];
                                            try
                                            {
                                                mCurrentEntry.UniqueID = int.Parse(strSplitLine[0]);
                                            }
                                            catch (Exception)
                                            {
                                                mCurrentEntry.UniqueID = 0;
                                            }

                                            if (strSplitLine.Length >= 4)
                                            {
                                                try
                                                {
                                                    mCurrentEntry.Mass = double.Parse(strSplitLine[2]);
                                                }
                                                catch (Exception)
                                                {
                                                }

                                                try
                                                {
                                                    mCurrentEntry.NET = float.Parse(strSplitLine[3]);
                                                }
                                                catch (Exception)
                                                {
                                                }
                                            }
                                            blnEntryFound = true;
                                        }
                                    }
                                    break;
                                case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                                case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                                case eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                                    if (strSplitLine.Length >= 3)
                                    {
                                        // Only process the line if the third column is numeric (useful for skipping header lines)
                                        // Also require that the sequence column is not a number
                                        if (IsNumber(strSplitLine[2]) & !IsNumber(strSplitLine[1]))
                                        {
                                            mCurrentEntry.HeaderLine = strLineIn;
                                            mCurrentEntry.Name = strSplitLine[0];
                                            mCurrentEntry.Description = string.Empty;
                                            mCurrentEntry.Sequence = strSplitLine[1];
                                            try
                                            {
                                                mCurrentEntry.UniqueID = int.Parse(strSplitLine[2]);
                                            }
                                            catch (Exception)
                                            {
                                                mCurrentEntry.UniqueID = 0;
                                            }

                                            if (strSplitLine.Length >= 5)
                                            {
                                                try
                                                {
                                                    mCurrentEntry.Mass = double.Parse(strSplitLine[3]);
                                                    mCurrentEntry.NET = float.Parse(strSplitLine[4]);
                                                }
                                                catch (Exception)
                                                {
                                                }

                                                if (strSplitLine.Length >= 7)
                                                {
                                                    try
                                                    {
                                                        mCurrentEntry.NETStDev = float.Parse(strSplitLine[5]);
                                                        mCurrentEntry.DiscriminantScore = float.Parse(strSplitLine[6]);
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }
                                            }
                                            blnEntryFound = true;
                                        }
                                    }
                                    break;
                                default:
                                    Debug.Assert(false, "Unknown file format code: " + mDelimitedFileFormatCode);
                                    blnEntryFound = false;
                                    break;
                            }

                            if (!blnEntryFound)
                            {
                                mFileLineSkipCount += 1;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Error reading the input file
                    // Ignore any errors
                    blnEntryFound = false;
                }
            }

            if (!blnEntryFound)
            {
                EraseProteinEntry(ref mCurrentEntry);
            }

            return blnEntryFound;
        }

        /// <summary>
        /// Open the delimited text file
        /// </summary>
        /// <param name="strInputFilePath"></param>
        /// <returns>True if success, false if a problem</returns>
        /// <remarks></remarks>
        public override bool OpenFile(string strInputFilePath)
        {
            // Reset the first line tracking variable
            mFirstLineSkipped = false;

            // Call OpenFile in the base class
            return base.OpenFile(strInputFilePath);
        }
    }
}
