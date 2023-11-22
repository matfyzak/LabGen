using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LabGen
{
    public abstract class LabyrintNode
    {
        protected string nodeCode;

        public abstract string GenerateTexTemplate();
    }

    public class  LabyrintFinishNode : LabyrintNode
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
            if (otherAnswers.Length != 2 || otherNodeCodes.Length != 3)
            {
                throw new ArgumentException { };
            }

            // FIXME shuffle answers first!


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

            \begin{{flushleft}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[0]}: {correctAnswer}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[1]}: {otherAnswers[0]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[2]}: {otherAnswers[1]}}}
            \end{{flushleft}}

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
                process.StartInfo.Arguments = $"-output-directory=output {texFilePath}";
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

            string nextLine = null;

            while (nextLine is not null)
            {
                string question = input.GetNextNonEmptyLine();
                string correctAnswer = input.GetNextNonEmptyLine();
                string[] otherAnswers = new string[answerCount - 1];

                for (int j = 0; j < answerCount - 1; j++)
                {
                    otherAnswers[j] = input.GetNextNonEmptyLine();
                }


                if(question is null || correctAnswer is null)
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

            return levels;
        }

        void LinkNodes(List<List<LabyrintNode>> levels)
        {
            return;
        }

        public List<List<LabyrintNode>> GenerateLabScheme(int[] args, List<ValiadtedDataFromInput> questionsAndAnswers, string removalDeadline)
        {
            ValidateArgs(args);
            List<List<LabyrintNode>> levels = AssignNodesToLevels(args, questionsAndAnswers, removalDeadline);
            LinkNodes(levels);

            return levels;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {

            /*string nodeCode = "AA";
            string question = "Jaká je Výfučí barva?";
            string[] answers = { "červená", "oranžová", "zelená" };
            string[] otherNodeCodes = { "VF", "DD", "FF" };
            string removalDeadline = "01.01.2100";
            ValiadtedDataFromInput queastionAndAnswers = new ValiadtedDataFromInput(question, answers);

            LabyrintStandardNode node = new LabyrintStandardNode(queastionAndAnswers, nodeCode, otherNodeCodes, removalDeadline);

            string texCode = node.GenerateTexTemplate();

            string filePath = nodeCode + ".tex";

            FileHandler.SaveTexToFile(texCode, filePath);

            PdfCompiler.CompileToPdf(filePath);*/

            string folderName = "input";
            string fileName = "test_input_correct.txt";

            List<ValiadtedDataFromInput> input = Inputreader.ReadInputFile(Path.Combine(folderName, fileName));
            Console.Write(input);
        }
    }
}

/*
 TODO:
   * SchemeGenerator - based on Interface
   * UniqueNodeCodeGenerator
   * UI
   * Better input handler (csv)
   * General count of answers
   * Visual output (graph or text file)
 */