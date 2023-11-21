using System;
using System.Diagnostics;
using System.IO;

namespace LabGen
{
    public class LabyrintStandardNode
    {
        string nodeCode;
        string question;
        string[] answers;
        string[] otherNodeCodes;
        string removalDeadline;

        public LabyrintStandardNode(string nodeCode, string question, string[] answers, string[] otherNodeCodes, string removalDeadline)
        {
            this.nodeCode = nodeCode;
            this.question = question;
            this.answers = answers;
            this.otherNodeCodes = otherNodeCodes;
            this.removalDeadline = removalDeadline;
        }

        public string GenerateTexTemplate()
        {
            if (answers.Length != 3 || otherNodeCodes.Length != 3)
            {
                throw new ArgumentException { };
            }


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
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[0]}: {answers[0]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[1]}: {answers[1]}}} \\
                \vspace{{0.5cm}}
                \textbf{{\fontsize{{1cm}}{{2cm}}\selectfont {otherNodeCodes[2]}: {answers[2]}}}
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
                    throw new TimeoutException();
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {

            string nodeCode = "AA";
            string question = "Jaká je Výfučí barva?";
            string[] answers = { "červená", "oranžová", "zelená" };
            string[] otherNodeCodes = { "VF", "DD", "FF" };
            string removalDeadline = "01.01.2100";

            LabyrintStandardNode node = new LabyrintStandardNode(nodeCode, question, answers, otherNodeCodes, removalDeadline);

            string texCode = node.GenerateTexTemplate();

            string filePath = nodeCode + ".tex";

            FileHandler.SaveTexToFile(texCode, filePath);

            PdfCompiler.CompileToPdf(filePath);
        }
    }
}