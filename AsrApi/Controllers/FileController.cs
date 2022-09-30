﻿using Microsoft.AspNetCore.Http;
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
        public IActionResult UploadFile()
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
                    postedFile.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                return StatusCode(200);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("delete")]
        public IActionResult DeleteFile()
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
                
                return StatusCode(200);
            }
            catch (Exception)
            {
                return StatusCode(500);
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
                    output = ExecuteWindowsCommand("cd /;ls");
                }

                return Ok(output);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }



        [HttpGet]
        [Route("test")]
        public string NewMethod()
        {
            return Environment.OSVersion.Platform.ToString();
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
