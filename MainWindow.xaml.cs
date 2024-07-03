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
				ZipFile.ExtractToDirectory(extractedPath, tempPath);

				//TODO Modify zip files
				//if (!ConvertEpubFiles(tempPath)) return;

				ZipFile.CreateFromDirectory(tempPath, outputPath);


				//TODO convert zip to epub

				if (Directory.Exists(tempPath))
				{
					Directory.Delete(tempPath, true);
				}
			}
		}

		static private bool ConvertEpubFiles(string fileLocation)
		{
			return true;
		}
	}
}