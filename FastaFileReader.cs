// This class can be used to open a Fasta file and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in January 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://panomics.pnl.gov/ or https://omics.pnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace ProteinFileReader
{
    /// <summary>
    /// Class for reading *.fasta files
    /// </summary>
    public sealed class FastaFileReader : ProteinFileReaderBaseClass
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public FastaFileReader()
        {
            InitializeLocalVariables();
        }

        /// <summary>
        /// Constructor that accepts a FASTA file path
        /// </summary>
        /// <param name="fastaFilePath"></param>
        public FastaFileReader(string fastaFilePath)
        {
            InitializeLocalVariables();
            OpenFile(fastaFilePath);
        }

        #region "Constants and Enums"

        /// <summary>
        /// Each protein description line in the Fasta file should start with a > symbol
        /// </summary>
        private const char PROTEIN_LINE_START_CHAR = '>';
        private const char PROTEIN_LINE_ACCESSION_TERMINATOR = ' ';

        #endregion

        #region "Classwide Variables"

        /// <summary>
        /// Character that indicates the start of a data line with protein name (accession) and optionally description
        /// </summary>
        /// <remarks>Typically a '>'</remarks>
        private char mProteinLineStartChar;

        /// <summary>
        /// Character that denotes the end of the protein name
        /// </summary>
        /// <remarks>Typically a space</remarks>
        private char mProteinLineAccessionEndChar;


        private string mCachedHeaderLine;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Header line (protein name and description), without the start character (&lt;)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string HeaderLine => GetHeaderLine(false);

        /// <summary>
        /// Character that precedes each header line (the line with the protein name and description)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char ProteinLineStartChar
        {
            get => mProteinLineStartChar;
            set
            {
                if (value != 0)
                {
                    mProteinLineStartChar = value;
                }
            }
        }

        /// <summary>
        /// The character that follows the protein name; always a space
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char ProteinLineAccessionEndChar
        {
            get => mProteinLineAccessionEndChar;
            set
            {
                if (value != 0)
                {
                    mProteinLineAccessionEndChar = value;
                }
            }
        }

        #endregion

        private string ExtractDescriptionFromHeader(string headerLine)
        {
            var description = headerLine;

            try
            {
                if (headerLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    headerLine = headerLine.TrimStart(mProteinLineStartChar).Trim();
                }

                var charIndex = headerLine.IndexOf(mProteinLineAccessionEndChar);
                if (charIndex > 0)
                {
                    description = headerLine.Substring(charIndex + 1).Trim();
                }
                else
                {
                    description = headerLine;
                }
            }
            catch (Exception)
            {
                // Ignore any errors
            }

            return description;
        }

        private string ExtractAccessionNameFromHeader(string headerLine)
        {
            // Note: headerLine should not start with the > character; it should have already been removed when the file was read
            // Look for mProteinLineAccessionEndChar in headerLine

            var accessionName = headerLine;

            try
            {
                if (headerLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    headerLine = headerLine.TrimStart(mProteinLineStartChar).Trim();
                }

                var charIndex = headerLine.IndexOf(mProteinLineAccessionEndChar);
                if (charIndex > 0)
                {
                    accessionName = headerLine.Substring(0, charIndex).Trim();
                }
                else
                {
                    accessionName = headerLine;
                }
            }
            catch (Exception)
            {
                // Ignore any errors
            }

            return accessionName;
        }

        /// <summary>
        /// Header line (protein name and description)
        /// </summary>
        /// <param name="includeStartChar">If true then include the start character (&lt;)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetHeaderLine(bool includeStartChar)
        {
            try
            {
                if (!includeStartChar && mCurrentEntry.HeaderLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    return mCurrentEntry.HeaderLine.TrimStart(mProteinLineStartChar).Trim();
                }
                return mCurrentEntry.HeaderLine;
            }
            catch (Exception)
            {
                return mCurrentEntry.HeaderLine;
            }
        }

        private void InitializeLocalVariables()
        {
            mProteinLineStartChar = PROTEIN_LINE_START_CHAR;
            mProteinLineAccessionEndChar = PROTEIN_LINE_ACCESSION_TERMINATOR;

            mCachedHeaderLine = string.Empty;
        }

        /// <summary>
        /// Reads the next entry in a Fasta file
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        /// <remarks></remarks>
        public override bool ReadNextProteinEntry()
        {
            mCurrentEntry.Clear();

            var proteinEntryFound = false;

            // This is always 0 for Fasta files
            mFileLineSkipCount = 0;

            if (mProteinFileInputStream == null)
                return false;

            try
            {
                while (!proteinEntryFound && !mProteinFileInputStream.EndOfStream)
                {
                    string lineIn;
                    if (!string.IsNullOrWhiteSpace(mCachedHeaderLine))
                    {
                        lineIn = string.Copy(mCachedHeaderLine);
                        mCachedHeaderLine = string.Empty;
                    }
                    else
                    {
                        lineIn = mProteinFileInputStream.ReadLine();
                        if (!string.IsNullOrWhiteSpace(lineIn))
                        {
                            mFileBytesRead += lineIn.Length + 2;
                            mFileLinesRead += 1;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(lineIn))
                        continue;

                    var dataLine = lineIn.Trim();

                    // See if lineIn starts with the protein header start character
                    if (!dataLine.StartsWith(mProteinLineStartChar.ToString()))
                        continue;

                    mCurrentEntry.HeaderLine = dataLine;
                    mCurrentEntry.Name = ExtractAccessionNameFromHeader(dataLine);
                    mCurrentEntry.Description = ExtractDescriptionFromHeader(dataLine);
                    mCurrentEntry.Sequence = string.Empty;
                    proteinEntryFound = true;

                    // Now continue reading until the next protein header start character is found
                    while (!mProteinFileInputStream.EndOfStream)
                    {
                        var lineIn2 = mProteinFileInputStream.ReadLine();

                        if (string.IsNullOrWhiteSpace(lineIn2))
                            continue;

                        mFileBytesRead += lineIn2.Length + 2;
                        mFileLinesRead += 1;

                        var dataLine2 = lineIn2.Trim();

                        if (dataLine2.StartsWith(mProteinLineStartChar.ToString()))
                        {
                            // Found the next protein entry
                            // Store in mCachedHeaderLine and jump out of the loop
                            mCachedHeaderLine = string.Copy(dataLine2);
                            break;
                        }

                        // dataLine2 has additional residues for the current protein
                        mCurrentEntry.Sequence += dataLine2;
                    }
                }
            }
            catch (Exception)
            {
                // Error reading the input file
                // Ignore any errors
                proteinEntryFound = false;
            }

            return proteinEntryFound;
        }
    }
}
