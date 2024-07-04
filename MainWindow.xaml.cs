using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace Bionic_Converter
{
	public partial class MainWindow : Window
	{
		private readonly EpubConverter _epubConverter;
		private readonly FileProcessor _fileProcessor;

		public MainWindow()
		{
			InitializeComponent();
			_epubConverter = new EpubConverter();
			_fileProcessor = new FileProcessor();
		}

		private void documentSelectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var dlg = new Microsoft.Win32.OpenFileDialog
				{
					DefaultExt = ".epub",
					Filter = "EPUB Files (*.epub)|*.epub"
				};

				var result = dlg.ShowDialog();
				if (result.HasValue && result.Value)
				{
					processStatusText.Text = "Starting Conversion";
					string fileName = System.IO.Path.GetFileName(dlg.FileName);
					string extractedPath = _fileProcessor.PrepareFile(dlg.FileName);
					string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

					_fileProcessor.ExtractFile(extractedPath, tempPath);

					if (_epubConverter.ConvertEpubFiles(tempPath))
					{
						string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(extractedPath) ?? "C:\\", fileName);
						_fileProcessor.CreateFileFromDirectory(tempPath, outputPath);
						_fileProcessor.CleanUp(tempPath);
						processStatusText.Text = "Conversion Finished!";
					}
				}
			}
			catch (Exception ex)
			{
				processStatusText.Text = ex.Message;
			}
		}
	}

	public class FileProcessor
	{
		public string PrepareFile(string filePath)
		{
			string extractedPath = System.IO.Path.ChangeExtension(filePath, ".zip");
			File.Move(filePath, extractedPath);
			return extractedPath;
		}

		public void ExtractFile(string zipPath, string extractPath)
		{
			ZipFile.ExtractToDirectory(zipPath, extractPath, System.Text.Encoding.UTF8);
		}

		public void CreateFileFromDirectory(string sourcePath, string destinationPath)
		{
			ZipFile.CreateFromDirectory(sourcePath, destinationPath);
		}

		public void CleanUp(string tempPath)
		{
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
		}
	}

	public class EpubConverter
	{
		public bool ConvertEpubFiles(string fileLocation)
		{
			try
			{
				string contentPath = System.IO.Path.Combine(fileLocation, "OEBPS");
				var regex = new Regex(@"\b\w+\b");

				if (Directory.Exists(contentPath))
				{
					var xhtmlFiles = Directory.GetFiles(contentPath, "*.xhtml");
					XNamespace ns = "http://www.w3.org/1999/xhtml";

					foreach (var xhtmlFile in xhtmlFiles)
					{
						var xhtml = XDocument.Load(xhtmlFile);

						foreach (var p in xhtml.Descendants(ns + "p"))
						{
							string originalText = p.Value;
							var parts = Regex.Split(originalText, @"(\b\w+\b)");
							var newContent = new List<object>();

							foreach (var part in parts)
							{
								if (regex.IsMatch(part))
								{
									int halfLength = part.Length / 2;
									string firstHalf = part.Substring(0, halfLength);
									string secondHalf = part.Substring(halfLength);

									newContent.Add(new XElement(ns + "b", firstHalf));
									newContent.Add(secondHalf);
								}
								else
								{
									newContent.Add(part);
								}
							}

							p.ReplaceNodes(newContent);
						}

						xhtml.Save(xhtmlFile);
					}
				}
				return true;
			}
			catch (Exception)
			{
				// Handle the exception
				return false;
			}
		}
	}
}
