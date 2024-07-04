using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Bionic_Converter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void documentSelectButton_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			dlg.DefaultExt = ".epub";
			dlg.Filter = "EPUB Files (*.epub)|*.epub";

			Nullable<bool> result = dlg.ShowDialog();
			if (result.HasValue && result.Value)
			{
				string fileName = System.IO.Path.GetFileName(dlg.FileName);
				string extractedPath = System.IO.Path.GetFullPath(dlg.FileName);
				File.Move(extractedPath, System.IO.Path.ChangeExtension(extractedPath, ".zip"));
				extractedPath = System.IO.Path.ChangeExtension(extractedPath, ".zip");
				string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(extractedPath) ?? "C:\\", fileName);
				string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
				ZipFile.ExtractToDirectory(extractedPath, tempPath, System.Text.Encoding.UTF8);

				//TODO Modify zip files
				if (!ConvertEpubFiles(tempPath)) return;

				ZipFile.CreateFromDirectory(tempPath, outputPath);

				if (Directory.Exists(tempPath))
				{
					Directory.Delete(tempPath, true);
				}
			}
		}

		static private bool ConvertEpubFiles(string fileLocation)
		{
			string contentPath = System.IO.Path.Combine(fileLocation, "OEBPS");
			Regex regex = new Regex(@"\b\w+\b");

			if (Directory.Exists(contentPath))
			{
				string[] xhtmlFiles = Directory.GetFiles(contentPath, "*.xhtml");
				XNamespace ns = "http://www.w3.org/1999/xhtml";

				foreach (string xhtmlFile in xhtmlFiles)
				{
					XDocument xhtml = XDocument.Load(xhtmlFile);

					foreach (XElement p in xhtml.Descendants(ns + "p"))
					{
						// Store the original text content
						string originalText = p.Value;

						// Split the original text content while preserving the whitespace
						var parts = Regex.Split(originalText, @"(\b\w+\b)");

						// Create a new list to build the new content
						List<object> newContent = new List<object>();

						foreach (var part in parts)
						{
							if (regex.IsMatch(part))
							{
								int halfLength = part.Length / 2;
								string firstHalf = part.Substring(0, halfLength);
								string secondHalf = part.Substring(halfLength);

								// Wrap only the first half of the word with <b> tags
								newContent.Add(new XElement(ns + "b", firstHalf));
								newContent.Add(secondHalf);
							}
							else
							{
								newContent.Add(part); // Preserve non-word parts (like whitespace)
							}
						}

						// Replace the old paragraph content with the new content
						p.ReplaceNodes(newContent);
					}

					xhtml.Save(xhtmlFile);
				}
			}
			return true;
		}

	}

}