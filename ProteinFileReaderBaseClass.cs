// This class can be used to open a Fasta file or delimited text file
//  with protein names and sequences and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in October 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://panomics.pnl.gov/ or https://omics.pnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.IO.Compression;

// ReSharper disable UnusedMember.Global

[assembly: CLSCompliant(true)]

namespace ProteinFileReader
{
    /// <summary>
    /// Abstract base class for reading protein files
    /// </summary>
    public abstract class ProteinFileReaderBaseClass : IDisposable
    {
        private const double GZIP_PROGRESS_SCALING_FACTOR = 0.5;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ProteinFileReaderBaseClass()
        {
            mCurrentEntry = new ProteinInfo(string.Empty);
        }

        #region "Class wide Variables"

        /// <summary>
        /// Current entry being read/evaluated
        /// </summary>
        protected readonly ProteinInfo mCurrentEntry;

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

        /// <summary>
        /// Number of bytes in the input file
        /// </summary>
        protected long mFileSizeBytes;

        /// <summary>
        /// Set to true when reading a gzipped file
        /// </summary>
        protected bool mReadingGzipFile;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Unique entry ID
        /// </summary>
        /// <remarks>Only used for delimited text files of peptides, with a DelimitedFileFormatCode format that has a UniqueID column</remarks>
        public int EntryUniqueID => mCurrentEntry.UniqueID;

        /// <summary>
        /// Number of lines read
        /// </summary>
        public int LinesRead => mFileLinesRead;

        /// <summary>
        /// Number of lines skipped due to having an invalid format
        /// </summary>
        public int LineSkipCount => mFileLineSkipCount;

        /// <summary>
        /// Peptide discriminant score
        /// </summary>
        public float PeptideDiscriminantScore => mCurrentEntry.DiscriminantScore;

        /// <summary>
        /// Peptide mass
        /// </summary>
        public double PeptideMass => mCurrentEntry.Mass;

        /// <summary>
        /// Peptide normalized elution time (NET)
        /// </summary>
        public float PeptideNET => mCurrentEntry.NET;

        /// <summary>
        /// Standard deviation of peptide normalized elution time (NET)
        /// </summary>
        public float PeptideNETStDev => mCurrentEntry.NETStDev;

        /// <summary>
        /// Protein name
        /// </summary>
        public string ProteinName => mCurrentEntry.Name;

        /// <summary>
        /// Protein description
        /// </summary>
        public string ProteinDescription => mCurrentEntry.Description;

        /// <summary>
        /// Protein sequence
        /// </summary>
        public string ProteinSequence => mCurrentEntry.Sequence;

        /// <summary>
        /// Protein Name or Protein Name and Description
        /// </summary>
        /// <remarks>If file format is DelimitedFileFormatCode.SequenceOnly, returns the protein sequence</remarks>
        public virtual string HeaderLine => mCurrentEntry.HeaderLine;

        #endregion

        /// <summary>
        /// Call this method after all proteins have been read from the input file
        /// </summary>
        protected void AdjustBytesReadForEOF()
        {
            // Update bytes read to assure that PercentFileProcessed() reports 100
            if (mReadingGzipFile)
            {
                mFileBytesRead = (long)(mFileSizeBytes / GZIP_PROGRESS_SCALING_FACTOR);
            }
            else
            {
                mFileBytesRead = mFileSizeBytes;
            }
        }

        /// <summary>
        /// Close the data file
        /// </summary>
        public bool CloseFile()
        {
            try
            {
                mProteinFileInputStream?.Close();
                mFileOpen = false;
                ResetTrackingVariables();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Open the text file
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns>True if success, false if a problem</returns>
        public virtual bool OpenFile(string inputFilePath)
        {
            try
            {
                if (!CloseFile())
                    return false;

                ResetTrackingVariables();

                if (string.IsNullOrWhiteSpace(inputFilePath))
                    return false;

                Stream fileStream;
                if (Path.HasExtension(inputFilePath) &&
                    Path.GetExtension(inputFilePath).Equals(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    var gzipFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    mFileSizeBytes = gzipFileStream.Length;

                    fileStream = new GZipStream(gzipFileStream, CompressionMode.Decompress);
                    mReadingGzipFile = true;
                }
                else
                {
                    fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    mFileSizeBytes = fileStream.Length;
                }

                mProteinFileInputStream = new StreamReader(fileStream);
                mFileOpen = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Percent of the file that has been read
        /// </summary>
        /// <returns>Value between 0 and 100</returns>
        public float PercentFileProcessed()
        {
            if (!mFileOpen)
                return 0;

            if (mFileSizeBytes == 0)
                return 0;

            double scalingFactor;
            if (mReadingGzipFile)
            {
                // Scale the number of bytes down by 50%; this is only an estimate of the compression ratio
                scalingFactor = GZIP_PROGRESS_SCALING_FACTOR;
            }
            else
            {
                scalingFactor = 1.0;
            }

            return Convert.ToSingle(Math.Round(scalingFactor * mFileBytesRead / mFileSizeBytes * 100, 2));
        }

        /// <summary>
        /// Look for the next protein entry
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        public abstract bool ReadNextProteinEntry();

        /// <summary>
        /// Reset variables that track bytes and lines read,
        /// input file size, and whether or not we're reading a gzipped file
        /// </summary>
        protected void ResetTrackingVariables()
        {
            mFileBytesRead = 0;
            mFileLinesRead = 0;
            mFileSizeBytes = 0;
            mReadingGzipFile = false;
        }

        /// <summary>
        /// Properly close open file handles
        /// </summary>
        public void Dispose()
        {
            if (mFileOpen)
            {
                CloseFile();
            }
        }
    }
}
