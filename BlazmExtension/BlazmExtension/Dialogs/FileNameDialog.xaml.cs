using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StageCoder
{
	public partial class FileNameDialog : Window
	{
		private const string DEFAULT_TEXT = "Enter a name for the component";
		

		public FileNameDialog()
		{
			InitializeComponent();

			Loaded += (s, e) =>
			{
				//Icon = BitmapFrame.Create(new Uri("pack://application:,,,/AddAnyFile;component/Resources/icon.png", UriKind.RelativeOrAbsolute));
				Title = "Save selection as a new component";

				Name.Focus();
				Name.CaretIndex = 0;
				Name.Text = DEFAULT_TEXT;
				Name.Select(0, Name.Text.Length);

				Name.PreviewKeyDown += (a, b) =>
				{
					if (b.Key == Key.Escape)
					{
						if (string.IsNullOrWhiteSpace(Name.Text) || Name.Text == DEFAULT_TEXT)
						{
							Close();
						}
						else
						{
							Name.Text = string.Empty;
						}
					}
					else if (Name.Text == DEFAULT_TEXT)
					{
						Name.Text = string.Empty;
						btnCreate.IsEnabled = true;
					}
				};

			};
		}

		public string Input => Name.Text.Trim();

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
