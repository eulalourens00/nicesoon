using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using nicesoon.Models;
namespace nicesoon.ViewModels
{
    public class EditNightmareViewModel : BaseViewModel
    {
        private readonly DiaryViewModel _parentViewModel;

        public NightmareRecord Record { get; private set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public AnxietyLevel AnxietyLevel { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SetAnxietyCommand { get; } 

        public EditNightmareViewModel(DiaryViewModel parentViewModel, NightmareRecord record)
        {
            _parentViewModel = parentViewModel;
            Record = record;

            Title = record.Title;
            Content = record.Content;
            AnxietyLevel = record.RecordAnxietyLevel;

            SaveCommand = new Command(Save);
            CancelCommand = new Command(async () =>
                await Application.Current.MainPage.Navigation.PopAsync());

            SetAnxietyCommand = new Command<AnxietyLevel>(level =>
            {
                AnxietyLevel = level;
                OnPropertyChanged(nameof(AnxietyLevel));
            });
        }

        private async void Save()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка", "Введите название сна", "OK");
                return;
            }

            Record.Title = Title;
            Record.Content = Content;
            Record.RecordAnxietyLevel = AnxietyLevel;

            await RefreshParentCollection();

            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task RefreshParentCollection()
        {
            var currentList = _parentViewModel.Nightmares.ToList();

            var index = currentList.FindIndex(r => r.Id == Record.Id);
            if (index != -1)
            {
                var updated = new NightmareRecord
                {
                    Id = Record.Id,
                    Title = Title,
                    Content = Content,
                    RecordAnxietyLevel = AnxietyLevel,
                    RecordDate = Record.RecordDate,
                    CreatedAt = Record.CreatedAt
                };

                currentList[index] = updated;

                _parentViewModel.Nightmares.Clear();
                foreach (var item in currentList)
                {
                    _parentViewModel.Nightmares.Add(item);
                }
            }
            _parentViewModel.FilterRecords();
        }
    }
}
