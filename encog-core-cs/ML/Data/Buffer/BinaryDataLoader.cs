﻿using System;
using Encog.ML.Data.Buffer.CODEC;

namespace Encog.ML.Data.Buffer
{
    /// <summary>
    /// This class is used, together with a CODEC, to move data to/from the Encog
    /// binary training file format. The same Encog binary files can be used on all
    /// Encog platforms. CODEC's are used to import/export with other formats, such
    /// as CSV.
    /// </summary>
    public class BinaryDataLoader
    {
        /// <summary>
        /// The CODEC to use.
        /// </summary>
        private readonly IDataSetCODEC codec;

        /// <summary>
        /// Construct a loader with the specified CODEC. 
        /// </summary>
        /// <param name="codec">The codec to use.</param>
        public BinaryDataLoader(IDataSetCODEC codec)
        {
            this.codec = codec;
            Status = new NullStatusReportable();
        }

        /// <summary>
        /// Used to report the status.
        /// </summary>
        private IStatusReportable Status { get; set; }

        /// <summary>
        /// The CODEC that is being used.
        /// </summary>
        public IDataSetCODEC CODEC
        {
            get { return codec; }
        }

        /// <summary>
        /// Convert an external file format, such as CSV, to the Encog binary
        /// training format. 
        /// </summary>
        /// <param name="binaryFile">The binary file to create.</param>
        public void External2Binary(String binaryFile)
        {
            Status.Report(0, 0, "Importing to binary file: "
                                + binaryFile);

            var egb = new EncogEGBFile(binaryFile);

            egb.Create(codec.InputSize, codec.IdealSize);

            var input = new double[codec.InputSize];
            var ideal = new double[codec.IdealSize];

            codec.PrepareRead();

            int index = 3;
            int currentRecord = 0;
            int lastUpdate = 0;

            while (codec.Read(input, ideal))
            {
                egb.Write(input);
                egb.Write(ideal);

                index += input.Length;
                index += ideal.Length;
                currentRecord++;
                lastUpdate++;
                if (lastUpdate >= 10000)
                {
                    lastUpdate = 0;
                    Status.Report(0, currentRecord, "Importing...");
                }
            }

            egb.Close();
            codec.Close();
            Status.Report(0, 0, "Done importing to binary file: "
                                + binaryFile);
        }

        /// <summary>
        /// Convert an Encog binary file to an external form, such as CSV. 
        /// </summary>
        /// <param name="binaryFile">THe binary file to use.</param>
        public void Binary2External(String binaryFile)
        {
            Status.Report(0, 0, "Exporting binary file: " + binaryFile);

            var egb = new EncogEGBFile(binaryFile);
            egb.Open();

            codec.PrepareWrite(egb.NumberOfRecords, egb.InputCount,
                               egb.IdealCount);

            int inputCount = egb.InputCount;
            int idealCount = egb.IdealCount;

            var input = new double[inputCount];
            var ideal = new double[idealCount];

            int currentRecord = 0;
            int lastUpdate = 0;

            // now load the data
            for (int i = 0; i < egb.NumberOfRecords; i++)
            {
                for (int j = 0; j < inputCount; j++)
                {
                    input[j] = egb.Read();
                }

                for (int j = 0; j < idealCount; j++)
                {
                    ideal[j] = egb.Read();
                }

                codec.Write(input, ideal);

                currentRecord++;
                lastUpdate++;
                if (lastUpdate >= 10000)
                {
                    lastUpdate = 0;
                    Status.Report(egb.NumberOfRecords, currentRecord,
                                  "Exporting...");
                }
            }

            egb.Close();
            codec.Close();
            Status.Report(0, 0, "Done exporting binary file: "
                                + binaryFile);
        }
    }
}