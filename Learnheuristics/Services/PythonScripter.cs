using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Learnheuristics.Services {

    public class PythonScripter {

        //Permite ejecutar script's escritos en Python utilizando el interprete especificado en <path_to_interpreter>.
        public static List<string> Run(string path_to_script, Dictionary<string, object> arguments, string path_to_interpreter = @"C:\Python38\python.exe") {
            var output_lines = new List<string>();
            var this_process = Process.GetCurrentProcess();
            using (Process process = new Process()) {
                process.StartInfo.FileName = path_to_interpreter;
                process.StartInfo.Arguments = path_to_script;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string arguments_lines = string.Join('\n', arguments.Select(key_value => $"{key_value.Key}:{key_value.Value}"));
                process.StandardInput.WriteLine(arguments_lines);
                process.StandardInput.Close();
                var error = process.StandardError.ReadToEnd();
                var output = process.StandardOutput.ReadToEnd();
                output_lines = output.Split("\r\n").SkipLast(1).ToList();
                if (!string.IsNullOrEmpty(error))
                    output_lines.Add(error);
                process.WaitForExit();
            }
            return output_lines;
        }

    }

}