// This class can be used to open a Fasta file or delimited text file
//  with protein names and sequences and return each protein present
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

using System;
using System.IO;

[assembly: CLSCompliant(true)]

namespace ProteinFileReader
{
    /// <summary>
    /// Abstract base class for reading protein files
    /// </summary>
    public abstract class ProteinFileReaderBaseClass : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public ProteinFileReaderBaseClass()
        {
            InitializeLocalVariables();
        }

        #region "Structures"

        /// <summary>
        /// This structure is used both for Proteins and for Peptides
        /// Only Peptides from delimited text files use the Mass and NET fields
        /// </summary>
        /// <remarks></remarks>
        protected struct udtProteinEntryType
        {
            /// <summary>
            /// For fasta files, the header line, including the protein header start character; for Delimited files, the entire line
            /// </summary>
            public string HeaderLine;

            /// <summary>
            /// Aka the accession name of the protein
            /// </summary>
            public string Name;

            /// <summary>
            /// Protein description
            /// </summary>
            public string Description;

            /// <summary>
            /// Protein sequence
            /// </summary>
            public string Sequence;

            /// <summary>
            /// For delimited text files listing peptides, the UniqueID of the peptide sequence
            /// </summary>
            public int UniqueID;

            /// <summary>
            /// Protein mass
            /// </summary>
            public double Mass;

            /// <summary>
            /// Protein Normalized Elution Time
            /// </summary>
            public float NET;

            /// <summary>
            /// Standard deviation of the Normalized Elution Time
            /// </summary>
            public float NETStDev;

            /// <summary>
            /// Discriminant score
            /// </summary>
            public float DiscriminantScore;
        }

        #endregion

        #region "Classwide Variables"

        /// <summary>
        /// Current entry being read/evaluated
        /// </summary>
        protected udtProteinEntryType mCurrentEntry;

        /// <summary>
        /// Stream for reading the protein file
        /// </summary>
        protected StreamReader mProteinFileInputStream;

        private bool mFileOpen;

        /// <summary>
        /// Bytes read from the file
        /// </summary>
        protected long mFileBytesRead;

        /// <summary>
        /// Lines read from the file
        /// </summary>
        protected int mFileLinesRead;

        /// <summary>
        /// Lines skipped in the file
        /// </summary>
        protected int mFileLineSkipCount;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Unique entry ID
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Only used for delimited text files of peptides, with a eDelimitedFileFormatCode format that has a UniqueID column</remarks>
        public int EntryUniqueID
        {
            get { return mCurrentEntry.UniqueID; }
        }

        /// <summary>
        /// Number of lines read
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int LinesRead
        {
            get { return mFileLinesRead; }
        }

        /// <summary>
        /// Number of lines skipped due to having an invalid format
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int LineSkipCount
        {
            get { return mFileLineSkipCount; }
        }

        /// <summary>
        /// Peptide discriminant score
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideDiscriminantScore
        {
            get { return mCurrentEntry.DiscriminantScore; }
        }

        /// <summary>
        /// Peptide mass
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double PeptideMass
        {
            get { return mCurrentEntry.Mass; }
        }

        /// <summary>
        /// Peptide normalized elution time (NET)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideNET
        {
            get { return mCurrentEntry.NET; }
        }

        /// <summary>
        /// Standard deviation of peptide normalized elution time (NET)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideNETStDev
        {
            get { return mCurrentEntry.NETStDev; }
        }

        /// <summary>
        /// Protein name
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinName
        {
            get { return mCurrentEntry.Name; }
        }

        /// <summary>
        /// Protein description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinDescription
        {
            get { return mCurrentEntry.Description; }
        }

        /// <summary>
        /// Protein sequence
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinSequence
        {
            get { return mCurrentEntry.Sequence; }
        }

        /// <summary>
        /// Protein Name or Protein Name and Description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>If file format is eDelimitedFileFormatCode.SequenceOnly, returns the protein sequence</remarks>
        public virtual string HeaderLine
        {
            get
            {
                try
                {
                    return mCurrentEntry.HeaderLine;
                }
                catch (Exception)
                {
                    return mCurrentEntry.HeaderLine;
                }
            }
        }

        #endregion

        /// <summary>
        /// Close the data file
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool CloseFile()
        {
            var blnSuccess = false;

            try
            {
                if (mProteinFileInputStream != null)
                {
                    mProteinFileInputStream.Close();
                }
                mFileOpen = false;
                mFileBytesRead = 0;
                mFileLinesRead = 0;
                blnSuccess = true;
            }
            catch (Exception)
            {
                blnSuccess = false;
            }

            return blnSuccess;
        }

        /// <summary>
        /// Reset the fields of the protein entry to defaults
        /// </summary>
        /// <param name="udtProteinEntry"></param>
        protected void EraseProteinEntry(ref udtProteinEntryType udtProteinEntry)
        {
            udtProteinEntry.HeaderLine = string.Empty;
            udtProteinEntry.Name = string.Empty;
            udtProteinEntry.Description = string.Empty;
            udtProteinEntry.Sequence = string.Empty;
            udtProteinEntry.UniqueID = 0;
            udtProteinEntry.Mass = 0;
            udtProteinEntry.NET = 0;
            udtProteinEntry.NETStDev = 0;
            udtProteinEntry.DiscriminantScore = 0;
        }

        private void InitializeLocalVariables()
        {
            EraseProteinEntry(ref mCurrentEntry);
        }

        /// <summary>
        /// Open the text file
        /// </summary>
        /// <param name="strInputFilePath"></param>
        /// <returns>True if success, false if a problem</returns>
        /// <remarks></remarks>
        public virtual bool OpenFile(string strInputFilePath)
        {
            var blnSuccess = false;

            try
            {
                if (CloseFile())
                {
                    mProteinFileInputStream = new StreamReader(new FileStream(strInputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    blnSuccess = true;
                }
            }
            catch (IOException)
            {
                try
                {
                    // Try again, this time allowing for read/write access
                    mProteinFileInputStream = new StreamReader(new FileStream(strInputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    blnSuccess = true;
                }
                catch (Exception)
                {
                    blnSuccess = false;
                }
            }
            catch (Exception)
            {
                blnSuccess = false;
            }

            if (blnSuccess)
            {
                mFileOpen = true;
                mFileBytesRead = 0;
                mFileLinesRead = 0;
                mFileLineSkipCount = 0;
            }

            return blnSuccess;
        }

        /// <summary>
        /// Percent of the file that has been read
        /// </summary>
        /// <returns>Value between 0 and 100</returns>
        /// <remarks></remarks>
        public float PercentFileProcessed()
        {
            if (mFileOpen)
            {
                if (mProteinFileInputStream.BaseStream.Length > 0)
                {
                    return Convert.ToSingle(Math.Round((double)mFileBytesRead / mProteinFileInputStream.BaseStream.Length * 100, 2));
                }
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// Look for the next protein entry
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        /// <remarks></remarks>
        public abstract bool ReadNextProteinEntry();

        /// <summary>
        /// Properly close open file handles
        /// </summary>
        public void Dispose()
        {
            if (mFileOpen)
                CloseFile();
        }
    }
}
