using nicesoon.Models;
using nicesoon.Services;
using nicesoon.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace nicesoon.ViewModels
{
    public class DiaryViewModel : BaseViewModel
    {
        private readonly DatabaseService _dbService;
        private readonly AuthService _authService;

        private AnxietyLevel? _selectedFilter;
        private string _searchText = "";

        public ObservableCollection<NightmareRecord> Nightmares { get; } = new();
        public ObservableCollection<NightmareRecord> FilteredNightmares { get; } = new();

        public ICommand LoadRecordsCommand { get; }
        public ICommand OpenRecordCommand { get; }
        public ICommand AddRecordCommand { get; }
        public ICommand EditRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand FilterByAnxietyCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand RefreshCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    FilterRecords();
            }
        }

        public AnxietyLevel? SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                    FilterRecords();
            }
        }

        public DiaryViewModel()
        {

            Nightmares.Add(new NightmareRecord
            {
                Title = "Оно",
                Content = "Мне снилось Оно. Оставь меня, Боб Грей.",
                RecordDate = DateTime.Now,
                RecordAnxietyLevel = AnxietyLevel.Low
            });

            Nightmares.Add(new NightmareRecord
            {
                Title = "Лестница",
                Content = "Падать было больно.",
                RecordDate = DateTime.Now.AddDays(-1),
                RecordAnxietyLevel = AnxietyLevel.High
            });

            AddRecordCommand = new Command(async () => await AddRecordAsync());
            OpenRecordCommand = new Command<NightmareRecord>(async (record) => await OpenRecordAsync(record));
            DeleteRecordCommand = new Command<NightmareRecord>(async (record) => await DeleteRecordAsync(record));

            RefreshCommand = new Command(() =>
            {
                OnPropertyChanged(nameof(Nightmares));
            });
            FilterRecords();
        }

        //public DiaryViewModel(DatabaseService dbService)
        //{
        //    _dbService = dbService;

        //    LoadRecordsCommand = new Command(async () => await LoadRecordsAsync());
        //    AddRecordCommand = new Command(async () => await AddRecordAsync());
        //    EditRecordCommand = new Command<NightmareRecord>(async (record) => await EditRecordAsync(record));
        //    DeleteRecordCommand = new Command<NightmareRecord>(async (record) => await DeleteRecordAsync(record));
        //    FilterByAnxietyCommand = new Command<AnxietyLevel?>(
        //        (level) => SelectedFilter = level == SelectedFilter ? null : level);
        //    ClearFilterCommand = new Command(() =>
        //    {
        //        SelectedFilter = null;
        //        SearchText = "";
        //    });
        //    RefreshCommand = new Command(async () => await LoadRecordsAsync());

        //    if (!_authService.IsAuthenticated) 
        //    {
        //        Task.Run(async () =>
        //        {
        //            await Shell.Current.GoToAsync("//login");
        //        });
        //    }

        //    LoadRecordsCommand.Execute(null);
        //}

        public async Task LoadRecordsAsync()
        {
            try
            {
                IsBusy = true;

                var records = await _dbService.GetAll<NightmareRecord>();

                Nightmares.Clear();
                foreach (var record in records)
                {
                    Nightmares.Add(record);
                }

                FilterRecords();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки записей: {ex.Message}");
            
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void FilterRecords()
        {
            FilteredNightmares.Clear();

            var filtered = Nightmares.AsEnumerable();

            if (SelectedFilter.HasValue)
            {
                filtered = filtered.Where(n => n.RecordAnxietyLevel == SelectedFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(n =>
                    (n.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (n.Content?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            filtered = filtered.OrderByDescending(n => n.RecordDate);

            foreach (var record in filtered)
            {
                FilteredNightmares.Add(record);
            }
        }

        public async Task AddRecordAsync()
        {
            var newRecord = new NightmareRecord
            {
                Id = Nightmares.Count + 1,
                Title = $"Сон от {DateTime.Now:HH:mm}",
                Content = "Опишите ваш сон здесь...",
                RecordDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                RecordAnxietyLevel = AnxietyLevel.Medium
            };
            Nightmares.Add(newRecord);
            FilterRecords();

            await OpenRecordAsync(newRecord);
        }

        public async Task OpenRecordAsync(NightmareRecord record)
        {
            if (record == null) return;

            var editViewModel = new EditNightmareViewModel(this, record);

            var editPage = new NewSoonPage(editViewModel);
            await Application.Current.MainPage.Navigation.PushAsync(editPage);
        }

        public async Task DeleteRecordAsync(NightmareRecord record)
        {
            if (record == null) return;

            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                "Удаление записи",
                $"Удалить запись \"{record.Title}\"?",
                "Удалить",
                "Отмена");

            if (confirmed)
            {
                Nightmares.Remove(record);
                FilterRecords();

                await Application.Current.MainPage.DisplayAlert(
                    "Успех", "Запись удалена", "OK");
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
