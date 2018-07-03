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
            StringBuilder commandText = new StringBuilder();

            if (args.Length < 3)
            {
                Console.WriteLine("too few arguments.");
                Console.WriteLine("");
                Console.WriteLine(usageString);
                return;
            }
            else if (args[0] == "-help")
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
            else if (args[1] == "-t")
            {
                trimType += "Test";
            }
            else
            {
                Console.WriteLine("Invalid command. Required: -b, -e, -t");
                Console.WriteLine("");
                Console.WriteLine(usageString);
                return;
            }

            string inputFileName = args[0];

            string[] parts = args[0].Split('.');

            string baseFileName = parts[0];
            string fileExtension = "";
            // check if extension exists, if so, store for use vs assuming mp4

            if (parts.Length > 1)
            {
                if (String.Equals(parts[1].ToString(), "mp4", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "." + parts[1];
                }
                else
                {
                    Console.WriteLine("Invalid file extension. Only mp4 supported at this time");
                    return;
                }
            }
            else
                fileExtension = ".mp4";

            inputFileName = baseFileName + fileExtension;


            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("File not found: " + inputFileName);
                return;
            };

            string outputFileName = genOutPutFileName(inputFileName);

            // need to add validation for args[2]
            // it must be of the form: N or NN or N:NN or NN:NN or N:NN:NN etc.

            Console.WriteLine("trimType = " + trimType);

            string timeParameter = getTimeParameter(args[2]);
            if (timeParameter == "-1")
            {
                Console.WriteLine("Invalid time parameter. Must be of the form: N, NN, N:NN, NN:NN, N:NN:NN, or NN:NN:NN");
                return;
            }

            commandText.Append("-y -ss ");      // -y means overwrite files with same name!
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
            else
            {

            }

            if (File.Exists(outputFileName))
            {
                try
                {
                    File.Delete(outputFileName);
                }
                catch { };
            }



            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("ffmpeg.exe " + commandText.ToString());

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
                    File.Delete(inputFileName);
                    File.Move(outputFileName, inputFileName);
                }
                catch(Exception e)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Exception String:" );
                    Console.WriteLine(e.ToString());
                };

            }


        }

        static string getTimeParameter(string inputString)
        {
            // need to add validation for args[2]
            // it must be of the form: NN or N:NN or NN:NN or N:NNNN or NN:NN:NN

            // currently only works for the NN form

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

        static string genOutPutFileName(string inputFileName)
        {
            // assumes no . in file name other than extension delimeter

            String[] parts = inputFileName.Split('.');
            string outFile = "";

            if (parts.Length == 2)
                outFile = parts[0] + "-trimmedFile." + parts[1];
            else
                outFile = parts[0] + "-trimmedFile." + parts[(parts.Length - 1)];


            return outFile;
        }


        static string GetUsageString()
        {
            StringBuilder sbUsage = new StringBuilder();

            sbUsage.Append(" trim.exe filename -b timeString");
            sbUsage.Append("\r\n");
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
