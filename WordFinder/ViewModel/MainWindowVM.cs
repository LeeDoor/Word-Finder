using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Commands;
using WordFinder.Model;

namespace WordFinder.ViewModel
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private FoldersReader reader;
        private string forbiddenWords = "split with 1 space";
        private string initFolder = "";
        private int maxProgressBar;
        private int valProgressBar;

        public string ForbiddenWords 
        {
            get => forbiddenWords;
            set
            {
                ForbiddenWords = value;
                OnPropertyChanged(nameof(ForbiddenWords));
            }
        }
        public string InitFolder
        {
            get => initFolder;
            set
            {
                initFolder = value;
                OnPropertyChanged(nameof(InitFolder));
            }
        }
        public int MaxProgressBar
        {
            get => maxProgressBar;
            set
            {
                maxProgressBar = value;
                OnPropertyChanged(nameof(MaxProgressBar));
            }
        }
        public int ValProgressBar
        {
            get => valProgressBar;
            set
            {
                valProgressBar = value;
                OnPropertyChanged(nameof(ValProgressBar));
            }
        }

        public DelegateCommand StartCommand { get; set; }

        public MainWindowVM()
        {
            reader = new();

            StartCommand = new DelegateCommand(() => reader.Start(this));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
