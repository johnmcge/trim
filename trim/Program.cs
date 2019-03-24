using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace trim
{
    class Program
    {
        enum trimType : int
        {
            deleteBegining = 1,
            deleteToEnd = 2,
        }

        public static string baseFileName;
        public static string fileExtension;
        public static string inputFileName;

        static void Main(string[] args)
        {
            // trim.exe file -b: trim the begining
            //      remove the first 15 seconds:
            //      ffmpeg -ss 00:00:15 -t 59:59:59 -i input.mp4 -acodec copy -vcodec copy output.mp4
            //
            // trim.exe file -e: trim the end
            //      remove everything after 1:00 
            //      ffmpeg -ss 00:00:00 -t 00:01:00 -i input.mp4 -acodec copy -vcodec copy output.mp4
            //
            
            string usageString = GetUsageString();
            string trimType = null;

            if (args.Length < 3)
            {
                Console.WriteLine("too few arguments.");
                Console.WriteLine("");
                Console.WriteLine(usageString);
                return;
            }
            else if (args[0] == "-help" || args[0] == "-h")
            {
                Console.WriteLine(usageString);
                return;
            }

            if (args[1] == "-b")
            {
                trimType = "deleteBegining";
            }
            else if (args[1] == "-e")
            {
                trimType = "deleteToEnd";
            }
            else
            {
                Console.WriteLine("Invalid command. Required: -b, -e ");
                Console.WriteLine("");
                Console.WriteLine(usageString);
                return;
            }

            inputFileName = args[0];

            int baseFileNameLength = inputFileName.LastIndexOf(".");

            if (baseFileNameLength > 0)
            {
                baseFileName = inputFileName.Substring(0, baseFileNameLength);
                fileExtension = inputFileName.Substring(baseFileNameLength, (inputFileName.Length - baseFileNameLength));
            }
            else
            {
                Console.WriteLine("Invalid file name, no extension found.");
                return;
            }

            if (String.Equals(fileExtension, ".mp4", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(fileExtension, ".ts", StringComparison.OrdinalIgnoreCase))
            {
                // fileExtension = "." + parts[1];
            }
            else
            {
                Console.WriteLine("Invalid file extension. Only .mp4 and .ts supported at this time");
                return;
            }


            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("File not found: " + inputFileName);
                return;
            };


            // outFile = parts[0] + "-trimmedFile." + parts[1];
            string outputFileName = baseFileName + "-trimmedFile" + fileExtension;

            Console.WriteLine("trimType = " + trimType);

            string timeParameter = getTimeParameter(args[2]);
            if (timeParameter == "-1")
            {
                Console.WriteLine("Invalid time parameter. Must be of the form: N, NN, N:NN, NN:NN, N:NN:NN, or NN:NN:NN");
                return;
            }

            StringBuilder commandText = new StringBuilder();

            commandText.Append(" -y -ss ");      // -y means overwrite files with same name!
            if (trimType == "deleteBegining")
            {
                commandText.Append(timeParameter);
                commandText.Append(" -t ");
                commandText.Append("59:59:59");
                commandText.Append(" -i ");
                commandText.Append(inputFileName);
                commandText.Append(" -acodec copy -vcodec copy ");
                commandText.Append(outputFileName);
            }
            else if (trimType == "deleteToEnd")
            {
                commandText.Append("00:00:00");
                commandText.Append(" -t ");
                commandText.Append(timeParameter);
                commandText.Append(" -i ");
                commandText.Append(inputFileName);
                commandText.Append(" -acodec copy -vcodec copy ");
                commandText.Append(outputFileName);
            }

            if (File.Exists(outputFileName))
            {
                try
                {
                    File.Delete(outputFileName);
                }
                catch
                {
                    Console.WriteLine(Environment.NewLine + "Error trying to delete existing outputFileName: " + outputFileName + Environment.NewLine);
                    return;
                }
            }


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("ffmpeg.exe" + commandText.ToString());

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = commandText.ToString();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }

            if (File.Exists(outputFileName))
            {
                try
                {
                    File.Delete(inputFileName);                 // delete the original file
                    File.Move(outputFileName, inputFileName);   // rename the newly trimmed file to name of original file
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error deleting input file and/or renaming trimmed file to original file name.");
                    Console.WriteLine("Exception String:" );
                    Console.WriteLine(e.ToString());
                };
            }
            else
            {
                Console.WriteLine(Environment.NewLine + "Something went wrong. Expected output file was not found: " + outputFileName + Environment.NewLine);
            }

        }

        static string getTimeParameter(string inputString)
        {
            // it must be of the form: N, NN, N:NN, NN:NN, N:NN:NN or NN:NN:NN
            // really should verify that N values can be cast to int32

            if (inputString.Length == 1)
                return "00:00:0" + inputString;

            if (inputString.Length == 2)
                return "00:00:" + inputString;


            string[] parts = inputString.Split(':');

            if (parts.Length == 2)
            {
                // we are expecting:  N:NN or NN:NN
                if (parts[1].Length != 2)
                    return "-1";

                if (parts[0].Length == 1)
                    return "00:0" + parts[0] + ":" + parts[1];

                if (parts[0].Length == 2)
                    return "00:" + parts[0] + ":" + parts[1];

                return "-1";
            }

            if (parts.Length == 3)
            {
                // we are expecting:  N:NN:NN or NN:NN:NN
                if (parts[1].Length != 2 || parts[2].Length != 2)
                    return "-1";

                if (parts[0].Length == 1)
                    return "0" + parts[0] + ":" + parts[1] + ":" + parts[2];

                if (parts[0].Length == 2)
                    return parts[0] + ":" + parts[1] + ":" + parts[2];
                
                return "-1";
            }
            
            return "-1";
        }


        static string GetUsageString()
        {
            StringBuilder sbUsage = new StringBuilder();

            sbUsage.Append(" trim.exe filename -b timeString" + Environment.NewLine);
            sbUsage.Append(" trim the begnining: delete part of movie between 00:00:00 and timeString");
            sbUsage.Append("\r\n");
            sbUsage.Append("\r\n");
            sbUsage.Append(" trim.exe filename -e timeString");
            sbUsage.Append("\r\n");
            sbUsage.Append(" trim the end: delete part of movie between timeString and 59:59:59");
            sbUsage.Append("\r\n");
            sbUsage.Append("\r\n");
            sbUsage.Append(" example timeStrings: 5 (5 seconds), 25 (25 seconds), 1:05 (1 min and 5 seconds)");
            sbUsage.Append("\r\n");
            sbUsage.Append("\r\n");

            return sbUsage.ToString();
        }
        


    }
}
