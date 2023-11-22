using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LabGen
{
    public abstract class LabyrintNode
    {
        public string nodeCode;

        public abstract string GenerateTexTemplate();
    }

    public class LabyrintFinishNode : LabyrintNode
    {
        string message;

        public LabyrintFinishNode(string nodeCode, string message = "Dorazili jste do cíle. Gratulujeme! Vraťte se na objekt a nahlašte přítomnému organizátorovi svůj návrat, aby vám mohl zaznamenat váš čas.")
        {
            this.nodeCode = nodeCode;
            this.message = message;
        }

        public override string GenerateTexTemplate()
        {

            string template = $@"
            \documentclass[a4paper]{{article}}
            \usepackage[margin=2cm]{{geometry}}
            \usepackage{{{{fix - cm}}}}

            \begin{{document}}

            \pagestyle{{empty}}

            \begin{{center}}
                \textbf{{\fontsize{{3cm}}{{4cm}}\selectfont {nodeCode}}}
                \vspace{{2cm}}
            \end{{center}}

            \begin{{center}}
                \textbf{{\fontsize{{1.5cm}}{{2cm}}\selectfont {message}}}
                \vspace{{0.5cm}}
            \end{{center}}

            \end{{document}}
            ";

            return template;
        }
    }

    public class LabyrintStandardNode : LabyrintNode
    {
        string question;
        string correctAnswer;
        string[] otherAnswers;
        public string[] otherNodeCodes;
        string removalDeadline;

        public LabyrintStandardNode(ValiadtedDataFromInput questionAndAnswers, string nodeCode, string[] otherNodeCodes, string removalDeadline)
        {
            this.nodeCode = nodeCode;
            question = questionAndAnswers.question;
            correctAnswer = questionAndAnswers.correctAnswer;
            otherAnswers = questionAndAnswers.otherAnswers;
            this.otherNodeCodes = otherNodeCodes;
            this.removalDeadline = removalDeadline;

        }

        public override string GenerateTexTemplate()
        {
            const int answerCount = 3;

            if (otherAnswers.Length != (answerCount - 1) || otherNodeCodes.Length != answerCount)
            {
                throw new ArgumentException { };
            }

            Random random = new Random();
            int positionOfCorrectNswer = random.Next(answerCount);


            string template = $@"
            \documentclass[a4paper]{{article}}
            \usepackage[margin=2cm]{{geometry}}
            \usepackage{{{{fix - cm}}}}

            \begin{{document}}

            \pagestyle{{empty}}

            \begin{{center}}
                \textbf{{\fontsize{{3cm}}{{4cm}}\selectfont {nodeCode}}}
                \vspace{{2cm}}
            \end{{center}}

            \begin{{center}}
                \textbf{{\fontsize{{1.5cm}}{{2cm}}\selectfont {question}}}
                \vspace{{0.5cm}}
            \end{{center}}

            \begin{{flushleft}}";

            if (positionOfCorrectNswer == 0)
            {
                template += $@"\textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[0]}: {correctAnswer}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[1]}: {otherAnswers[0]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[2]}: {otherAnswers[1]}}}
                 ";
            }
            else if (positionOfCorrectNswer == 1)
            {
                template += $@"\textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[1]}: {otherAnswers[0]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[0]}: {correctAnswer}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[2]}: {otherAnswers[1]}}}
                 ";
            }
            else
            {
                template += $@"\textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[2]}: {otherAnswers[1]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[1]}: {otherAnswers[0]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[0]}: {correctAnswer}}}
                 ";
            }

            template += $@"\end{{flushleft}}

            \vfill

            \begin{{center}}
            Tento papír je součástí táborové hry v rámci tábora pořádaného MFF UK. Papír prosím neodstraňujte, bude odstraněn nejpozději do {removalDeadline}.
            \end{{center}}


            \end{{document}}
            ";

            return template;
        }
    }
    public static class FileHandler
    {
        public static void SaveTexToFile(string texCode, string filePath)
        {
            File.WriteAllText(filePath, texCode);
        }
    }

    public static class PdfCompiler
    {
        public static void CompileToPdf(string texFilePath)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "pdflatex";
                process.StartInfo.Arguments = $"-output-directory=out {texFilePath}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                if (!process.WaitForExit(1000))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();

                    Console.WriteLine(output);
                    Console.WriteLine(errors);

                    int exitCode = process.ExitCode;
                }
                else
                {
                    process.Kill();
                    throw new TimeoutException("Compilation to pdf took too long.");
                }
            }
        }
    }

    public struct ValiadtedDataFromInput
    {
        public readonly string question;
        public readonly string correctAnswer;
        public readonly string[] otherAnswers;

        public ValiadtedDataFromInput(string question, string correctAnswer, string[] otherAnswers)
        {
            this.question = question;
            this.correctAnswer = correctAnswer;
            this.otherAnswers = otherAnswers;
        }
    }

    public class InputAsLinesWrapper
    {
        string[] lines;
        int index;

        public InputAsLinesWrapper(string[] lines)
        {
            this.lines = lines;
            index = 0;
        }

        public string GetNextNonEmptyLine()
        {
            while (index < lines.Length)
            {
                string line = lines[index];
                index++;

                if (!string.IsNullOrWhiteSpace(line))
                {

                    return line;
                }
            }

            return null;
        }
    }

    public static class Inputreader
    {

        public static List<ValiadtedDataFromInput> ReadInputFile(string relativeFilePath)
        {
            // Ignores empty lines. Question must be followed by three answers, each on separate line. 

            const int answerCount = 3;
            InputAsLinesWrapper input = new InputAsLinesWrapper(File.ReadAllLines(relativeFilePath));

            List<ValiadtedDataFromInput> output = new List<ValiadtedDataFromInput>();

            while(true)
            {
                string question = input.GetNextNonEmptyLine();

                if(question == null)
                {
                    break;
                }   

                string correctAnswer = input.GetNextNonEmptyLine();
                string[] otherAnswers = new string[answerCount - 1];

                for (int j = 0; j < answerCount - 1; j++)
                {
                    otherAnswers[j] = input.GetNextNonEmptyLine();
                }


                if (correctAnswer is null)
                {
                    throw new FormatException("The input file format is seriously wrong.");
                }
                for (int k = 0; k < answerCount - 1; k++)
                {
                    if (otherAnswers[k] is null)
                    {
                        throw new FormatException("The input file format is seriously wrong.");
                    }
                }

                ValiadtedDataFromInput questionAndAnswers = new ValiadtedDataFromInput(question, correctAnswer, otherAnswers);
                output.Add(questionAndAnswers);
            } 

            return output;
        }
    }

    public class UniqueNodeCodeGenerator
    {
        private static Random random = new Random();
        private HashSet<string> generatedCodes = new HashSet<string>();

        private char GetRandomUpperCaseLetter()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int index = random.Next(alphabet.Length);

            return alphabet[index];
        }

        public string GenerateNextUniqueNodeCode()
        {
            string uniqueCode;

            do
            {
                char firstChar = GetRandomUpperCaseLetter();
                char secondChar = GetRandomUpperCaseLetter();

                uniqueCode = $"{firstChar}{secondChar}";
            } while (!generatedCodes.Add(uniqueCode));

            return uniqueCode;
        }
    }

    public interface ISchemeGenerator
    {
        public void ValidateArgs(int[] args);

        public List<List<LabyrintNode>> GenerateLabScheme(int[] args, List<ValiadtedDataFromInput> questionAndAnswers, string removalDeadline);
    }

    public class SimpleSchemeGenerator : ISchemeGenerator
    {

        private static Random random = new Random();

        public void ValidateArgs(int[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Simple scheme generator takes just one integer as an argument.");
            }
            else if (args[0] < 3 || args[0] > 20)
            {
                throw new ArgumentException("Parameter of Simple cheme generator must be an integer between 3 and 20.");
            }
        }

        List<List<LabyrintNode>> AssignNodesToLevels(int[] args, List<ValiadtedDataFromInput> questionsAndAnswers, string removalDeadline)
        {
            int levelSize = args[0];
            int remainingNumberOfQuestions = questionsAndAnswers.Count;
            int inputIndex = 0;

            UniqueNodeCodeGenerator codeGenerator = new UniqueNodeCodeGenerator();

            List<List<LabyrintNode>> levels = new List<List<LabyrintNode>>();

            while (remainingNumberOfQuestions > 0)
            {
                List<LabyrintNode> level = new List<LabyrintNode>();

                if (remainingNumberOfQuestions >= levelSize)
                {
                    remainingNumberOfQuestions -= levelSize;

                    for (int i = 0; i < levelSize; i++)
                    {
                        LabyrintNode node = new LabyrintStandardNode(questionsAndAnswers[inputIndex], codeGenerator.GenerateNextUniqueNodeCode(), null, removalDeadline);
                        inputIndex++;
                        level.Add(node);
                    }
                }
                else
                {
                    for (int i = 0; i < remainingNumberOfQuestions; i++)
                    {
                        LabyrintNode node = new LabyrintStandardNode(questionsAndAnswers[inputIndex], codeGenerator.GenerateNextUniqueNodeCode(), null, removalDeadline);
                        inputIndex++;
                        level.Add(node);
                    }

                    remainingNumberOfQuestions = 0;
                }

                levels.Add(level);
            }

            levels.Reverse(); //now first layer/level is on top

            List<LabyrintNode> lastLevel = new List<LabyrintNode>
            {
                new LabyrintFinishNode(codeGenerator.GenerateNextUniqueNodeCode())
            };
            levels.Add(lastLevel);

            return levels;
        }

        void LinkNodes(List<List<LabyrintNode>> levels)
        {
            const int answerCount = 3;

            for (int i = 0; i < levels.Count - 1; i++)
            {
                foreach (LabyrintNode node in levels[i])
                {
                    int index = random.Next(levels[i + 1].Count);

                    ((LabyrintStandardNode)node).otherNodeCodes = new string[answerCount];
                    ((LabyrintStandardNode)node).otherNodeCodes[0] = levels[i + 1][index].nodeCode;

                    List<int> usedIndexes = new List<int>(); 
                    usedIndexes.Add(levels[i].IndexOf(node));

                    for (int j = 1; j < answerCount; j++)
                    {

                        do 
                        {
                            index = random.Next(levels[i].Count);
                        }
                        while (usedIndexes.Contains(index));

                        usedIndexes.Add(index);
                        ((LabyrintStandardNode)node).otherNodeCodes[j] = levels[i][index].nodeCode;
                    }
                }
            }
        }

        public List<List<LabyrintNode>> GenerateLabScheme(int[] args, List<ValiadtedDataFromInput> questionsAndAnswers, string removalDeadline)
        {
            ValidateArgs(args);
            List<List<LabyrintNode>> levels = AssignNodesToLevels(args, questionsAndAnswers, removalDeadline);
            LinkNodes(levels);

            return levels;
        }
    }

    public static class FileGenerator
    {
        static void CreateFolderIfNotExists(string folderPath)
        {

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        static void DeleteFilesWithExtension(string folderPath, string extension)
        {
            string[] files = Directory.GetFiles(folderPath, $"*{extension}", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public static void GenerateTeXFilesAndPdfs(List<List<LabyrintNode>> input)
        {
            const string nameOfTexOutputFolder = "tex";
            const string nameOfPdfOutputFolder = "out";

            CreateFolderIfNotExists(nameOfTexOutputFolder);
            CreateFolderIfNotExists(nameOfPdfOutputFolder);

            string[] outFiles = Directory.GetFiles(nameOfPdfOutputFolder);
            string[] texFiles = Directory.GetFiles(nameOfTexOutputFolder);

            foreach (string file in outFiles)
            {
                File.Delete(file);
            }

            foreach (string file in texFiles)
            {
                File.Delete(file);
            }

            int level = 1;

            foreach (List<LabyrintNode> stage in input)
            {
                foreach (LabyrintNode node in stage)
                {
                    string texCode = node.GenerateTexTemplate();
                    string filePath = Path.Combine(nameOfTexOutputFolder, level.ToString() + "_" + node.nodeCode + ".tex");

                    FileHandler.SaveTexToFile(texCode, filePath);

                    PdfCompiler.CompileToPdf(filePath);
                }

                level++;
            }

            DeleteFilesWithExtension(nameOfPdfOutputFolder, ".aux");
            DeleteFilesWithExtension(nameOfPdfOutputFolder, ".log");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string inputFolderName = "input";
            string inputFileName = "test_input_correct.txt";

            List<ValiadtedDataFromInput> input = Inputreader.ReadInputFile(Path.Combine(inputFolderName, inputFileName));
            
            SimpleSchemeGenerator schemeGenerator = new SimpleSchemeGenerator();

            int levelSize = 4;
            string date = "01.01.2100";

            int[] wrappedLevelSize = new int[1];
            wrappedLevelSize[0] = levelSize;

            List<List<LabyrintNode>> labScheme = schemeGenerator.GenerateLabScheme(wrappedLevelSize, input, date);

            FileGenerator.GenerateTeXFilesAndPdfs(labScheme);
        }
    }
}

/*
 TODO:
   * UI
   * Better input handler (csv)
   * General count of answers
   * Visual output (graph or text file)
   * Divide this file into more files
 */