using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AsrApi.Controllers
{
    [Route("api/File")]
    [ApiController]
    public class FileController : ControllerBase
    {

        [HttpPost]
        [Route("upload")]
        public bool UploadFile()
        {
            try
            {
                string fileName = null;
                var httpRequest = HttpContext.Request;
                var postedFile = httpRequest.Form.Files["file"];
                if (postedFile != null)
                {
                    fileName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    var filePath = Path.Combine(folder, fileName);  
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(fileStream);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("delete")]
        public bool DeleteFile()
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
                if (Directory.Exists(folder))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                    {
                        directory.Delete(true);
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        [HttpGet]
        [Route("transcribe")]
        public IActionResult TranscribeAudio()
        {
            try
            {
                var output = "";
                if (IsLinux)
                {
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
                    output = ExecuteBashCommand("ls");
                }
                else
                {
                    //output = ExecuteWindowsCommand("cd /;ls");
                }

                return StatusCode(200, "विकिपीडिया:इण्टरनेट पर हिन्दी के साधन");
            }
            catch (Exception)
            {
                return StatusCode(500, false); ;
            }
        }



        [HttpGet]
        [Route("test")]
        public string NewMethod()
        {
            return "API works";
        }


        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static string ExecuteBashCommand(string command)
        {
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }

        private static string ExecuteWindowsCommand(string command)
        {
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }
}
